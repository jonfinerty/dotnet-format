using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace dotnet_format
{
    [Command(Description = "A global command tool for formatting dotnet code")]
    class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Argument(0, Description = "A positional parameter that must be specified.\nThe solution file for the solution to format")]
        [Required]
        public string Solution { get; }

        private async Task<int> OnExecuteAsync()
        {
            Console.WriteLine($"Starting Format");

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