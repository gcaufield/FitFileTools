using Dynastream.Fit;
using System.Collections.Generic;
using System.Linq;

namespace FitFileTools.WorkoutServices
{
    public enum EquipmentType
    {
        Barbell = 0,
        SingleHandDumbbell,
        TwoHandDumbbell,
        StackMachine,
        Plate,
        LegPress
    }

    public class Exercise
    {
        public string FriendlyName { get; set; }

        public ushort Category { get; set; }

        public ushort Name
        {
            get; set;
        }

        public EquipmentType Equipment
        {
            get; set;
        }

        public ushort MinWeight
        {
            get
            {
                switch(Equipment)
                {
                    case EquipmentType.Barbell:
                        return 45;
                    case EquipmentType.Plate:
                    case EquipmentType.SingleHandDumbbell:
                    case EquipmentType.TwoHandDumbbell:
                    case EquipmentType.StackMachine:
                        return 5;
                    case EquipmentType.LegPress:
                        return 118;
                }

                return 0;
            }
        }

        public double Step
        {
            get
            {
                switch(Equipment)
                {
                    default:
                    case EquipmentType.TwoHandDumbbell:
                    case EquipmentType.Barbell:
                    case EquipmentType.LegPress:
                        return 5;

                    case EquipmentType.Plate:
                    case EquipmentType.SingleHandDumbbell:
                        return 2.5;
                    case EquipmentType.StackMachine:
                        return 10;
                }
            }
        }
    }

    public class ExerciseLookup
    {
        public ExerciseLookup()
        {
            exercises_ = new List<Exercise> {
                new Exercise
                {
                    FriendlyName = "Deadlift",
                    Category = ExerciseCategory.Deadlift,
                    Name = DeadliftExerciseName.BarbellDeadlift,
                    Equipment = EquipmentType.Barbell
                },
                new Exercise
                {
                    FriendlyName = "BenchPress",
                    Category = ExerciseCategory.BenchPress,
                    Name = BenchPressExerciseName.BarbellBenchPress,
                    Equipment = EquipmentType.Barbell
                },
                new Exercise
                {
                    FriendlyName = "Squat",
                    Category = ExerciseCategory.Squat,
                    Name = SquatExerciseName.BarbellBackSquat,
                    Equipment = EquipmentType.Barbell
                },
                new Exercise
                {
                    FriendlyName = "FrontSquat",
                    Category = ExerciseCategory.Squat,
                    Name = SquatExerciseName.BarbellFrontSquat,
                    Equipment = EquipmentType.Barbell
                },
                new Exercise
                {
                    FriendlyName = "OverheadPress",
                    Category = ExerciseCategory.ShoulderPress,
                    Name = ShoulderPressExerciseName.OverheadBarbellPress,
                    Equipment = EquipmentType.Barbell
                },
                new Exercise
                {
                    FriendlyName = "PullUp",
                    Category = ExerciseCategory.PullUp,
                    Name = PullUpExerciseName.WeightedPullUp,
                    Equipment = EquipmentType.Plate
                },
                new Exercise
                {
                    FriendlyName = "BenchDip",
                    Category = ExerciseCategory.TricepsExtension,
                    Name = TricepsExtensionExerciseName.BenchDip,
                    Equipment = EquipmentType.Plate
                },
                new Exercise
                {
                    FriendlyName = "TricepsExtension",
                    Category = ExerciseCategory.TricepsExtension,
                    Name = TricepsExtensionExerciseName.OverheadDumbbellTricepsExtension,
                    Equipment = EquipmentType.SingleHandDumbbell
                },
                new Exercise
                {
                    FriendlyName = "DumbbellRow",
                    Category = ExerciseCategory.Row,
                    Name = RowExerciseName.DumbbellRow,
                    Equipment = EquipmentType.SingleHandDumbbell
                },
                new Exercise
                {
                    FriendlyName = "SeatedCableRow",
                    Category = ExerciseCategory.Row,
                    Name = RowExerciseName.SeatedCableRow,
                    Equipment = EquipmentType.StackMachine,
                },
                new Exercise
                {
                    FriendlyName = "DumbbellLunge",
                    Category = ExerciseCategory.Lunge,
                    Name = LungeExerciseName.WalkingDumbbellLunge,
                    Equipment = EquipmentType.TwoHandDumbbell,
                },
                new Exercise
                {
                    FriendlyName = "DumbbellBenchPress",
                    Category = ExerciseCategory.BenchPress,
                    Name = BenchPressExerciseName.DumbbellBenchPress,
                    Equipment = EquipmentType.TwoHandDumbbell,
                },
                new Exercise
                {
                    FriendlyName = "DumbbellOverheadPress",
                    Category = ExerciseCategory.ShoulderPress,
                    Name = ShoulderPressExerciseName.OverheadDumbbellPress,
                    Equipment = EquipmentType.TwoHandDumbbell,
                },
                new Exercise
                {
                    FriendlyName = "DumbbellStepUp",
                    Category = ExerciseCategory.Squat,
                    Name = SquatExerciseName.DumbbellStepUp,
                    Equipment = EquipmentType.TwoHandDumbbell,
                },
                new Exercise
                {
                    FriendlyName = "TricepsPressdown",
                    Category = ExerciseCategory.TricepsExtension,
                    Name = TricepsExtensionExerciseName.TricepsPressdown,
                    Equipment = EquipmentType.StackMachine
                },
                new Exercise
                {
                    FriendlyName = "DumbbellCurl",
                    Category = ExerciseCategory.Curl,
                    Name = CurlExerciseName.AlternatingDumbbellBicepsCurl,
                    Equipment = EquipmentType.TwoHandDumbbell
                },
                new Exercise
                {
                    FriendlyName = "StraightLegDeadlift",
                    Category = ExerciseCategory.Deadlift,
                    Name = DeadliftExerciseName.BarbellStraightLegDeadlift,
                    Equipment = EquipmentType.Barbell
                },
                new Exercise
                {
                    FriendlyName = "FacePull",
                    Category = ExerciseCategory.Row,
                    Name = RowExerciseName.FacePull,
                    Equipment = EquipmentType.StackMachine
                },
            }.ToDictionary(k => k.FriendlyName);
        }

        public Exercise GetExercise(string friendlyName)
        {
            return exercises_[friendlyName];
        }

        private Dictionary<string, Exercise> exercises_;
    }
}