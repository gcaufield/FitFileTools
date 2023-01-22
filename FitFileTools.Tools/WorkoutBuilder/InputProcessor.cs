using System;
using System.Collections.Generic;
using FitFileTools.WorkoutServices;

namespace FitFileTools.Tools.WorkoutBuilder
{
    class InputProcessor
    {
        private ExerciseLookup _lookup;

        public InputProcessor(ExerciseLookup lookup)
        {
            _lookup = lookup;
        }

        public Dictionary<string, object> GetUserInput(IEnumerable<WorkoutInput> inputs)
        {
            var input_values = new Dictionary<string, object>();

            foreach (var input in inputs)
            {
                input_values[input.Id] = GetValue(input);
            }

            return input_values;
        }

        private object GetValue(WorkoutInput input)
        {
            string val = null;
            do
            {
                Console.Write($"Value of {input.Name}: ");
                val = Console.ReadLine();
            } while (!ValidateInput(input, val));

            return val;
        }

        private bool ValidateInput(WorkoutInput input, string val)
        {
            bool retVal = false;
            switch (input.InputType)
            {
                case UserInputType.Numeric:
                    retVal = ValidateNumeric(val);
                    break;

                case UserInputType.Exercise:
                    retVal = ValidateExercise(val);
                    break;

                default:
                    throw new ArgumentException("Unhandled Input Type");
            }

            if (!retVal)
            {
                Console.WriteLine($"Input value: {val} invalid for required input type " +
                    $"{input.InputType}");
            }

            return retVal;
        }

        private bool ValidateExercise(string val)
        {
            return _lookup.GetExercise(val) != null;
        }

        private bool ValidateNumeric(string val)
        {
            uint temp;
            return uint.TryParse(val, out temp);
        }
    }
}