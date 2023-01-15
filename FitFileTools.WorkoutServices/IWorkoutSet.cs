using System.Collections.Generic;
using Dynastream.Fit;

namespace FitFileTools.WorkoutServices
{
    public interface IWorkoutSet
    {
        IEnumerable<WorkoutStepMesg> GetMesgs(ref ushort index);
        IEnumerable<Exercise> GetExercises();
    }
}