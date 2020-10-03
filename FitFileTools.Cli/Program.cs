using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;


namespace FitFileTools.Cli
{
    class Program
    {
        static Command BuildConvertTcxCommand()
        {
            var cmd = new Command("tcx")
            {
                new Argument<FileInfo>("input_file", description: "The path of the TCX file to convert")
                {
                    Arity = ArgumentArity.ExactlyOne
                }.ExistingOnly()
            };

            // ReSharper disable once InconsistentNaming
            cmd.Handler = CommandHandler.Create((FileInfo input_file) =>
            {
                Console.WriteLine($"The value for tcx file input is {input_file}");
            });

            return cmd;
        }
        static int Main(string[] args)
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
