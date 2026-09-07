using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Vision_Align
{
    internal static class NativeRuntimeBootstrapper
    {
        private const string HalconArchitecture = "x64-win64";
        private const string HalconDllName = "halcon.dll";
        private static IntPtr s_halconModule = IntPtr.Zero;

        internal static string HalconRuntimePath { get; private set; }

        private static string ApplicationDirectory
        {
            get
            {
                string assemblyLocation = typeof(NativeRuntimeBootstrapper).Assembly.Location;
                return string.IsNullOrWhiteSpace(assemblyLocation)
                    ? AppDomain.CurrentDomain.BaseDirectory
                    : Path.GetDirectoryName(assemblyLocation);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string fileName);

        internal static void ConfigureHalconRuntime()
        {
            if (s_halconModule != IntPtr.Zero)
                return;

            if (!Environment.Is64BitProcess)
            {
                throw new BadImageFormatException(
                    "Vision_Align requires the 64-bit HALCON 18.11 runtime, but the process is running as 32-bit.");
            }

            List<string> searchedFiles = new List<string>();
            List<string> loadFailures = new List<string>();

            foreach (string directory in GetHalconRuntimeDirectories())
            {
                string halconPath = Path.Combine(directory, HalconDllName);
                searchedFiles.Add(halconPath);

                if (!File.Exists(halconPath))
                    continue;

                ConfigureProcessEnvironment(directory);
                s_halconModule = LoadLibrary(halconPath);
                if (s_halconModule != IntPtr.Zero)
                {
                    HalconRuntimePath = halconPath;
                    return;
                }

                int errorCode = Marshal.GetLastWin32Error();
                loadFailures.Add(string.Format(
                    "{0} (Win32 {1}: {2})",
                    halconPath,
                    errorCode,
                    new Win32Exception(errorCode).Message));
            }

            string details = loadFailures.Count > 0
                ? "HALCON was found but could not be loaded. Check its x64 architecture and Microsoft Visual C++ runtime dependencies:\r\n- "
                    + string.Join("\r\n- ", loadFailures)
                : "HALCON was not found in the application folder, HALCONROOT, PATH, or the standard MVTec installation folder.";
            string searchedPathText = string.Join("\r\n- ", searchedFiles.Take(12));
            if (searchedFiles.Count > 12)
                searchedPathText += string.Format("\r\n- ... and {0} additional PATH locations", searchedFiles.Count - 12);

            throw new DllNotFoundException(string.Format(
                "The HALCON 18.11 x64 native runtime ({0}) is unavailable.\r\n{1}\r\nSearched paths:\r\n- {2}\r\nInstall or repair HALCON 18.11 Steady x64 Runtime and set HALCONROOT, then restart Vision_Align.",
                HalconDllName,
                details,
                searchedPathText));
        }

        private static IEnumerable<string> GetHalconRuntimeDirectories()
        {
            List<string> directories = new List<string>();
            string applicationDirectory = ApplicationDirectory;

            AddDirectory(directories, applicationDirectory);
            AddDirectory(directories, Path.Combine(applicationDirectory, "HALCON", HalconArchitecture));
            AddDirectory(directories, Path.Combine(applicationDirectory, "HALCON", "bin", HalconArchitecture));

            string halconRoot = Environment.GetEnvironmentVariable("HALCONROOT");
            string halconArch = Environment.GetEnvironmentVariable("HALCONARCH");
            if (!string.IsNullOrWhiteSpace(halconRoot))
            {
                if (!string.IsNullOrWhiteSpace(halconArch))
                    AddDirectory(directories, Path.Combine(halconRoot, "bin", halconArch));

                AddDirectory(directories, Path.Combine(halconRoot, "bin", HalconArchitecture));
            }

            AddMvtEcInstallations(directories, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            AddMvtEcInstallations(directories, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));

            string path = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrWhiteSpace(path))
            {
                foreach (string entry in path.Split(Path.PathSeparator))
                    AddDirectory(directories, entry.Trim().Trim('"'));
            }

            return directories;
        }

        private static void AddMvtEcInstallations(List<string> directories, string programFiles)
        {
            if (string.IsNullOrWhiteSpace(programFiles))
                return;

            string mvtecDirectory = Path.Combine(programFiles, "MVTec");
            if (!Directory.Exists(mvtecDirectory))
                return;

            try
            {
                foreach (string installation in Directory.GetDirectories(mvtecDirectory, "HALCON-18.11*")
                    .OrderByDescending(item => item, StringComparer.OrdinalIgnoreCase))
                {
                    AddDirectory(directories, Path.Combine(installation, "bin", HalconArchitecture));
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private static void AddDirectory(List<string> directories, string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                return;

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(directory.Trim());
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
            {
                return;
            }

            if (!directories.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
                directories.Add(fullPath);
        }

        private static void ConfigureProcessEnvironment(string runtimeDirectory)
        {
            string currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            bool alreadyInPath = currentPath.Split(Path.PathSeparator)
                .Any(entry => string.Equals(
                    entry.Trim().Trim('"').TrimEnd(Path.DirectorySeparatorChar),
                    runtimeDirectory.TrimEnd(Path.DirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase));

            if (!alreadyInPath)
                Environment.SetEnvironmentVariable("PATH", runtimeDirectory + Path.PathSeparator + currentPath);

            Environment.SetEnvironmentVariable("HALCONARCH", HalconArchitecture);

            DirectoryInfo architectureDirectory = new DirectoryInfo(runtimeDirectory);
            if (architectureDirectory.Parent != null
                && architectureDirectory.Parent.Parent != null
                && string.Equals(architectureDirectory.Name, HalconArchitecture, StringComparison.OrdinalIgnoreCase)
                && string.Equals(architectureDirectory.Parent.Name, "bin", StringComparison.OrdinalIgnoreCase))
            {
                Environment.SetEnvironmentVariable("HALCONROOT", architectureDirectory.Parent.Parent.FullName);
            }
        }
    }
}
