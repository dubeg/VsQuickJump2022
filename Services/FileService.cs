using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuickJump2022.Models;

namespace QuickJump2022.Services;

public class ProjectFileService() {

    record ProjectMetadata(
        string ProjectName,
        string ProjectFolderPath
    );

    public async Task<List<FileItem>> GetFilesInSolutionAsync(bool activeProjectOnly = false) {
        // --
        static async Task<IEnumerable<Project>> GetActiveProjectAsync() {
            var project = await VS.Solutions.GetActiveProjectAsync();
            return project is null ? Enumerable.Empty<Project>() : [ project ];
        }
        // --
        var results = new List<FileItem>();
        var projects = activeProjectOnly 
            ? await GetActiveProjectAsync()
            : await VS.Solutions.GetAllProjectsAsync();
        foreach (var project in projects) {
            if (project is null) continue;
            if (!project.IsLoaded) continue;
            var projectName = project.Name;
            var projectPath = project.FullPath;
            var projectFolderPath = GetFolderPath(projectPath);
            var projectMetadata = new ProjectMetadata(projectName, projectFolderPath);
            InternalGetProjectItems(projectMetadata, project.Children, results);
        }
        return results;
    }

    private void InternalGetProjectItems(ProjectMetadata projectMetadata, IEnumerable<SolutionItem> projectItems, List<FileItem> results) {
        foreach (var item in projectItems) {
            if (item.Type == SolutionItemType.PhysicalFile) {
                var projectRelativePath = item.FullPath
                        .Remove(0, projectMetadata.ProjectFolderPath.Length)
                        .TrimStart('\\');
                var fileItem = new FileItem {
                    FileName = item.Text, // or Path.GetFileName(item.FullPath)
                    FullPath = item.FullPath,
                    FolderPath = GetFolderPath(item.FullPath),
                    ProjectName = projectMetadata.ProjectName,
                    ProjectPath = projectMetadata.ProjectFolderPath,
                    ProjectRelativePath = projectRelativePath,
                    ProjectRelativeFolderPath = GetFolderPath(projectRelativePath),
                    ProjectFolderPath = Path.Combine(
                        Path.GetFileName(projectMetadata.ProjectFolderPath),
                        GetFolderPath(projectRelativePath)
                    )
                };
                results.Add(fileItem);
            }
            if (item.Children.Any()) {
                InternalGetProjectItems(projectMetadata, item.Children, results);
            }
        }
    }

    // --

    static string GetFolderPath(string fullPath) {
        if (string.IsNullOrEmpty(fullPath)) return string.Empty;
        var lastSlashIdx = fullPath.LastIndexOf('\\');
        return lastSlashIdx >= 0 ? fullPath.Remove(lastSlashIdx).TrimEnd('\\') : string.Empty;
    }
}
