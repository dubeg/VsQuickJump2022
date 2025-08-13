using EnvDTE;

namespace QuickJump2022.Models;

public class ListItemFile : ListItemBase {
    private FileItem _item;

    public override string Name => _item.FileName;
    public override string Description => _item.ProjectFolderPath;
    public string FileExtension => _item.FileExtension;
    public string FilePath => _item.FullPath;

    public static ListItemFile FromFileItem(FileItem item) 
        => new ListItemFile { _item = item };
}
