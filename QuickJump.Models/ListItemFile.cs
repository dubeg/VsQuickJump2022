using EnvDTE;
using QuickJump2022.Tools;

namespace QuickJump2022.Models;

public class ListItemFile : ListItemBase {
    private ProjectItem m_ProjectItem;
    public string FullPath;

    public ListItemFile(ProjectItem projFile, string srcTxt) {
        ThreadHelper.ThrowIfNotOnUIThread(".ctor");
        m_ProjectItem = projFile;
        Weight = projFile.Name.Length - srcTxt.Length;
        Name = projFile.Name;
        FullPath = projFile.TryGetProperty<string>("FullPath");
        int lastSlash = FullPath.LastIndexOf('\\');
        Description = FullPath.Remove(lastSlash).TrimEnd('\\');
        IconImage = Utilities.GetMimeTypeIcon(FullPath);
    }

    public override void Go() {
        ThreadHelper.ThrowIfNotOnUIThread("Go");
        if (m_ProjectItem != null) {
            QuickJumpData.Instance.Dte.ItemOperations.OpenFile(FullPath, "{00000000-0000-0000-0000-000000000000}");
            object selection = m_ProjectItem.Document.Selection;
            if (selection is TextSelection textSelection) {
                textSelection.GotoLine(Line, false);
            }
        }
    }

    public int CompareTo(ListItemFile other) {
        if (other == null) {
            return 1;
        }
        return string.Compare(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
    }
}
