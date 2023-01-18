

namespace FitFileTools.WorkoutServices
{
    public class WorkoutInput
    {
        public WorkoutInput(string name, string id)
        {
            _name = name;
            _id = id;
        }


        private string _name;
        public string Name { get { return _name; } }
    
        private string _id;
        public string Id { get { return _id; } }
    }
}