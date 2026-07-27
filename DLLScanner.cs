using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace DLLFixer
{
    public class DLLScanner
    {
        private List<string> issues = new List<string>();

        public List<string> ScanSystem()
        {
            issues.Clear();

            // Scan common system paths
            ScanDirectory(Environment.GetFolderPath(Environment.SpecialFolder.System));
            ScanDirectory(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));
            ScanRegistryDLLs();

            return issues;
        }

        private void ScanDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return;

                string[] dllFiles = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);

                foreach (string dll in dllFiles)
                {
                    if (!File.Exists(dll))
                    {
                        issues.Add($"Missing: {Path.GetFileName(dll)} at {path}");
                    }
                    else if (!IsValidDLL(dll))
                    {
                        issues.Add($"Corrupted: {Path.GetFileName(dll)} at {path}");
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error scanning {path}: {ex.Message}");
            }
        }

        private bool IsValidDLL(string filePath)
        {
            try
            {
                // Check if file is readable and has DLL signature
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] header = new byte[2];
                    fs.Read(header, 0, 2);
                    // MZ signature for PE files (DLL/EXE)
                    return header[0] == 0x4D && header[1] == 0x5A;
                }
            }
            catch
            {
                return false;
            }
        }

        private void ScanRegistryDLLs()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Classes\CLSID");
                if (key != null)
                {
                    string[] subKeyNames = key.GetSubKeyNames();
                    int scanned = 0;

                    foreach (string subKeyName in subKeyNames)
                    {
                        if (scanned > 50) break; // Limit registry scan

                        try
                        {
                            RegistryKey subKey = key.OpenSubKey(subKeyName);
                            RegistryKey inprocKey = subKey?.OpenSubKey("InprocServer32");
                            object dllPath = inprocKey?.GetValue(null);

                            if (dllPath != null && !File.Exists(dllPath.ToString()))
                            {
                                issues.Add($"Missing registry DLL: {dllPath}");
                            }

                            scanned++;
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"Error scanning registry: {ex.Message}");
            }
        }
    }
}
