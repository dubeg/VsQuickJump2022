using Microsoft.CodeAnalysis.Elfie.Model;

namespace QuickJump2022.Models;

public class ListItemSymbol : ListItemBase {
    public CodeItem Item { get; init; }
    
    public override string Name => Item.Name;
    public override string Description => $"{FormatAccessType(Item.AccessType)} {Item.BindType}";
    public override string Type => !string.IsNullOrEmpty(Item.Type) ? $" -> {Item.Type}" : "";
    public int Line => Item.Line;

    public static ListItemSymbol FromCodeItem(CodeItem item) 
        => new ListItemSymbol { Item = item };

    string FormatAccessType(Enums.EAccessType accessType) {
        var str = accessType.ToString();
        if (str.Contains(",")) {
            str = string.Empty;
            var accessTypes = str.Split(',');
            for (var i = accessTypes.Length - 1; i > 0; i--) {
                str = str + accessTypes[i] + " ";
            }
            str = str.TrimEnd(' ');
        }
        return str;
    }
}
