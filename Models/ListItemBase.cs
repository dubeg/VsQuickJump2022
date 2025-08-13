namespace QuickJump2022.Models;

public abstract class ListItemBase {
    public int Weight;
    public virtual string Name => string.Empty;
    public virtual string Type => string.Empty;
    public virtual string Description => string.Empty;
    public override string ToString() => Name;
}
