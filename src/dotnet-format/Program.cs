using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace JonFinerty.DotNetFormat
{
    [Command(Description = "A global command tool for formatting dotnet code")]
    public class Program
    {
        [Argument(0, Description =
            "A positional parameter that must be specified.\nThe solution file for the solution to format")]
        [Required]
        public string Solution { get; }

        public static int Main(string[] args)
        {
            return CommandLineApplication.Execute<Program>(args);
        }

        private async Task<int> OnExecuteAsync()
        {
            Console.WriteLine("Starting Format");

            var resharperCommandLineTools = new ResharperCommandLineTools();
            if (!resharperCommandLineTools.IsInstalled())
            {
                await resharperCommandLineTools.Install();
            }

            resharperCommandLineTools.CleanupCode(Solution);
            return 0;
        }
    }
}