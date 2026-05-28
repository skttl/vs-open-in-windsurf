using Microsoft.Win32;
using System;
using System.IO;

namespace OpenInWindsurf
{
    internal static class WindsurfDetect
    {
        internal static string InRegistry()
        {
            var key = Registry.CurrentUser;
            var name = "Icon";
            try
            {
                var subKey = key.OpenSubKey(@"SOFTWARE\Classes\*\shell\Windsurf\");
                var value = subKey.GetValue(name).ToString();
                if (File.Exists(value))
                {
                    return value;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        internal static string InLocalAppData()
        {
            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");

            var windsurfPartDir = @"Programs\Windsurf";
            var windsurfDir = Path.Combine(localAppData, windsurfPartDir);
            var drives = DriveInfo.GetDrives();

            foreach (var drive in drives)
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    var path = Path.Combine(drive.Name[0] + windsurfDir.Substring(1), "windsurf.exe");
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }

            return null;
        }

        internal static string InEnvVarPath()
        {
            var envPath = Environment.GetEnvironmentVariable("Path");
            var paths = envPath.Split(';');
            var parentDir = "Windsurf";
            foreach (var path in paths)
            {
                if (path.ToLower().Contains("windsurf"))
                {
                    var temp = Path.Combine(
                        path.Substring(0, path.IndexOf(parentDir, StringComparison.InvariantCultureIgnoreCase)),
                        parentDir, "windsurf.exe");
                    if (File.Exists(temp))
                    {
                        return temp;
                    }
                }
            }
            return null;
        }
    }
}
