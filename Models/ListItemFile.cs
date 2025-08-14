using EnvDTE;
using QuickJump2022.Tools;

namespace QuickJump2022.Models;

public class ListItemFile : ListItemBase {
    private FileItem _item;
    private string _desc;

    public override string Name => _item.FileName;
    public override string Description => _desc;
    public string FileExtension => _item.FileExtension;
    public string FilePath => _item.FullPath;

    public static ListItemFile FromFileItem(FileItem item)
        => new ListItemFile { 
            _item = item,
            _desc = item.ProjectFolderPath.ReversePath()
        };
}
