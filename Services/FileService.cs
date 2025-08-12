using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using QuickJump2022.Models;
using QuickJump2022.Tools;
using Project = EnvDTE.Project;

namespace QuickJump2022.Services;

public class ProjectFileService(DTE Dte) {
    public List<FileItem> GetFilesInSolution() {
        // --
        string GetFolderPath(string fullPath) {
            if (string.IsNullOrEmpty(fullPath)) return string.Empty;
            var lastSlashIdx = fullPath.LastIndexOf('\\');
            return lastSlashIdx >= 0 ? fullPath.Remove(lastSlashIdx).TrimEnd('\\') : string.Empty;
        }
        // --
        var items = GetProjectItems();
        var fileItems = new List<FileItem>();
        foreach (var (item, path) in items) {
            var fileItem = new FileItem {
                FileName = item.Name,
                FullPath = path,
                FolderPath = GetFolderPath(path),
            };
            fileItems.Add(fileItem);
        }
        return fileItems;
    }

    private List<(ProjectItem item, string path)> GetProjectItems() {
        ThreadHelper.ThrowIfNotOnUIThread(nameof(GetProjectItems));
        var list = new List<(ProjectItem, string path)>();
        foreach (Project project in Dte.Solution.Projects) {
            InternalGetProjectItems(project.ProjectItems, list);
        }
        return list;
    }

    private void InternalGetProjectItems(ProjectItems projItems, List<(ProjectItem, string path)> list) {
        ThreadHelper.ThrowIfNotOnUIThread(nameof(InternalGetProjectItems));
        if (projItems is null) {
            return;
        }
        foreach (ProjectItem projItem in projItems) {
            if (projItem.ProjectItems != null && projItem.ProjectItems.Count > 0) {
                InternalGetProjectItems(projItem.ProjectItems, list);
            }
            var path = projItem.TryGetProperty<string>("FullPath");
            if (projItem.Name.Contains(".") && !string.IsNullOrEmpty(path) && File.Exists(path)) {
                list.Add((projItem, path));
            }
        }
    }
}
