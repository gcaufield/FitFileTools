using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dynastream.Fit;

namespace FitFileTools.Tools.MergeTool
{
    public class ZwiftMergeTool
    {
        public enum FileGenerator
        {
            Garmin,
            VirtualPlatform
        }

        private FitIndexer _zwiftIndex;
        private readonly FitReader _zwiftFile;
        private readonly FitReader _garminFile;
        private readonly Stream _outStream;
        private readonly FileGenerator _generator;

        private Encode _encode;
        private long? _tsOffset = null;

        private Decode _zwiftDecode;
        private LapManager _lapManager = new LapManager();

        public ZwiftMergeTool(Stream zwiftFile, Stream garminFile, Stream outStream, FileGenerator generator)
        {
            _zwiftFile = new FitReader(zwiftFile);
            _garminFile = new FitReader(garminFile);
            _outStream = outStream;
            _generator = generator;
        }

        public Task MergeFilesAsync()
        {
            return Task.Factory.StartNew(MergeFiles);
        }

        private void MergeFiles()
        {
            lock (_zwiftFile)
            {
                ResetStreams();
                FindDelay();

                ResetStreams();
                _zwiftIndex = new FitIndexer(_zwiftFile, new HashSet<ushort>{MesgNum.FileId, MesgNum.DeviceInfo, MesgNum.Session});

                _encode = new Encode(_outStream, ProtocolVersion.V20);
                try
                {
                    ResetStreams();

                    // Read the header from both files to ensure we can step through the messages
                    MergeMessages();
                }
                finally
                {
                    _encode.Close();
                    _encode = null;
                    ResetStreams();
                }
            }
        }

        private void MergeRecord(Mesg msg)
        {
            var zwiftMessage = GetZwiftRecord(
                (uint) msg.GetFieldValue(RecordMesg.FieldDefNum.Timestamp));

            // If the file contains enhanced speed remove it
            msg.RemoveField(msg.GetField(RecordMesg.FieldDefNum.EnhancedSpeed));
            msg.RemoveField(msg.GetField(RecordMesg.FieldDefNum.EnhancedAltitude));

            msg.RemoveField(msg.GetField(RecordMesg.FieldDefNum.Speed));
            msg.RemoveField(msg.GetField(RecordMesg.FieldDefNum.Distance));
            msg.RemoveField(msg.GetField(RecordMesg.FieldDefNum.Altitude));

            if (zwiftMessage != null)
            {
                FitHelpers.CopyField(zwiftMessage, msg, RecordMesg.FieldDefNum.PositionLat);
                FitHelpers.CopyField(zwiftMessage, msg, RecordMesg.FieldDefNum.PositionLong);
                FitHelpers.CopyField(zwiftMessage, msg, RecordMesg.FieldDefNum.EnhancedAltitude);
                FitHelpers.CopyField(zwiftMessage, msg, RecordMesg.FieldDefNum.EnhancedSpeed);
                FitHelpers.CopyField(zwiftMessage, msg, RecordMesg.FieldDefNum.Distance);


                //msg.SetFieldValue(RecordMesg.FieldDefNum.Speed, zwiftMessage.GetFieldValue(RecordMesg.FieldDefNum.Speed));
                //msg.SetFieldValue(RecordMesg.FieldDefNum.Distance, zwiftMessage.GetFieldValue(RecordMesg.FieldDefNum.Distance));
                _lapManager.OnRecordMesg(msg);
            }
        }
        private void FindDelay()
        {
            var zwiftRecords = _zwiftFile.ReadMesgs(m => m.Num == MesgNum.Record).ToArray();
            var garminRecords = _garminFile.ReadMesgs(m => m.Num == MesgNum.Record).ToArray();

            Mesg[] keyList = null;
            Mesg[] shiftList = null;

            var zwiftSpan = GetTimespan(zwiftRecords);
            var garminSpan = GetTimespan(garminRecords);

            if (zwiftSpan.TotalSeconds > garminSpan.TotalSeconds)
            {
                keyList = zwiftRecords;
                shiftList = garminRecords;
            }
            else
            {
                keyList = garminRecords;
                shiftList = zwiftRecords;
            }

            uint diffScore = uint.MaxValue;
            int possibleShifts = (int)Math.Abs(garminSpan.TotalSeconds - zwiftSpan.TotalSeconds);
            int offset = 0;

            for (int i = 0; i < possibleShifts; i++)
            {
                var newDiffScore = GetDiffScore(keyList, shiftList, i, diffScore);

                if (newDiffScore < diffScore)
                {
                    diffScore = newDiffScore;
                    offset = i;
                }
            }

            _tsOffset = (uint) keyList[offset].GetFieldValue(RecordMesg.FieldDefNum.Timestamp) -
                        (uint) shiftList[0].GetFieldValue(RecordMesg.FieldDefNum.Timestamp);
        }

        private TimeSpan GetTimespan(Mesg[] records)
        {
            uint first = (uint)records[0].GetFieldValue(RecordMesg.FieldDefNum.Timestamp);
            uint last = (uint)records[^1].GetFieldValue(RecordMesg.FieldDefNum.Timestamp);

            return new TimeSpan((last - first) * TimeSpan.TicksPerSecond);
        }

        private uint GetDiffScore(Mesg[] keyList, Mesg[] shiftList, int shift, uint stopScore)
        {
            uint absScore = 0;

            for (int i = 0; i < shiftList.Length; i++)
            {
                if((i + shift) >= keyList.Length) break;
                var keyHr = (byte?)keyList[i + shift].GetFieldValue(RecordMesg.FieldDefNum.HeartRate);
                var shiftHr = (byte?)shiftList[i].GetFieldValue(RecordMesg.FieldDefNum.HeartRate);
                if (!shiftHr.HasValue || !keyHr.HasValue)
                {
                    continue;
                }

                absScore += (uint)Math.Abs(keyHr.Value - shiftHr.Value);

                if (absScore > stopScore)
                {
                    return stopScore;
                }
            }

            return absScore;
        }


        private Mesg GetZwiftRecord(uint timestamp)
        {
            Mesg record;

            if (_tsOffset == null)
            {
                record = _zwiftFile.ReadNextMesg((mesg => mesg.Num == MesgNum.Record));
                uint zwiftTs = (uint)record.GetFieldValue(RecordMesg.FieldDefNum.Timestamp);

                _tsOffset = (long)zwiftTs - timestamp;
            }
            else
            {
                record = _zwiftFile.ReadNextMesg(mesg =>
                {
                    if (mesg.Num == MesgNum.Record)
                    {
                        var zwiftTs = (uint)mesg.GetFieldValue(RecordMesg.FieldDefNum.Timestamp);

                        if (zwiftTs + _tsOffset >= timestamp)
                        {
                            return true;
                        }
                    }

                    return false;
                });
            }

            return record;
        }

        private void MergeMessages()
        {
            var mesg = _garminFile.ReadNextMesg();
            while (mesg != null)
            {
                if (KeepMessage(mesg))
                {
                    if (IsMergeagle(mesg))
                    {
                        MergeMessage(mesg);
                    }

                    _encode.Write(mesg);
                }

                mesg = _garminFile.ReadNextMesg();
            }
        }

        private bool KeepMessage(Mesg mesg)
        {
            switch (mesg.Num)
            {
                case 312:
                case 313:
                //case MesgNum.WorkoutStep:
                    /* Remove private messages that interfere with ability to merge relevant data */
                    return false;

                default:
                    return true;
            }
        }

        private bool IsMergeagle(Mesg lastMesg)
        {
            switch (lastMesg.Num)
            {
                case MesgNum.FileId:
                case MesgNum.DeviceInfo:
                case MesgNum.Sport:
                case MesgNum.Record:
                case MesgNum.Session:
                case MesgNum.Lap:
                    return true;
                default:
                    return false;
            }
        }

        private void MergeMessage(Mesg lastMesg)
        {
            switch (lastMesg.Num)
            {
                case MesgNum.FileId:
                    FixupFileIdMesg(lastMesg);
                    break;
                case MesgNum.DeviceInfo:
                    FixupDeviceInfoMesg(lastMesg);
                    break;
                case MesgNum.Sport:
                    FixupSportMesg(lastMesg);
                    break;
                case MesgNum.Record:
                    MergeRecord(lastMesg);
                    break;
                case MesgNum.Session:
                    MergeSession(lastMesg);
                    break;
                case MesgNum.Lap:
                    MergeLap(lastMesg);
                    break;
            }
        }

        private void FixupFileIdMesg(Mesg lastMesg)
        {
            if (_generator == FileGenerator.VirtualPlatform)
            {
                var zwiftMessage = _zwiftIndex.Get(MesgNum.FileId);
                if (zwiftMessage == null) return;

                var fieldsToCopy = new List<byte> {
                    FileIdMesg.FieldDefNum.Product,
                    FileIdMesg.FieldDefNum.SerialNumber,
                    FileIdMesg.FieldDefNum.Manufacturer,
                };

                foreach (var field in fieldsToCopy)
                {
                    FitHelpers.CopyField(zwiftMessage, lastMesg, field);
                }
            }
        }

        private void FixupDeviceInfoMesg(Mesg lastMesg)
        {
            if (_generator == FileGenerator.VirtualPlatform)
            {
                var zwiftMessage = _zwiftIndex.Get(MesgNum.DeviceInfo);
                if (zwiftMessage == null) return;

                var fieldsToCopy = new List<byte> {
                    DeviceInfoMesg.FieldDefNum.Product,
                    DeviceInfoMesg.FieldDefNum.Product,
                    DeviceInfoMesg.FieldDefNum.SoftwareVersion,
                };

                foreach (var field in fieldsToCopy)
                {
                    FitHelpers.CopyField(zwiftMessage, lastMesg, field);
                }
            }
        }

        private void MergeLap(Mesg lastMesg)
        {
            _lapManager.OnLapMesg(lastMesg);
        }

        private void MergeSession(Mesg session)
        {
            _lapManager.OnSessionMesg(session);
            session.SetFieldValue(SessionMesg.FieldDefNum.SubSport, SubSport.VirtualActivity);
        }

        private void FixupSportMesg(Mesg lastMesg)
        {
            lastMesg.SetFieldValue(SportMesg.FieldDefNum.SubSport, SubSport.VirtualActivity);
        }

        private void ResetStreams()
        {
            lock (_zwiftFile)
            {
                _zwiftFile.Reset();
                _garminFile.Reset();
            }
        }
    }
}
