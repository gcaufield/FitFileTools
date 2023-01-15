using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Dynastream.Fit;

namespace FitFileTools.WorkoutServices
{
    public class Set : IWorkoutSet
    {
        public Exercise Exercise { get; set; }
        public uint? Reps { get; set; }

        public uint? Time { get; set; }
        public double? Weight { get; set; }
        public string Notes { get; set; }

        public IEnumerable<WorkoutStepMesg> GetMesgs(ref ushort index)
        {
            List<WorkoutStepMesg> steps = new List<WorkoutStepMesg>();
            WorkoutStepMesg step = new WorkoutStepMesg();

            if (index != 0)
            {
                /* Return the Rest Set First Because it helps with looping */
                step.SetDurationType(WktStepDuration.Open);
                step.SetIntensity(Intensity.Rest);
                step.SetTargetValue(0);
                step.SetMessageIndex(index++);
                steps.Add(step);
            }

            /* Construct the Actual Step */
            step = new WorkoutStepMesg();
            if (Reps != null)
            {
                step.SetDurationType(WktStepDuration.Reps);
                step.SetDurationReps(Reps);
            }
            else if (Time != null)
            {
                step.SetDurationType(WktStepDuration.Time);
                step.SetDurationTime(Time);
            }
            else
            {
                step.SetDurationType(WktStepDuration.Open);
            }

            step.SetIntensity(Intensity.Active);
            step.SetTargetType(WktStepTarget.Open);
            step.SetTargetValue(0);
            step.SetMessageIndex(index++);

            step.SetExerciseCategory(Exercise.Category);
            if (65535 != Exercise.Name)
            {
                step.SetExerciseName(Exercise.Name);
            }

            StringBuilder notesBuilder = new StringBuilder(Notes);

            if (Weight != null)
            {
                step.SetExerciseWeight((float)(Weight * 0.4535924));
                notesBuilder.Append($" {GetWeightInfo()}");
            }

            step.SetWeightDisplayUnit(FitBaseUnit.Pound);
            step.SetNotes(notesBuilder.ToString());
            steps.Add(step);

            return steps;
        }

        public IEnumerable<Exercise> GetExercises()
        {
            yield return Exercise;
        }

        private string GetWeightInfo()
        {
            switch (Exercise.Equipment)
            {
                case EquipmentType.Plate:
                    return GetPlatesString(1);

                case EquipmentType.LegPress:
                case EquipmentType.Barbell:
                    return GetPlatesString(2);

                case EquipmentType.TwoHandDumbbell:
                    return GetDumbbellString(2);

                case EquipmentType.SingleHandDumbbell:
                    return GetDumbbellString(1);

                default:
                    return "";
            }
        }

        private string GetDumbbellString(int count)
        {
            return $"{count}-{Weight.Value / count:F1}";

        }

        private string GetPlatesString(int plateIncrement)
        {
            StringBuilder plateStringBuilder = new StringBuilder();
            double[] plates = { 45, 35, 25, 10, 5, 2.5 };
            double remainingWeight = Weight.Value - Exercise.MinWeight;
            int plateCount = 0;

            for (int i = 0; i < plates.Length; i++)
            {
                while (remainingWeight >= (plateIncrement * plates[i]))
                {
                    plateCount++;
                    remainingWeight -= plateIncrement * plates[i];
                }

                if (plateCount != 0)
                {
                    plateStringBuilder.AppendFormat("{0}-{1} ", plates[i], plateCount);
                }

                plateCount = 0;
            }

            if (plateStringBuilder.Length > 0)
            {
                plateStringBuilder.Insert(0, '\n');
                plateStringBuilder.Remove(plateStringBuilder.Length - 1, 1);
            }

            return plateStringBuilder.ToString();
        }
    }
}
