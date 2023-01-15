using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Dynastream.Fit;
using DateTime = Dynastream.Fit.DateTime;
using File = Dynastream.Fit.File;

namespace FitFileTools.WorkoutServices
{
    public class FitWorkoutBuilder
        : IWorkoutBuilder
    {
        private ZipArchive _archive;
        private uint _serialNum = 0;
        private Random rndm = new Random();

        public void ExportWorkout(Workout workout)
        {
            try
            {
                var temp = new MemoryStream();
                var workoutEncoder = new Encode(temp, ProtocolVersion.V10);
                var workoutStepMesgs = new List<WorkoutStepMesg>();
                var exercises = new List<Exercise>();

                /* Each Workout needs a unique serial nuber */
                var fileId = new FileIdMesg();
                fileId.SetTimeCreated(new DateTime(System.DateTime.Now));
                fileId.SetType(File.Workout);
                fileId.SetSerialNumber(_serialNum);
                fileId.SetManufacturer(Manufacturer.Dynastream);

                workoutEncoder.Write(fileId);

                /* Get the Steps */
                ushort mesgIndex = 0;
                foreach (var set in workout.Sets)
                {
                    workoutStepMesgs.AddRange(set.GetMesgs(ref mesgIndex));
                    exercises.AddRange(set.GetExercises());
                }

                var step = new WorkoutStepMesg();
                step.SetDurationType(WktStepDuration.Open);
                step.SetIntensity(Intensity.Rest);
                step.SetTargetValue(0);
                step.SetMessageIndex(mesgIndex++);
                workoutStepMesgs.Add(step);

                /* Generate Messages */
                var workoutMesg = new WorkoutMesg();
                workoutMesg.SetWktName(workout.Name);
                workoutMesg.SetSport(Sport.Training);
                workoutMesg.SetSubSport(SubSport.StrengthTraining);
                workoutMesg.SetCapabilities(WorkoutCapabilities.Tcx);
                workoutMesg.SetNumValidSteps((ushort)(mesgIndex));
                workoutEncoder.Write(workoutMesg);

                foreach (var mesg in workoutStepMesgs)
                {
                    workoutEncoder.Write(mesg);
                }

                mesgIndex = 0;
                foreach (var exercise in exercises.Distinct())
                {
                    workoutEncoder.Write(GetExerciseTitleMesg(exercise, mesgIndex++));
                }

                workoutEncoder.Close();

                byte[] fileBuf = temp.GetBuffer();
                string output = string.Concat(_serialNum, ".fit");
                var entry = _archive.CreateEntry(output).Open();
                entry.Write(fileBuf, 0, (int)temp.Length);
                entry.Close();

                temp.Close();
                _serialNum++;
            }
            catch (Exception e)
            {
                Console.Out.Write(e);
            }
        }

        private static string SplitCamelCase(string friendlyName)
        {
            return Regex.Replace(friendlyName, "([A-Z])", " $1", RegexOptions.Compiled)
                .TrimStart();
        }

        private static ExerciseTitleMesg GetExerciseTitleMesg(Exercise exercise, ushort mesgIndex)
        {
            var newMesg = new ExerciseTitleMesg();
            newMesg.SetMessageIndex(mesgIndex);
            newMesg.SetWktStepName(0, SplitCamelCase(exercise.FriendlyName));
            newMesg.SetExerciseName(exercise.Name);
            newMesg.SetExerciseCategory(exercise.Category);
            return newMesg;
        }

        public void EndExport()
        {
            _archive.Dispose();
        }

        public void StartExport(Stream outputStream)
        {
            _serialNum = (uint)rndm.Next(1, int.MaxValue);
            _archive = new ZipArchive(outputStream, ZipArchiveMode.Create);
        }
    }
}
