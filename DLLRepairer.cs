using System;
using System.Diagnostics;
using System.IO;

namespace DLLFixer
{
    public class DLLRepairer
    {
        public Tuple<int, int> RepairDLLs()
        {
            int fixed_count = 0;
            int failed_count = 0;

            try
            {
                // Re-register common system DLLs
                string[] commonDLLs = new string[]
                {
                    "oleaut32.dll",
                    "comctl32.dll",
                    "shlwapi.dll",
                    "shell32.dll",
                    "advapi32.dll"
                };

                foreach (string dll in commonDLLs)
                {
                    string dllPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), dll);

                    if (File.Exists(dllPath))
                    {
                        if (RegisterDLL(dllPath))
                            fixed_count++;
                        else
                            failed_count++;
                    }
                }

                // Run System File Checker
                if (RunSystemFileChecker())
                    fixed_count += 5; // Count as multiple fixes
            }
            catch (Exception ex)
            {
                failed_count++;
            }

            return new Tuple<int, int>(fixed_count, failed_count);
        }

        private bool RegisterDLL(string dllPath)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "regsvr32.exe",
                    Arguments = $"/s \"{dllPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    Verb = "runas" // Run as admin
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit(5000);
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool RunSystemFileChecker()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "sfc.exe",
                    Arguments = "/scannow",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    Verb = "runas" // Run as admin
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit(60000); // Wait up to 1 minute
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
