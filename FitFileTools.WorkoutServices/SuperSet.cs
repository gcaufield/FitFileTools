using System.Collections.Generic;
using System.Linq;
using Dynastream.Fit;

namespace FitFileTools.WorkoutServices
{
    public class SuperSet : IWorkoutSet
    {
        private LinkedList<IWorkoutSet> _sets;
        public IEnumerable<IWorkoutSet> Sets
        {
            get { return _sets; }
            set { _sets = new LinkedList<IWorkoutSet>(value); }
        }

        public SuperSet()
        {
            _sets = new LinkedList<IWorkoutSet>();
        }

        public IEnumerable<WorkoutStepMesg> GetMesgs(ref ushort index)
        {
            List<WorkoutStepMesg> mesgs = new List<WorkoutStepMesg>();

            foreach (var workoutSet in _sets)
            {
                mesgs.AddRange(workoutSet.GetMesgs(ref index));
            }

            return mesgs;
        }

        public IEnumerable<Exercise> GetExercises()
        {
            return _sets.SelectMany(set => set.GetExercises());
        }
    }
}
