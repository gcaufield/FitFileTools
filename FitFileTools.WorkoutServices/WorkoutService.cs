using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FitFileTools.WorkoutServices
{
    public class WorkoutService
    {
        private readonly JsonSetBuilder _setBuilder;
        private readonly IWorkoutBuilder _builder;

        public WorkoutService(JsonSetBuilder setBuilder, IWorkoutBuilder builder)
        {
            _setBuilder = setBuilder;
            _builder = builder;
        }

        public void BuildWorkouts(Stream outStream, TextReader workoutReader, IDictionary<string, object> inputs)
        {
            _builder.StartExport(outStream);
            foreach (var workout in BuildWorkouts(workoutReader, inputs))
            {
                _builder.ExportWorkout(workout);
            }
            _builder.EndExport();
        }

        private IEnumerable<Workout> BuildWorkouts(TextReader workout, IDictionary<string, object> inputs)
        {
            using (var reader = new JsonTextReader(workout))
            {
                var o = (JObject)JToken.ReadFrom(reader);

                var templates = GetTemplates(o);
                return GetWorkouts(o.GetValue("workouts") as JArray, inputs, templates);
            }
        }

        private IEnumerable<Workout> GetWorkouts(
            JArray jsonWorkouts,
            IDictionary<string, object> inputs,
            IDictionary<string, JObject> templates)
        {
            foreach (var jToken in jsonWorkouts)
            {
                if (jToken is JObject jsonWorkout)
                {
                    yield return GetWorkout(jsonWorkout, inputs, templates);
                }
            }
        }

        private Workout GetWorkout(
            JObject jsonWorkout,
            IDictionary<string, object> inputs,
            IDictionary<string, JObject> templates)
        {
            if (jsonWorkout.GetValue("steps") is JArray steps)
            {
                IEnumerable<IWorkoutSet> sets = _setBuilder.GetSteps(steps, inputs, templates);

                return new Workout
                {
                    Name = jsonWorkout.GetValue("name").Value<string>(),
                    Sets = sets
                };
            }

            return null;

        }

        private IDictionary<string, JObject> GetTemplates(JObject workout)
        {
            var retval = new Dictionary<string, JObject>();

            if (workout.GetValue("templates") is JArray jsonTemplates)
            {
                foreach (var template in jsonTemplates)
                {
                    if (template is JObject templateObj)
                    {
                        retval.Add(templateObj.GetValue("name").Value<string>(), templateObj);
                    }
                }
            }

            return retval;
        }
    }
}
