using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using FitFileTools.Tools;

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

            // ReSharper disable once InconsistentNaming
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

        private static int Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Command("convert", "Convert a File to Fit.")
                {
                    BuildConvertTcxCommand()
                }
            };

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}