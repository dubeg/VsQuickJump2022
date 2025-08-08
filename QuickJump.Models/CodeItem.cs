using EnvDTE;
using QuickJump2022.Data;

namespace QuickJump2022.Models;

/// <summary>
/// Symbol data returned by the roslyin symbol retrieval method.
/// </summary>
public class CodeItem {
	public Document ProjDocument;
	public Enums.EBindType BindType;
	public Enums.EAccessType AccessType;
	public string Name;
	public string NameOnly;
	public string Type;
	public int Line;
}
