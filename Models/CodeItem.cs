using Microsoft.CodeAnalysis;

namespace QuickJump2022.Models;

/// <summary>
/// Symbol data returned by the roslyin symbol retrieval method.
/// </summary>
public class CodeItem {
	public DocumentId DocumentId;
	public Enums.TokenType BindType;
	public Enums.ModifierType AccessType;
	public string Name;
	public string NameOnly;
	public string Type;
	public int Line;
}
