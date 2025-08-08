using EnvDTE;
using Microsoft.VisualStudio.Shell;
using QuickJump2022.Tools;

namespace QuickJump2022.Models;

public class ListItemCSharp : ListItemBase
{
	private Document m_Document;

	public ListItemCSharp(CodeItem codeItem, string srcTxt)
	{
		ThreadHelper.ThrowIfNotOnUIThread(".ctor");
		m_Document = codeItem.ProjDocument;
		Weight = codeItem.Name.Length - srcTxt.Length;
		string accessTypeString = codeItem.AccessType.ToString();
		if (accessTypeString.Contains(","))
		{
			accessTypeString = string.Empty;
			string[] accessTypes = accessTypeString.Split(',');
			for (int i = accessTypes.Length - 1; i > 0; i--)
			{
				accessTypeString = accessTypeString + accessTypes[i] + " ";
			}
			accessTypeString = accessTypeString.TrimEnd(' ');
		}
		Description = $"{accessTypeString} {codeItem.BindType}";
		Name = codeItem.NameOnly;
		Type = codeItem.Type;
		Line = codeItem.Line;
		IconImage = Utilities.GetCodeIcon(codeItem.BindType);
	}

	public override void Go()
	{
		ThreadHelper.ThrowIfNotOnUIThread("Go");
		if (m_Document != null)
		{
			object selection = m_Document.Selection;
			object obj = ((selection is TextSelection) ? selection : null);
			((TextSelection)obj).GotoLine(Line, false);
			((TextSelection)obj).StartOfLine((vsStartOfLineOptions)1, false);
		}
	}
}
