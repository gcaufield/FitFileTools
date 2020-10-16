using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Dynastream.Fit;

namespace FitFileTools.Tools.MergeTool
{
    class FitReader
    {
        private Stream _readStream;
        private Decode _decode;
        private Mesg _lastMesg;

        public FitReader(Stream readStream)
        {
            _readStream = readStream;
            Reset();
        }

        public void Reset()
        {
            _readStream.Seek( 0, SeekOrigin.Begin);
            Header h = new Header(_readStream);
            InitDecode();
        }

        public Mesg ReadNextMesg()
        {
            while(_readStream.Position < (_readStream.Length - 2))
            {
                _decode.DecodeNextMessage(_readStream);
                if (_lastMesg != null)
                {
                    var temp = _lastMesg;
                    temp.RemoveExpandedFields();
                    _lastMesg = null;
                    return temp;
                }
            }

            return null;
        }

        public IEnumerable<Mesg> ReadMesgs(Func<Mesg, bool> matcher)
        {
            var mesg = ReadNextMesg(matcher);
            while (mesg != null)
            {
                yield return mesg;
                mesg = ReadNextMesg(matcher);
            }
        }

        public Mesg ReadNextMesg(Func<Mesg, bool> matcher)
        {
            var temp = ReadNextMesg();
            while (temp != null)
            {
                if (matcher(temp))
                {
                    return temp;
                }

                temp = ReadNextMesg();
            }

            return null;
        }

        private void InitDecode()
        {
            _decode = new Decode();
            _decode.MesgEvent += (sender, args) => { _lastMesg = args.mesg; };
        }
    }

    public class ZwiftMergeTool
    {
        private readonly FitReader _zwiftFile;
        private readonly FitReader _garminFile;

        private Encode _encode;
        private long? _tsOffset = 0;

        private Decode _zwiftDecode;

        public ZwiftMergeTool(Stream zwiftFile, Stream garminFile)
        {
            _zwiftFile = new FitReader(zwiftFile);
            _garminFile = new FitReader(garminFile);
        }

        public Task MergeFilesAsync(Stream stream)
        {
            return Task.Factory.StartNew(() => {MergeFiles(stream);});
        }

        private void MergeFiles(Stream outStream)
        {
            lock (_zwiftFile)
            {
                ResetStreams();
                FindDelay();

                _encode = new Encode(outStream, ProtocolVersion.V20);
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

            outStream.Seek(0, SeekOrigin.Begin);
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
                var keyHr = (byte)keyList[i + shift].GetFieldValue(RecordMesg.FieldDefNum.HeartRate);
                var shiftHr = (byte)shiftList[i].GetFieldValue(RecordMesg.FieldDefNum.HeartRate);
                absScore += (uint)Math.Abs(keyHr - shiftHr);

                if (absScore > stopScore)
                {
                    return stopScore;
                }
            }

            return absScore;
        }

        private void MergeRecord(Mesg msg)
        {
            var zwiftMessage = GetZwiftRecord(
                (uint) msg.GetFieldValue(RecordMesg.FieldDefNum.Timestamp));

            if (zwiftMessage == null)
            {
                // No zwift record
                return;
            }

            // If the file contains enhanced speed remove it
            msg.RemoveField(msg.GetField(RecordMesg.FieldDefNum.EnhancedSpeed));

            msg.SetField(zwiftMessage.GetField(RecordMesg.FieldDefNum.PositionLat));
            msg.SetField(zwiftMessage.GetField(RecordMesg.FieldDefNum.PositionLong));
            msg.SetField(zwiftMessage.GetField(RecordMesg.FieldDefNum.Altitude));
            msg.SetField(zwiftMessage.GetField(RecordMesg.FieldDefNum.Speed));
            msg.SetField(zwiftMessage.GetField(RecordMesg.FieldDefNum.Distance));
        }

        private Mesg GetZwiftRecord(uint timestamp)
        {
            var record = _zwiftFile.ReadNextMesg(mesg =>
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

            return record;
        }

        private void MergeMessages()
        {
            var mesg = _garminFile.ReadNextMesg();
            while (mesg != null)
            {
                if (IsMergeagle(mesg))
                {
                    MergeMessage(mesg);
                }

                _encode.Write(mesg);

                mesg = _garminFile.ReadNextMesg();
            }
        }

        private bool IsMergeagle(Mesg lastMesg)
        {
            switch (lastMesg.Num)
            {
                case MesgNum.Sport:
                case MesgNum.Record:
                    return true;
                default:
                    return false;
            }
        }

        private void MergeMessage(Mesg lastMesg)
        {
            switch (lastMesg.Num)
            {
                case MesgNum.Sport:
                    FixupSportMesg(lastMesg);
                    break;
                case MesgNum.Record:
                    MergeRecord(lastMesg);
                    break;
            }
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
