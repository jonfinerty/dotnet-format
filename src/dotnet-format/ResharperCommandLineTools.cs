using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace JonFinerty.DotNetFormat
{
    //TODO checksum zip
    public class ResharperCommandLineTools
    {
        private static readonly string installLocation = Path.Combine(AppContext.BaseDirectory, ".resharper");
        private readonly string executable = Path.Combine(installLocation, "cleanupcode.exe");

        private readonly string url =
            "https://download.jetbrains.com/resharper/ReSharperUltimate.2018.2.3/JetBrains.ReSharper.CommandLineTools.2018.2.3.zip";

        private readonly string zip = Path.Combine(installLocation,
            "JetBrains.ReSharper.CommandLineTools.2018.2.3.zip");


        public bool IsInstalled()
        {
            Console.WriteLine($"Checking for tools in {executable}");
            var executableFound = File.Exists(executable);
            Console.WriteLine(executableFound ? "Executable found" : "Executable not found");
            return executableFound;
        }

        public async Task Install()
        {
            Console.WriteLine($"Downloading to {zip}");
            Directory.CreateDirectory(installLocation);

            var totalReads = 0L;
            var buffer = new byte[8192];

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                using (var response = await client.SendAsync(request))
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fs = new FileStream(zip, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length))
                {
                    int read;
                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        await fs.WriteAsync(buffer, 0, read);

                        totalReads += 1;

                        if (totalReads % 64 == 0) Console.Write('.');
                    }
                }
            }

            Console.WriteLine();

            Unzip();
        }

        private void Unzip()
        {
            Console.WriteLine("Unzipping tools");
            ZipFile.ExtractToDirectory(zip, installLocation, true);
        }

        public void CleanupCode(string solution)
        {
            var arguments = solution;

            var cleanupProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = executable,
                    CreateNoWindow = true,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            cleanupProcess.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            cleanupProcess.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);
            cleanupProcess.Start();
            cleanupProcess.BeginOutputReadLine();
            cleanupProcess.BeginErrorReadLine();
            cleanupProcess.WaitForExit();
        }
    }
}