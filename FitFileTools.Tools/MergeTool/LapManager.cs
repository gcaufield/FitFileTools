using System;
using System.Collections.Generic;
using Dynastream.Fit;

namespace FitFileTools.Tools.MergeTool
{
    class Accumulator
    {
        private Mesg _firstRecord;
        private Mesg _lastRecord;

        public Accumulator()
        {
        }

        public Accumulator(Accumulator oldAccumulator)
        {
            _firstRecord = oldAccumulator._lastRecord;
            _lastRecord = _firstRecord;
        }

        public float TotalDistance => FitHelpers.FieldDiff(_lastRecord, _firstRecord, RecordMesg.FieldDefNum.Distance);

        public float TotalDescent { get; private set; } = 0;

        public float TotalAscent { get; private set; } = 0;

        public float MaxSpeed { get; private set; }
        
        public uint StartTime { get;  set; }
        public float ElapsedTime { get; set; }

        private uint EndTime => StartTime + (uint)ElapsedTime;

        public int? StartPositionLat => (int?)_firstRecord.GetFieldValue(RecordMesg.FieldDefNum.PositionLat);
        public int? StartPositionLong => (int?)_firstRecord.GetFieldValue(RecordMesg.FieldDefNum.PositionLong);
        public int? EndPositionLat => (int?)_lastRecord.GetFieldValue(RecordMesg.FieldDefNum.PositionLat);
        public int? EndPositionLong => (int?)_lastRecord.GetFieldValue(RecordMesg.FieldDefNum.PositionLong);

        public void OnRecordMesg(Mesg mesg)
        {
            if (_firstRecord == null)
            {
                _firstRecord = mesg;
            }
            else
            {
                var delta = FitHelpers.FieldDiff(mesg, _lastRecord, RecordMesg.FieldDefNum.EnhancedAltitude);
                if (delta < 0)
                {
                    TotalDescent += Math.Abs(delta);
                }
                else
                {
                    TotalAscent += delta;
                }

                if (mesg.HasField(RecordMesg.FieldDefNum.EnhancedSpeed) && (float)mesg.GetFieldValue(RecordMesg.FieldDefNum.EnhancedSpeed) > MaxSpeed)
                {
                    MaxSpeed = (float)mesg.GetFieldValue(RecordMesg.FieldDefNum.EnhancedSpeed);
                }
            }
            _lastRecord = mesg;
        }

        public bool Contains(uint ts)
        {
            return ts >= StartTime && ts < EndTime;
        }
    }

    class LapManager
    {
        private List<Accumulator> _laps = new List<Accumulator>{ new Accumulator() };
        private Accumulator _currentLap;
        private Accumulator _session = new Accumulator();

        public LapManager()
        {
            _currentLap = _laps[0];
        }

        public void OnRecordMesg(Mesg mesg)
        {
            var ts = (uint)mesg.GetFieldValue(RecordMesg.FieldDefNum.Timestamp);
            var lap = _laps.Find(accum => accum.Contains(ts)) ?? _currentLap;
            lap.OnRecordMesg(mesg);
            _session.OnRecordMesg(mesg);
        }

        public void ExportLapMesg(Mesg mesg)
        {
            uint startTime = (uint)mesg.GetFieldValue(LapMesg.FieldDefNum.StartTime);
            Accumulator accum = _laps.Find(accumulator => accumulator.StartTime == startTime);
            
            mesg.RemoveField(mesg.GetField(LapMesg.FieldDefNum.EnhancedAvgSpeed));
            mesg.RemoveField(mesg.GetField(LapMesg.FieldDefNum.EnhancedMaxSpeed));

            mesg.SetFieldValue(
                LapMesg.FieldDefNum.StartPositionLat,
                accum.StartPositionLat);
            mesg.SetFieldValue(
                LapMesg.FieldDefNum.StartPositionLong,
                accum.StartPositionLong);

            mesg.SetFieldValue(
                LapMesg.FieldDefNum.EndPositionLat,
                accum.EndPositionLat);
            mesg.SetFieldValue(
                LapMesg.FieldDefNum.EndPositionLong,
                accum.EndPositionLong);

            float averageSpeed = accum.TotalDistance / (float)mesg.GetFieldValue(LapMesg.FieldDefNum.TotalTimerTime);

            mesg.SetFieldValue(LapMesg.FieldDefNum.TotalDistance, accum.TotalDistance);
            mesg.SetFieldValue(LapMesg.FieldDefNum.EnhancedAvgSpeed, averageSpeed);
            mesg.SetFieldValue(LapMesg.FieldDefNum.TotalDescent, accum.TotalDescent);
            mesg.SetFieldValue(LapMesg.FieldDefNum.TotalAscent, accum.TotalAscent);
            mesg.SetFieldValue(LapMesg.FieldDefNum.EnhancedMaxSpeed, accum.MaxSpeed);
        }
        
        public void OnLapMesg(Mesg mesg)
        {
            _currentLap.StartTime = (uint)mesg.GetFieldValue(LapMesg.FieldDefNum.StartTime);
            _currentLap.ElapsedTime = (float)mesg.GetFieldValue(LapMesg.FieldDefNum.TotalElapsedTime);
            _currentLap = new Accumulator();
            _laps.Add(_currentLap);
        }

        public void OnSessionMesg(Mesg mesg)
        {
            mesg.RemoveField(mesg.GetField(SessionMesg.FieldDefNum.EnhancedAvgSpeed));
            mesg.RemoveField(mesg.GetField(SessionMesg.FieldDefNum.EnhancedMaxSpeed));
            
            mesg.SetFieldValue(
                SessionMesg.FieldDefNum.StartPositionLat,
                _session.StartPositionLat);
            mesg.SetFieldValue(
                SessionMesg.FieldDefNum.StartPositionLong,
                _session.StartPositionLong);

            float averageSpeed = _session.TotalDistance / (float)mesg.GetFieldValue(SessionMesg.FieldDefNum.TotalTimerTime);

            mesg.SetFieldValue(SessionMesg.FieldDefNum.TotalDistance, _session.TotalDistance);
            mesg.SetFieldValue(SessionMesg.FieldDefNum.AvgSpeed, averageSpeed);
            mesg.SetFieldValue(SessionMesg.FieldDefNum.TotalDescent, _session.TotalDescent);
            mesg.SetFieldValue(SessionMesg.FieldDefNum.TotalAscent, _session.TotalAscent);
            mesg.SetFieldValue(SessionMesg.FieldDefNum.MaxSpeed, _session.MaxSpeed);
        }
    }
}