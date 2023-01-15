using System.Collections.Generic;

namespace FitFileTools.WorkoutServices
{
    public class Workout
    {
        private string _name;
        private LinkedList<IWorkoutSet> _sets;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public IEnumerable<IWorkoutSet> Sets
        {
            get { return _sets; }
            set { _sets = new LinkedList<IWorkoutSet>(value); }
        }

        public Workout()
        {
            _name = "";
            _sets = new LinkedList<IWorkoutSet>();
        }
    }
}