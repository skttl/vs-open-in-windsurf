using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;

namespace OpenInWindsurf
{
    internal static class ProjectHelpers
    {
        public static string GetSelectedPath(DTE2 dte, bool openSolutionProjectAsRegularFile)
        {
            var items = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;
            var files = new List<string>();

            foreach (UIHierarchyItem selItem in items)
            {
                ProjectItem item = selItem.Object as ProjectItem;

                if (item != null)
                    files.Add(item.GetFilePath());

                Project proj = selItem.Object as Project;

                if (proj != null)
                    return openSolutionProjectAsRegularFile ? proj.FileName : proj.GetRootFolder();

                Solution sol = selItem.Object as Solution;

                if (sol != null)
                    return openSolutionProjectAsRegularFile ? sol.FullName : Path.GetDirectoryName(sol.FileName);
            }

            return files.Count > 0 ? String.Join(" ", files.Select(f => $"\"{f}\"")) : null;
        }

        public static string GetFilePath(this ProjectItem item)
        {
            return item.FileNames[1];
        }

        public static string GetRootFolder(this Project project)
        {
            if (string.IsNullOrEmpty(project.FullName))
                return null;

            string fullPath;

            try
            {
                fullPath = project.Properties.Item("FullPath").Value as string;
            }
            catch (ArgumentException)
            {
                try
                {
                    fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                }
                catch (ArgumentException)
                {
                    fullPath = project.Properties.Item("ProjectPath").Value as string;
                }
            }

            if (string.IsNullOrEmpty(fullPath))
                return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : null;

            if (Directory.Exists(fullPath))
                return fullPath;

            if (File.Exists(fullPath))
                return Path.GetDirectoryName(fullPath);

            return null;
        }
    }
}
