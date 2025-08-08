using EnvDTE;

namespace QuickJump2022.Models;

public class ListItemFile : ListItemBase {
    public ProjectItem ProjectItem { get; init; }
    public string FullPath;
}
