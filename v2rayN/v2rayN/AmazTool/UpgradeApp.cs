﻿using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace AmazTool
{
    internal class UpgradeApp
    {
        public static void Upgrade(string fileName)
        {
            Console.WriteLine($"{Resx.Resource.StartUnzipping}\n{fileName}");

            Waiting(3);

            if (!File.Exists(fileName))
            {
                Console.WriteLine(Resx.Resource.UpgradeFileNotFound);
                return;
            }

            Console.WriteLine(Resx.Resource.TryTerminateProcess);
            try
            {
                var existing = Process.GetProcessesByName(Utils.V2rayN);
                foreach (var pp in existing)
                {
                    var path = pp.MainModule?.FileName ?? "";
                    if (path.StartsWith(Utils.GetPath(Utils.V2rayN)))
                    {
                        pp?.Kill();
                        pp?.WaitForExit(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                // Access may be denied without admin right. The user may not be an administrator.
                Console.WriteLine(Resx.Resource.FailedTerminateProcess + ex.StackTrace);
            }

            Console.WriteLine(Resx.Resource.StartUnzipping);
            StringBuilder sb = new();
            try
            {
                string thisAppOldFile = $"{Utils.GetExePath()}.tmp";
                File.Delete(thisAppOldFile);
                string splitKey = "/";

                using ZipArchive archive = ZipFile.OpenRead(fileName);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    try
                    {
                        if (entry.Length == 0)
                        {
                            continue;
                        }

                        Console.WriteLine(entry.FullName);

                        var lst = entry.FullName.Split(splitKey);
                        if (lst.Length == 1) continue;
                        string fullName = string.Join(splitKey, lst[1..lst.Length]);

                        if (string.Equals(Utils.GetExePath(), Utils.GetPath(fullName), StringComparison.OrdinalIgnoreCase))
                        {
                            File.Move(Utils.GetExePath(), thisAppOldFile);
                        }

                        string entryOutputPath = Utils.GetPath(fullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(entryOutputPath)!);
                        entry.ExtractToFile(entryOutputPath, true);

                        Console.WriteLine(entryOutputPath);
                    }
                    catch (Exception ex)
                    {
                        sb.Append(ex.StackTrace);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resx.Resource.FailedUpgrade + ex.StackTrace);
                //return;
            }
            if (sb.Length > 0)
            {
                Console.WriteLine(Resx.Resource.FailedUpgrade + sb.ToString());
                //return;
            }

            Console.WriteLine(Resx.Resource.Restartv2rayN);
            Waiting(2);

            Utils.StartV2RayN();
        }

        public static void Waiting(int second)
        {
            for (var i = second; i > 0; i--)
            {
                Console.WriteLine(i);
                Thread.Sleep(1000);
            }
        }
    }
}