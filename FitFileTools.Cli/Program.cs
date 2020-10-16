using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using FitFileTools.Tools;
using FitFileTools.Tools.MergeTool;
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

        private static Command BuildMergeFilesCommand()
        {
            var cmd = new Command("merge")
            {
                new Argument<FileInfo>("zwift_file", "The path to the Zwift .FIT file")
                {
                    Arity = ArgumentArity.ExactlyOne
                }.ExistingOnly(),
                new Argument<FileInfo>("garmin_file", "The path to the Garmin device .FIT file")
                {
                    Arity = ArgumentArity.ExactlyOne
                }.ExistingOnly()
            };

            cmd.Handler = CommandHandler.Create(async (FileInfo zwift_file, FileInfo garmin_file) =>
            {
                await using var zwiftStream = new FileStream(zwift_file.FullName, FileMode.Open, FileAccess.Read);
                await using var garminStream = new FileStream(garmin_file.FullName, FileMode.Open, FileAccess.Read);
                var tool = new ZwiftMergeTool(zwiftStream, garminStream);

                await using var tempOut = new FileStream("merge.fit", FileMode.Create, FileAccess.ReadWrite);
                await tool.MergeFilesAsync(tempOut);
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
                BuildMergeFilesCommand()
            };

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
