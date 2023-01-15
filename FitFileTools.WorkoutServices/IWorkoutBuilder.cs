using System.IO;

namespace FitFileTools.WorkoutServices
{
    public interface IWorkoutBuilder
    {
        void ExportWorkout(Workout workout);
        void StartExport(Stream outputStream);
        void EndExport();
    }
}