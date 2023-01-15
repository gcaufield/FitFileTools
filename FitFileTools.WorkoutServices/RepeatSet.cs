using System.Collections.Generic;
using System.Linq;
using Dynastream.Fit;

namespace FitFileTools.WorkoutServices
{
    class RepeatSet : IWorkoutSet
    {
        private IEnumerable<IWorkoutSet> _sets;

        public uint RepeatCount { get; set; }

        public RepeatSet(IEnumerable<IWorkoutSet> wrappedSets)
        {
            _sets = wrappedSets;
            RepeatCount = 1;
        }

        public IEnumerable<WorkoutStepMesg> GetMesgs(ref ushort index)
        {
            ushort initialIdx = index;
            List<WorkoutStepMesg> mesgs = new List<WorkoutStepMesg>();

            foreach (var set in _sets)
            {
                mesgs.AddRange(set.GetMesgs(ref index));
            }

            if (initialIdx == 0 && RepeatCount > 1)
            {
                // If the Initial Index was 0, the set will not include the rest set that we expect it to. So we are
                // going to leave the initial set and repeat 1 less times so that the rest correctly occurs at the start
                // of the repetition like we expect it to.
                initialIdx = index;

                foreach (var set in _sets)
                {
                    mesgs.AddRange(set.GetMesgs(ref index));
                }

                RepeatCount -= 1;
            }


            WorkoutStepMesg mesg = new WorkoutStepMesg();
            mesg.SetDurationType(WktStepDuration.RepeatUntilStepsCmplt);
            mesg.SetDurationStep(initialIdx);
            mesg.SetRepeatSteps(RepeatCount);
            mesg.SetMessageIndex(index++);
            mesgs.Add(mesg);

            return mesgs;
        }

        public IEnumerable<Exercise> GetExercises()
        {
            return _sets.SelectMany(set => set.GetExercises());
        }
    }
}
