using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace JonFinerty.DotNetFormat
{
    public class ResharperCommandLineTools
    {
        private const string ToolsDownloadUrl =
            "https://download.jetbrains.com/resharper/ReSharperUltimate.2018.2.3/JetBrains.ReSharper.CommandLineTools.2018.2.3.zip";

        private const string Checksum = "83b160e7d3e4fd4cef4f250e5f1ccb295a6e7a23c673ae0f50fa9ece1667c6d3";

        private static readonly string InstallPath = Path.Combine(AppContext.BaseDirectory, ".resharper");
        private readonly string _executablePath = Path.Combine(InstallPath, "cleanupcode.exe");

        private readonly string _zipPath = Path.Combine(InstallPath,
            "JetBrains.ReSharper.CommandLineTools.2018.2.3.zip");


        public bool IsInstalled()
        {
            Console.WriteLine($"Checking for tools in {_executablePath}");
            var executableFound = File.Exists(_executablePath);
            Console.WriteLine(executableFound ? "Executable found" : "Executable not found");
            return executableFound;
        }

        public async Task Install()
        {
            Console.WriteLine($"Downloading to {_zipPath}");
            Directory.CreateDirectory(InstallPath);

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, ToolsDownloadUrl);

                using (var response = await client.SendAsync(request))
                using (var responseContentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(_zipPath, FileMode.Create, FileAccess.Write))
                {
                    await responseContentStream.CopyToAsync(fileStream);
                }
            }

            Console.WriteLine();
            CheckCheckSum();
            Unzip();
        }

        private void CheckCheckSum()
        {
            using (var zipStream = new FileStream(_zipPath, FileMode.Open, FileAccess.Read))
            {
                var sha256 = SHA256.Create();

                var byteHash = sha256.ComputeHash(zipStream);
                var hexCheckSum = string.Concat(Array.ConvertAll(byteHash, x => x.ToString("X2")));
                if (!hexCheckSum.Equals(Checksum, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException("Downloaded zip file checksum failed");
                }
            }
        }

        private void Unzip()
        {
            Console.WriteLine("Unzipping tools");
            ZipFile.ExtractToDirectory(_zipPath, InstallPath, true);
        }

        public void CleanupCode(string solution)
        {
            var arguments = solution;

            var cleanupProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = _executablePath,
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