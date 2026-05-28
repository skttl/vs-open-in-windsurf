using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Forms;
using Microsoft;

namespace OpenInWindsurf
{
    internal sealed class OpenInWindsurfCommand
    {
        private readonly Package _package;
        private readonly Options _options;

        private OpenInWindsurfCommand(Package package, Options options)
        {
            _package = package;
            _options = options;

            var commandService = (OleMenuCommandService)ServiceProvider.GetService(typeof(IMenuCommandService));

            if (commandService != null)
            {
                var menuCommandID = new CommandID(PackageGuids.guidOpenInWindsurfCmdSet, PackageIds.OpenInWindsurf);
                var menuItem = new MenuCommand(OpenFolderInWindsurf, menuCommandID);
                commandService.AddCommand(menuItem);

                var currentCommandID = new CommandID(PackageGuids.guidOpenCurrentInWindsurfCmdSet, PackageIds.OpenCurrentInWindsurf);
                var currentItem = new MenuCommand(OpenCurrentFileInWindsurf, currentCommandID);
                commandService.AddCommand(currentItem);
            }
        }

        public static OpenInWindsurfCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package, Options options)
        {
            Instance = new OpenInWindsurfCommand(package, options);
        }

        private void OpenCurrentFileInWindsurf(object sender, EventArgs e)
        {
            try
            {
                var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
                Assumes.Present(dte);

                var activeDocument = dte.ActiveDocument;

                if (activeDocument != null)
                {
                    var path = activeDocument.FullName;

                    if (!string.IsNullOrEmpty(path))
                    {
                        int line = 0;
                        int column = 0;

                        if (activeDocument.Selection is TextSelection selection)
                        {
                            line = selection.ActivePoint.Line;
                            column = selection.ActivePoint.LineCharOffset;
                        }

                        OpenWindsurf(path, line, column);
                    }
                    else
                    {
                        MessageBox.Show("Couldn't resolve the folder");
                    }
                }
                else
                {
                    MessageBox.Show("Couldn't find active document");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void OpenFolderInWindsurf(object sender, EventArgs e)
        {
            try
            {
                var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
                Assumes.Present(dte);

                string path = ProjectHelpers.GetSelectedPath(dte, _options.OpenSolutionProjectAsRegularFile);

                if (!string.IsNullOrEmpty(path))
                {
                    int line = 0;
                    int column = 0;

                    if (dte.ActiveDocument?.Selection is TextSelection selection)
                    {
                        line = selection.ActivePoint.Line;
                        column = selection.ActivePoint.LineCharOffset;
                    }

                    OpenWindsurf(path, line, column);
                }
                else
                {
                    MessageBox.Show("Couldn't resolve the folder");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void OpenWindsurf(string path, int line = 0, int column = 0)
        {
            EnsurePathExist();

            bool isMultipleFiles = path.Contains("\" \"");
            bool isDirectory = !isMultipleFiles && Directory.Exists(path);

            var args = isMultipleFiles
                ? path
                : isDirectory
                    ? "."
                    : line > 0
                        ? column > 0
                            ? $"-g \"{path}:{line}:{column}\""
                            : $"-g \"{path}:{line}\""
                        : $"\"{path}\"";

            if (!string.IsNullOrEmpty(_options.CommandLineArguments))
            {
                args = $"{args} {_options.CommandLineArguments}";
            }

            var start = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = $"\"{_options.PathToExe}\"",
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
            };

            if (isDirectory)
            {
                start.WorkingDirectory = path;
            }

            using (System.Diagnostics.Process.Start(start))
            {
            }
        }

        private void EnsurePathExist()
        {
            if (File.Exists(_options.PathToExe))
                return;

            if (!string.IsNullOrEmpty(WindsurfDetect.InRegistry()))
            {
                SaveOptions(_options, WindsurfDetect.InRegistry());
            }
            else if (!string.IsNullOrEmpty(WindsurfDetect.InEnvVarPath()))
            {
                SaveOptions(_options, WindsurfDetect.InEnvVarPath());
            }
            else if (!string.IsNullOrEmpty(WindsurfDetect.InLocalAppData()))
            {
                SaveOptions(_options, WindsurfDetect.InLocalAppData());
            }
            else
            {
                var box = MessageBox.Show(
                    "I can't find Windsurf (windsurf.exe). Would you like to help me find it?", Vsix.Name,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (box == DialogResult.No)
                    return;

                var dialog = new OpenFileDialog
                {
                    DefaultExt = ".exe",
                    FileName = "windsurf.exe",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    CheckFileExists = true
                };

                var result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    SaveOptions(_options, dialog.FileName);
                }
            }
        }

        private void SaveOptions(Options options, string path)
        {
            options.PathToExe = path;
            options.SaveSettingsToStorage();
        }
    }
}
