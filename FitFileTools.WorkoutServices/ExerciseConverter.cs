using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Dynastream.Fit;

namespace FitFileTools.WorkoutServices
{
    public class ExerciseConverter
    {
        private static readonly Dictionary<ushort, Type> CategoryLookup = new Dictionary<ushort, Type>
        {
            { ExerciseCategory.BenchPress, typeof(BenchPressExerciseName) },
            { ExerciseCategory.CalfRaise, typeof(CalfRaiseExerciseName) },
            { ExerciseCategory.Cardio, typeof(CardioExerciseName) },
            { ExerciseCategory.Carry, typeof(CarryExerciseName) },
            { ExerciseCategory.Chop, typeof(ChopExerciseName) },
            { ExerciseCategory.Core, typeof(CoreExerciseName) },
            { ExerciseCategory.Crunch, typeof(CrunchExerciseName) },
            { ExerciseCategory.Curl, typeof(CurlExerciseName) },
            { ExerciseCategory.Deadlift, typeof(DeadliftExerciseName) },
            { ExerciseCategory.Flye, typeof(FlyeExerciseName) },
            { ExerciseCategory.HipRaise, typeof(HipRaiseExerciseName) },
            { ExerciseCategory.HipStability, typeof(HipStabilityExerciseName) },
            { ExerciseCategory.HipSwing, typeof(HipSwingExerciseName) },
            { ExerciseCategory.Hyperextension, typeof(HyperextensionExerciseName) },
            { ExerciseCategory.LateralRaise, typeof(LateralRaiseExerciseName) },
            { ExerciseCategory.LegCurl, typeof(LegCurlExerciseName) },
            { ExerciseCategory.LegRaise, typeof(LegRaiseExerciseName) },
            { ExerciseCategory.Lunge, typeof(LungeExerciseName) },
            { ExerciseCategory.OlympicLift, typeof(OlympicLiftExerciseName) },
            { ExerciseCategory.Plank, typeof(PlankExerciseName) },
            { ExerciseCategory.Plyo, typeof(PlyoExerciseName) },
            { ExerciseCategory.PullUp, typeof(PullUpExerciseName) },
            { ExerciseCategory.PushUp, typeof(PushUpExerciseName) },
            { ExerciseCategory.Row, typeof(RowExerciseName) },
            { ExerciseCategory.ShoulderPress, typeof(ShoulderPressExerciseName) },
            { ExerciseCategory.ShoulderStability, typeof(ShoulderStabilityExerciseName) },
            { ExerciseCategory.Shrug, typeof(ShrugExerciseName) },
            { ExerciseCategory.SitUp, typeof(SitUpExerciseName) },
            { ExerciseCategory.Squat, typeof(SquatExerciseName) },
            { ExerciseCategory.TotalBody, typeof(TotalBodyExerciseName) },
            { ExerciseCategory.TricepsExtension, typeof(TricepsExtensionExerciseName) },
        };

        private static string GetExcerciseName(Type type, ushort arg)
        {
            var flags = BindingFlags.Static | BindingFlags.Public;
            var fields = type.GetFields(flags).Where(x => x.IsLiteral); // that will return all fields of any typewitch (arg)

            foreach (var field in fields)
            {
                if ((ushort)field.GetValue(null) == arg)
                {
                    string fieldName = field.Name;

                    fieldName = fieldName.Replace("_", "");
                    fieldName = Regex.Replace(fieldName, "(.)([A-Z])", "$1 $2", RegexOptions.Compiled);
                    fieldName = Regex.Replace(fieldName, "(.)([0-9]+)", "$1 $2", RegexOptions.Compiled);

                    return fieldName;
                }
            }

            return "Unknown";
        }

        public static string ConvertToExerciseName(Exercise exercise)
        {
            if (CategoryLookup.ContainsKey(exercise.Category))
            {
                return GetExcerciseName(CategoryLookup[exercise.Category], exercise.Name);
            }

            return "Unknown";
        }
    }
}