using System;
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
    }

    class LapManager
    {
        private Accumulator _currentLap = new Accumulator();
        private Accumulator _session = new Accumulator();

        private void ResetAccumulation()
        {
            _currentLap = new Accumulator(_currentLap);
        }

        public void OnRecordMesg(Mesg mesg)
        {
            _currentLap.OnRecordMesg(mesg);
            _session.OnRecordMesg(mesg);
        }

        public void OnLapMesg(Mesg mesg)
        {
            mesg.RemoveField(mesg.GetField(LapMesg.FieldDefNum.EnhancedAvgSpeed));
            mesg.RemoveField(mesg.GetField(LapMesg.FieldDefNum.EnhancedMaxSpeed));

            mesg.SetFieldValue(
                LapMesg.FieldDefNum.StartPositionLat,
                _currentLap.StartPositionLat);
            mesg.SetFieldValue(
                LapMesg.FieldDefNum.StartPositionLong,
                _currentLap.StartPositionLong);

            mesg.SetFieldValue(
                LapMesg.FieldDefNum.EndPositionLat,
                _currentLap.EndPositionLat);
            mesg.SetFieldValue(
                LapMesg.FieldDefNum.EndPositionLong,
                _currentLap.EndPositionLong);

            float averageSpeed = _currentLap.TotalDistance / (float)mesg.GetFieldValue(LapMesg.FieldDefNum.TotalTimerTime);

            mesg.SetFieldValue(LapMesg.FieldDefNum.TotalDistance, _currentLap.TotalDistance);
            mesg.SetFieldValue(LapMesg.FieldDefNum.EnhancedAvgSpeed, averageSpeed);
            mesg.SetFieldValue(LapMesg.FieldDefNum.TotalDescent, _currentLap.TotalDescent);
            mesg.SetFieldValue(LapMesg.FieldDefNum.TotalAscent, _currentLap.TotalAscent);
            mesg.SetFieldValue(LapMesg.FieldDefNum.EnhancedMaxSpeed, _currentLap.MaxSpeed);


            ResetAccumulation();
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


            ResetAccumulation();
        }
    }
}
