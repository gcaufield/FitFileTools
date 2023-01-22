using System;

namespace FitFileTools.WorkoutServices
{
    public enum UserInputType
    {
        Numeric,
        Exercise
    }

    public class WorkoutInput
    {
        public WorkoutInput(string name, string id, string type)
        {
            _name = name;
            _id = id;

            switch (type)
            {
                case "uint":
                    _inputType = UserInputType.Numeric;
                    break;
                case "exercise":
                    _inputType = UserInputType.Exercise;
                    break;
                default:
                    throw new ArgumentException("Invalid input type");
            }
        }


        private string _name;
        public string Name { get { return _name; } }

        private string _id;
        public string Id { get { return _id; } }

        private UserInputType _inputType;
        public UserInputType InputType { get { return _inputType; } }
    }
}