using FitFileTools.WorkoutServices;

namespace FitFileTools.Tools.WorkoutBuilder
{
    public class WorkoutBuilder
    {
        public WorkoutBuilder()
        {
            var lookup = new ExerciseLookup();
            var setBuilder = new JsonSetBuilder(lookup);
            var workoutBuilder = new FitWorkoutBuilder();
            _service = new WorkoutService(setBuilder, workoutBuilder);
        }

        public void BuildWorkouts()
        {
            _service.BuildWorkouts(null, null, null);
        }

        private WorkoutService _service;
    }
}