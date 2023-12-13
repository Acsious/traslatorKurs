using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace translatorKurs
{
    class Compiler
    {
        static public string OutputFilename = "out";

        static readonly string ExecutableName = string.Concat(OutputFilename, ".exe");
        static readonly string SourceFilename = string.Concat(OutputFilename, ".cs");

        static public string CompilerPath = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csc.exe";

        static public async Task<(bool executionStatus, string compilerReportMsg)> CompileAsync(string sourceCSCode)
        {
            await File.WriteAllTextAsync(SourceFilename, sourceCSCode);

            using var process = Process.Start(GetProcessStartInfo());
            process.WaitForExit();

            File.Delete(SourceFilename);

            if (!process.ExitCode.Equals(0))
            {
                return (true, await GetErrorMessage(process));
            }

            return (false, string.Empty);
        }

        static ProcessStartInfo GetProcessStartInfo()
        {
            return new ProcessStartInfo
            {
                FileName = CompilerPath,
                Arguments = SourceFilename,
                RedirectStandardOutput = true,
            };
        }

        static async Task<string> GetErrorMessage(Process process)
        {
            var errorMessage = await process.StandardOutput.ReadToEndAsync();

            return errorMessage[(errorMessage.LastIndexOf(':') + 2)..];
        }

        static public Task<int> RunAsync()
        {
            return Task.Run(() =>
            {
                using var process = Process.Start(ExecutableName);

                process.WaitForExit();

                return process.ExitCode;
            });
        }
    }
}
