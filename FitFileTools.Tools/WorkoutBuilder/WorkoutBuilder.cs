using System;
using System.IO;
using FitFileTools.WorkoutServices;
using System.Collections.Generic;

namespace FitFileTools.Tools.WorkoutBuilder
{
    public class WorkoutBuilder
    {
        public WorkoutBuilder(FileInfo defitionFile)
        {
            _definitionFile = defitionFile;
            var lookup = new ExerciseLookup();
            var setBuilder = new JsonSetBuilder(lookup);
            var workoutBuilder = new FitWorkoutBuilder();
            _service = new WorkoutService(setBuilder, workoutBuilder);
            _inputProcessor = new InputProcessor(lookup);

        }

        public void BuildWorkouts()
        {
            List<WorkoutInput> inputs = null;
            using (var fileStream = new FileStream(_definitionFile.FullName, FileMode.Open))
            using (var fileReader = new StreamReader(fileStream))
            {
                inputs = _service.GetInputs(fileReader);
            }

            var input_values = _inputProcessor.GetUserInput(inputs);

            using (var fileStream = new FileStream(_definitionFile.FullName, FileMode.Open))
            using (var fileReader = new StreamReader(fileStream))
            using (var outputStream = new FileStream("outputs.zip", FileMode.Create))
            {
                _service.BuildWorkouts(outputStream, fileReader, input_values);
            }
        }

        private WorkoutService _service;
        private FileInfo _definitionFile;
        private InputProcessor _inputProcessor;
    }
}