using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NCalc;
using Newtonsoft.Json.Linq;

namespace FitFileTools.WorkoutServices
{
    public class JsonSetBuilder
    {
        private ExerciseLookup _lookup;
        public JsonSetBuilder(ExerciseLookup lookup)
        {
            _lookup = lookup;
        }

        public IEnumerable<IWorkoutSet> GetSteps(
            JArray steps,
            IDictionary<string, object> inputs,
            IDictionary<string, JObject> templates)
        {
            foreach (var jsonStep in steps)
            {
                if (jsonStep is JObject stepObj)
                {
                    foreach (var step in GetStep(stepObj, inputs, templates))
                    {
                        yield return step;
                    }
                }
            }
        }

        private IEnumerable<IWorkoutSet> GetStep(JObject step,
            IDictionary<string, object> inputs,
            IDictionary<string, JObject> templates)
        {
            string stepType = step.GetValue("type").Value<string>();
            switch (stepType)
            {
                case "template":
                    return ConvertTemplate(step, inputs, templates);

                case "repeat":
                    return ConvertRepeat(step, inputs, templates);

                case "strength":
                    return ConvertStrength(step, inputs);
            }

            return new List<IWorkoutSet>();
        }

        private IEnumerable<IWorkoutSet> ConvertTemplate(
            JObject step,
            IDictionary<string, object> inputs,
            IDictionary<string, JObject> templates)
        {
            string templateName = step.GetValue("name").Value<string>();
            templateName = ConvertString(templateName, inputs) as string;

            if (templateName == null)
            {
                throw new NullReferenceException();
            }

            JArray templateInputs = templates[templateName].GetValue("inputs") as JArray;
            JArray newInputsJson = step.GetValue("inputs") as JArray;

            if (newInputsJson != null)
            {
                Dictionary<string, object> newInputs = new Dictionary<string, object>(inputs);

                for (int i = 0; i < templateInputs.Count; i++)
                {
                    JToken input = newInputsJson[i];
                    if (newInputsJson[i] is JArray inputArray)
                    {
                        List<object> newInputArray = new List<object>();

                        foreach (var arrayInput in inputArray)
                        {
                            newInputArray.Add(ConvertString(arrayInput.Value<string>(), inputs));
                        }

                        newInputs[templateInputs[i].Value<string>()] = newInputArray.ToArray();
                    }
                    else
                    {
                        newInputs[templateInputs[i].Value<string>()] = ConvertString(input.Value<string>(), inputs);
                    }
                }

                inputs = newInputs;
            }

            // Find the template
            JObject template = templates[templateName];
            JArray templateSteps = template.GetValue("steps") as JArray;

            return GetSteps(templateSteps, inputs, templates);
        }

        private IEnumerable<IWorkoutSet> ConvertRepeat(
            JObject step,
            IDictionary<string, object> inputs,
            IDictionary<string, JObject> templates)
        {
            string count = step.GetValue("count").Value<string>();
            count = ConvertString(count, inputs) as string;

            JArray steps = step.GetValue("steps") as JArray;

            RepeatSet set = new RepeatSet(GetSteps(steps, inputs, templates));
            set.RepeatCount = uint.Parse(count);

            yield return set;
        }

        private IEnumerable<Set> ConvertStrength(
            JObject step,
            IDictionary<string, object> inputs)
        {
            string exercise = step.GetValue("exercise")?.Value<string>();
            string weight = step.GetValue("weight")?.Value<string>();
            string reps = step.GetValue("reps")?.Value<string>();
            string time = step.GetValue("time")?.Value<string>();
            string note = step.GetValue("note")?.Value<string>() ?? ""; // Notes are optional

            exercise = ConvertString(exercise, inputs) as string;
            if (weight != null)
            {
                weight = ConvertString(weight, inputs) as string;
            }

            Set workoutSet = new Set();

            workoutSet.Exercise = _lookup.GetExercise(exercise);

            if (workoutSet.Exercise == null)
            {
                // Bad Workout Type
                throw new Exception($"Unknown Exercise {exercise}");
            }

            if (reps != null)
            {
                workoutSet.Reps = uint.Parse(ConvertString(reps, inputs) as string ?? "0");
            }
            else if (time != null)
            {
                workoutSet.Time = uint.Parse(ConvertString(time, inputs) as string ?? "0");
            }

            workoutSet.Notes = ConvertString(note, inputs) as string;

            workoutSet.Weight = null;

            if (weight != null)
            {
                double w = double.Parse(weight);

                if (w > workoutSet.Exercise.MinWeight)
                {
                    // Remove the minimum weight from the weight
                    w -= workoutSet.Exercise.MinWeight;

                    // Floor the Remaining weight to the step weight
                    w = Math.Floor(w / workoutSet.Exercise.Step) * workoutSet.Exercise.Step;

                    // Put the minimum weight back
                    workoutSet.Weight = w + workoutSet.Exercise.MinWeight;
                }
                else
                {
                    workoutSet.Weight = workoutSet.Exercise.MinWeight;
                }
            }

            yield return workoutSet;
        }

        private object ConvertString(
            string value,
            IDictionary<string, object> inputs)
        {
            var template = new Regex(@"\$(?<input>[a-zA-Z0-9]+)(\[(?<arrayIndex>[0-9]+)\])?");

            var matches = template.Matches(value);

            if (matches.Count > 0)
            {
                //  There is a match we need to replace
                int currentIndex = 0;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (Match match in matches)
                {
                    stringBuilder.Append(value.Substring(currentIndex, match.Index - currentIndex));

                    string input = match.Groups["input"].Value;

                    if (!match.Groups["arrayIndex"].Success)
                    {
                        if (inputs[input] is object[] inputArr)
                        {
                            return inputArr;
                        }
                        else
                        {
                            stringBuilder.Append(inputs[input]);
                        }
                    }
                    else if (inputs[input] is object[] inputArr)
                    {
                        int arrIndex = int.Parse(match.Groups["arrayIndex"].Value);
                        stringBuilder.Append(inputArr[arrIndex]);
                    }

                    currentIndex = match.Index + match.Length;
                }

                stringBuilder.Append(value.Substring(currentIndex, value.Length - currentIndex));

                value = stringBuilder.ToString();
            }

            if (value.StartsWith("!"))
            {
                var e = new Expression(value.Substring(1));
                value = e.Evaluate().ToString();
            }

            return value;
        }
    }
}
