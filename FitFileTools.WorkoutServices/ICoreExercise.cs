namespace FitFileTools.WorkoutServices
{
    public interface ICoreExercise
    {
        string ExerciseTitle { get; }
        ushort OneRepMax { get; set; }
    }
}