using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Dynastream.Fit;
using FitFileTools.Tools;
using FitFileTools.Tools.FileFilter;
using FitFileTools.Tools.MergeTool;
using FitFileTools.Tools.WorkoutBuilder;

// ReSharper disable InconsistentNaming

namespace FitFileTools.Cli
{
    internal class Program
    {
        private static Command BuildConvertTcxCommand()
        {
            var cmd = new Command("tcx")
            {
                new Argument<FileInfo>("input_file", "The path of the TCX file to convert")
                {
                    Arity = ArgumentArity.ExactlyOne
                }.ExistingOnly()
            };

            cmd.Handler = CommandHandler.Create(async (FileInfo input_file) =>
            {
                Console.WriteLine($"The value for tcx file input is {input_file}");
                var writer = new FitWriter(
                    Path.ChangeExtension(input_file.FullName, ".fit"),
                    new TcxDataStream(input_file)
                );

                await writer.Run();
            });

            return cmd;
        }

        private static Command BuildFilterFilesCommand()
        {
            var cmd = new Command("filter")
            {
                new Argument<DirectoryInfo>("source", "The path to the source dir to search")
                {
                    Arity = ArgumentArity.ExactlyOne
                }.ExistingOnly(),
                new Argument<DirectoryInfo>("destination", "The path to the destination dir to copy to")
                {
                    Arity = ArgumentArity.ExactlyOne
                }.ExistingOnly()
            };

            cmd.Handler = CommandHandler.Create((DirectoryInfo source, DirectoryInfo destination) =>
            {
                var tool = new FileFilter(
                    new ActivityTypeFilter(new[] { Sport.Running, Sport.Swimming, Sport.Cycling }));

                tool.FilterFolder(source.FullName, destination.FullName);

                return Task.CompletedTask;
            });

            return cmd;
        }

        private static Command BuildMergeFilesCommand()
        {
            var cmd = new Command("merge")
            {
                new Option<bool>("--garmin", "Use the Garmin device as the base"),
                new Argument<FileInfo>("virtual_file", "The path to the Virtual Activity .FIT file")
                {
                    Arity = ArgumentArity.ExactlyOne
                }.ExistingOnly(),
                new Argument<FileInfo>("garmin_file", "The path to the Garmin device .FIT file")
                {
                    Arity = ArgumentArity.ExactlyOne
                }.ExistingOnly(),
            };

            cmd.Handler = CommandHandler.Create(async (FileInfo virtual_file, FileInfo garmin_file, bool garmin) =>
            {
                await using var zwiftStream = new FileStream(virtual_file.FullName, FileMode.Open, FileAccess.Read);
                await using var garminStream = new FileStream(garmin_file.FullName, FileMode.Open, FileAccess.Read);
                await using var tempOut = new FileStream($"{System.DateTime.Now:yyyy-MM-dd-HH-mm-ss}.fit", FileMode.Create, FileAccess.ReadWrite);

                var tool = new ZwiftMergeTool(zwiftStream, garminStream, tempOut,
                    garmin ? ZwiftMergeTool.FileGenerator.Garmin : ZwiftMergeTool.FileGenerator.VirtualPlatform);
                await tool.MergeFilesAsync();
            });

            return cmd;
        }
        private static Command BuildGenerateWorkoutsCommand()
        {
            var cmd = new Command("workout")
            {
                new Argument<FileInfo>("definition", "The path of the workout definition JSON")
                {
                    Arity = ArgumentArity.ExactlyOne
                }.ExistingOnly()
            };

            cmd.Handler = CommandHandler.Create((FileInfo definition) =>
            {
                var tool = new WorkoutBuilder(definition);
                tool.BuildWorkouts();
            });

            return cmd;
        }

        private static int Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Command("convert", "Convert a File to Fit.")
                {
                    BuildConvertTcxCommand()
                },
                BuildMergeFilesCommand(),
                BuildFilterFilesCommand(),
                BuildGenerateWorkoutsCommand()
            };

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
