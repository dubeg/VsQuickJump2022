using QuickJump2022.Models;

namespace QuickJump2022.Tools;

public static class Sort {
    public static int Alphabetical(ListItemBase b1, ListItemBase b2) {
        return string.Compare(b1.Name, b2.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    public static int AlphabeticalReverse(ListItemBase b1, ListItemBase b2) {
        return string.Compare(b2.Name, b1.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    public static int LineNumber(ListItemBase b1, ListItemBase b2) {
        return b1.Line.CompareTo(b2.Line);
    }

    public static int LineNumberReverse(ListItemBase b1, ListItemBase b2) {
        return b2.Line.CompareTo(b1.Line);
    }

    public static int Weight(ListItemBase b1, ListItemBase b2) {
        return b1.Weight.CompareTo(b2.Weight);
    }

    public static int WeightReverse(ListItemBase b1, ListItemBase b2) {
        return b2.Weight.CompareTo(b1.Weight);
    }

    public static int Fuzzy(ListItemBase b1, ListItemBase b2) {
        // This method is used when fuzzy search is explicitly selected as sort type
        // The actual fuzzy scoring is handled in SearchForm.RefreshList()
        return b1.Weight.CompareTo(b2.Weight);
    }

    public static int FuzzyReverse(ListItemBase b1, ListItemBase b2) {
        // This method is used when fuzzy search is explicitly selected as sort type
        // The actual fuzzy scoring is handled in SearchForm.RefreshList()
        return b2.Weight.CompareTo(b1.Weight);
    }
}
