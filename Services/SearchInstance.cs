using System.Collections.Generic;
using System.Linq;
using QuickJump2022.Models;
using QuickJump2022.Tools;

namespace QuickJump2022.Services;

/// <summary>
/// A search instance that must be loaded once at search window startup,
/// and then filtered using an input string.
/// It should be discarded when the search form is dismissed.
/// </summary>
public class SearchInstance(
    ProjectFileService projectFileService,
    SymbolService symbolService,
    CommandService commandService,
    Enums.ESearchType SearchType,
    Enums.SortType FileSortType,
    Enums.SortType CSharpSortType,
    Enums.SortType MixedSortType
) {
    public List<ListItemFile> Files { get; set; } = new();
    public List<ListItemSymbol> Symbols { get; set; } = new();
    public List<ListItemCommand> Commands { get; set; } = new();

    public async Task LoadDataAsync() {
        // -----------
        // Files
        // -----------
        if (SearchType is Enums.ESearchType.Files or Enums.ESearchType.All) {
            var items = projectFileService.GetFilesInSolution();
            Files = items.Select(x => ListItemFile.FromFileItem(x)).ToList();
        }
        // -----------
        // Symbols
        // -----------
        if (SearchType is Enums.ESearchType.Methods or Enums.ESearchType.All) {
            var items = await symbolService.GetCodeItemsForActiveDocumentAsync();
            Symbols = items.Select(x => ListItemSymbol.FromCodeItem(x)).ToList();
        }
        // -----------
        // Commands
        // -----------
        if (SearchType is Enums.ESearchType.Commands or Enums.ESearchType.All) {
            var items = commandService.GetCachedCommands();
            Commands = items.Select(x => ListItemCommand.FromCommandItem(x)).ToList();
        }
    }

    public List<ListItemBase> Search(string searchText) {
        var results = new List<ListItemBase>(20);

        void FilterItems<T>(List<T> items) where T : ListItemBase {
            if (string.IsNullOrEmpty(searchText)) results.AddRange(items);
            else {
                foreach (var item in items) {
                    if (Fts.FuzzyMatch(searchText, item.Name, out var score)) {
                        item.Weight = score;
                        results.Add(item);
                    }
                }
            }
        }

        if (SearchType is Enums.ESearchType.Files or Enums.ESearchType.All) FilterItems(Files);
        if (SearchType is Enums.ESearchType.Methods or Enums.ESearchType.All) FilterItems(Symbols);
        if (SearchType is Enums.ESearchType.Commands or Enums.ESearchType.All) FilterItems(Commands);

        // -------------------
        // Fuzzy search
        // -------------------
        if (!string.IsNullOrEmpty(searchText)) {
            // A bit slow: FuzzySearch.SortByFuzzyScore(results, searchText, x => x.Name, true);
            
            // Fast
            results.Sort(Tools.Sort.WeightReverse);
        }
        else {
            // -------------------
            // Standard search
            // -------------------
            Sort(results, SearchType switch {
                Enums.ESearchType.Files => FileSortType,
                Enums.ESearchType.Methods => CSharpSortType,
                Enums.ESearchType.Commands => Enums.SortType.Alphabetical,
                Enums.ESearchType.All => MixedSortType,
                _ => throw new NotImplementedException()
            });
        }
        return results;
    }

    private void Sort(List<ListItemBase> items, Enums.SortType sortType) {
        switch (sortType) {
            case Enums.SortType.Alphabetical: items.Sort(Tools.Sort.Alphabetical); break;
            case Enums.SortType.AlphabeticalReverse: items.Sort(Tools.Sort.AlphabeticalReverse); break;
            case Enums.SortType.LineNumber: items.Sort(Tools.Sort.LineNumber); break;
            case Enums.SortType.LineNumberReverse: items.Sort(Tools.Sort.LineNumberReverse); break;
            case Enums.SortType.Weight: items.Sort(Tools.Sort.Weight); break;
            case Enums.SortType.WeightReverse: items.Sort(Tools.Sort.WeightReverse); break;
        }
    }

    private int Compare(ListItemBase a, ListItemBase b, Enums.SortType sortType) {
        switch (sortType) {
            case Enums.SortType.Alphabetical: return Tools.Sort.Alphabetical(a, b);
            case Enums.SortType.AlphabeticalReverse: return Tools.Sort.AlphabeticalReverse(a, b);
            case Enums.SortType.LineNumber: return Tools.Sort.LineNumber(a, b);
            case Enums.SortType.LineNumberReverse: return Tools.Sort.LineNumberReverse(a, b);
            case Enums.SortType.Weight: return Tools.Sort.Weight(a, b);
            case Enums.SortType.WeightReverse: return Tools.Sort.WeightReverse(a, b);
            default: return Tools.Sort.Alphabetical(a, b);
        }
    }
}
