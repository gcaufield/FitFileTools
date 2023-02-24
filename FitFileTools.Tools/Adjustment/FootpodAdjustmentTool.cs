using System;
using System.Collections.Generic;
using System.IO;
using Dynastream.Fit;

namespace FitFileTools.Tools.Adjustment
{
    public class FootpodAdjustmentTool
    {
        private readonly decimal _trueDistance;
        private readonly Stream _input;
        private decimal? _scale;

        public FootpodAdjustmentTool(Stream input, decimal trueDistance)
        {
            _input = input;
            _trueDistance = trueDistance * 1000;
            _scale = null;
        }

        public bool ReadFile()
        {
            var decode = new Decode();

            lock (_input)
            {
                try
                {
                    ResetInput();

                    if (!decode.CheckIntegrity(_input))
                    {
                        // File fails integrity check
                        return false;
                    }

                    ResetInput();

                    decode.MesgEvent += (sender, args) =>
                    {
                        if (args.mesg.Num == MesgNum.Session)
                        {
                            if (args.mesg.GetFieldValue(SessionMesg.FieldDefNum.TotalDistance) is float distance)
                            {
                                _scale = _trueDistance / (decimal)distance;
                            }
                        }
                    };

                    decode.Read(_input);

                    if (_scale == null)
                    {
                        return false;
                    }
                }
                finally
                {
                    ResetInput();
                }
            }

            return true;
        }

        public void AdjustFile(Stream outStream)
        {
            if (_scale == null)
            {
                throw new InvalidOperationException("No Scale Defined");
            }

            var decode = new Decode();
            var encode = new Encode(outStream, ProtocolVersion.V20);

            // Forward all Definitions as we aren't going to modify them.
            //decode.MesgDefinitionEvent += (sender, args) => { encode.Write(args.mesgDef); };

            decode.MesgEvent += (sender, args) =>
            {
                // Remove all expanded fields before adjusting
                args.mesg.RemoveExpandedFields();

                switch (args.mesg.Num)
                {
                    case MesgNum.Record:
                        AdjustRecord(args.mesg);
                        break;
                    case MesgNum.Lap:
                        AdjustLap(args.mesg);
                        break;
                    case MesgNum.Split:
                        AdjustSplit(args.mesg);
                        break;
                    case MesgNum.Session:
                        AdjustSession(args.mesg);
                        break;
                    case MesgNum.WorkoutStep:
                        RemoveStrings(args.mesg);
                        break;
                }

                encode.Write(args.mesg);
            };

            lock (_input)
            {
                try
                {
                    ResetInput();

                    decode.Read(_input);
                    encode.Close();
                }
                finally
                {
                    ResetInput();
                }
            }
        }

        private void AdjustSession(Mesg mesg)
        {
            AdjustField(mesg.GetField(SessionMesg.FieldDefNum.EnhancedAvgSpeed));
            AdjustField(mesg.GetField(SessionMesg.FieldDefNum.EnhancedMaxSpeed));
            AdjustField(mesg.GetField(SessionMesg.FieldDefNum.AvgSpeed));
            AdjustField(mesg.GetField(SessionMesg.FieldDefNum.MaxSpeed));
            AdjustField(mesg.GetField(SessionMesg.FieldDefNum.TotalDistance));
            AdjustField(mesg.GetField(SessionMesg.FieldDefNum.AvgStepLength));
            AdjustVerticalRatio(mesg.GetField(SessionMesg.FieldDefNum.AvgVerticalRatio),
                mesg.GetFieldValue(SessionMesg.FieldDefNum.AvgStepLength),
                mesg.GetFieldValue(SessionMesg.FieldDefNum.AvgVerticalOscillation));
        }

        private void AdjustLap(Mesg mesg)
        {
            AdjustField(mesg.GetField(LapMesg.FieldDefNum.EnhancedAvgSpeed));
            AdjustField(mesg.GetField(LapMesg.FieldDefNum.EnhancedMaxSpeed));
            AdjustField(mesg.GetField(LapMesg.FieldDefNum.AvgSpeed));
            AdjustField(mesg.GetField(LapMesg.FieldDefNum.MaxSpeed));
            AdjustField(mesg.GetField(LapMesg.FieldDefNum.TotalDistance));
            AdjustField(mesg.GetField(LapMesg.FieldDefNum.AvgStepLength));
            AdjustVerticalRatio(mesg.GetField(LapMesg.FieldDefNum.AvgVerticalRatio),
                mesg.GetFieldValue(LapMesg.FieldDefNum.AvgStepLength),
                mesg.GetFieldValue(LapMesg.FieldDefNum.AvgVerticalOscillation));
        }

        private void AdjustSplit(Mesg mesg)
        {
            AdjustField(mesg.GetField(SplitMesg.FieldDefNum.TotalDistance));
        }

        private void AdjustRecord(Mesg mesg)
        {
            AdjustField(mesg.GetField(RecordMesg.FieldDefNum.EnhancedSpeed));
            AdjustField(mesg.GetField(RecordMesg.FieldDefNum.Speed));
            AdjustField(mesg.GetField(RecordMesg.FieldDefNum.Distance));
            AdjustField(mesg.GetField(RecordMesg.FieldDefNum.StepLength));
            AdjustField(mesg.GetField(RecordMesg.FieldDefNum.CycleLength));
            AdjustVerticalRatio(mesg.GetField(RecordMesg.FieldDefNum.VerticalRatio),
                mesg.GetFieldValue(RecordMesg.FieldDefNum.StepLength),
                mesg.GetFieldValue(RecordMesg.FieldDefNum.VerticalOscillation));
        }

        private void RemoveStrings(Mesg mesg)
        {
            mesg.RemoveField(mesg.GetField(WorkoutStepMesg.FieldDefNum.Notes));
        }

        private void AdjustField(Field field)
        {
            if (field == null)
            {
                return;
            }

            var value = Convert.ToDecimal(field.GetValue());
            if (value != decimal.Zero)
            {
                field.SetValue(value * _scale);
            }
        }

        private void AdjustVerticalRatio(Field ratio, object stepLen, object oscillation)
        {
            if (ratio == null || stepLen == null || oscillation == null)
            {
                return;
            }

            var value = Convert.ToDecimal(oscillation) / (Convert.ToDecimal(stepLen));
            ratio.SetValue(value * 100);
        }
        private void ResetInput()
        {
            lock (_input)
            {
                _input.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
