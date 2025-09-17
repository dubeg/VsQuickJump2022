using System.Collections.Generic;
using System.Linq;
using QuickJump2022.Models;

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
    KnownCommandService knownCommandService,
    FastFetchCommandService fastFetchCommandService,
    Enums.SearchType SearchType,
    Enums.FileSearchScope FileScope,
    Enums.SortType FileSortType,
    Enums.SortType CSharpSortType,
    Enums.SortType MixedSortType
) {
    public List<ListItemFile> Files { get; set; } = new();
    public List<ListItemSymbol> Symbols { get; set; } = new();
    public List<ListItemCommand> Commands { get; set; } = new();
    public List<ListItemKnownCommand> KnownCommands { get; set; } = new();
    public List<ListItemFastFetchCommand> FastFetchCommands { get; set; } = new();

    public async Task LoadDataAsync() {
        // -----------
        // Files
        // -----------
        if (SearchType is Enums.SearchType.Files or Enums.SearchType.All) {
            var items = await projectFileService.GetFilesInSolutionAsync(activeProjectOnly: FileScope == Enums.FileSearchScope.ActiveProject);
            Files = items.Select(x => ListItemFile.FromFileItem(x)).ToList();
        }
        // -----------
        // Symbols
        // -----------
        if (SearchType is Enums.SearchType.Symbols or Enums.SearchType.All) {
            var items = await symbolService.GetCodeItemsForActiveDocumentAsync();
            Symbols = items.Select(x => ListItemSymbol.FromCodeItem(x)).ToList();
        }
        // -----------
        // Commands
        // -----------
        if (SearchType is Enums.SearchType.Commands or Enums.SearchType.All) {
            var items = commandService.GetCommands();
            Commands = items.Select(x => ListItemCommand.FromCommandItem(x)).ToList();
        }
        // -----------
        // Known Commands
        // -----------
        if (SearchType is Enums.SearchType.KnownCommands or Enums.SearchType.All) {
            var items = knownCommandService.GetCommands();
            KnownCommands = items.Select(x => ListItemKnownCommand.FromKnownCommandMapping(x)).ToList();
        }
        // -----------
        // Fast Fetch Commands
        // -----------
        if (SearchType is Enums.SearchType.FastFetchCommands or Enums.SearchType.All) {
            var items = await fastFetchCommandService.GetCommandsAsync();
            FastFetchCommands = items.Select(x => ListItemFastFetchCommand.FromFastFetchItem(x)).ToList();
        }
    }

    public List<ListItemBase> Search(string searchText) {
        var results = new List<ListItemBase>(20);
        // ----------------------------------------
        // Note: works around a limitation where FlxCs won't match an all-caps searchText
        // to any result that isn't also all caps.
        // ----------------------------------------
        if (searchText.All(x => char.IsUpper(x))) { 
            searchText = searchText.ToLower();
        }
        void FilterItems<T>(List<T> items) where T : ListItemBase {
            if (string.IsNullOrEmpty(searchText)) results.AddRange(items);
            else {
                foreach (var item in items) {
                    var result = FlxCs.Flx.Score(item.Name, searchText);
                    if (result is not null) {
                        item.Weight = result.score;
                        results.Add(item);
                    }
                }
            }
        }
        if (SearchType is Enums.SearchType.Files or Enums.SearchType.All) FilterItems(Files);
        if (SearchType is Enums.SearchType.Symbols or Enums.SearchType.All) FilterItems(Symbols);
        if (SearchType is Enums.SearchType.Commands or Enums.SearchType.All) FilterItems(Commands);
        if (SearchType is Enums.SearchType.KnownCommands or Enums.SearchType.All) FilterItems(KnownCommands);
        if (SearchType is Enums.SearchType.FastFetchCommands or Enums.SearchType.All) FilterItems(FastFetchCommands);
        // -------------------
        // Fuzzy search
        // -------------------
        if (!string.IsNullOrEmpty(searchText)) {
            results.Sort((a, b) => {
                if (a.Weight != b.Weight) return Tools.Sort.WeightReverse(a, b);
                return Tools.Sort.Alphabetical(a, b);
            });
        }
        else {
            // -------------------
            // Standard search
            // -------------------
            Sort(results, SearchType switch {
                Enums.SearchType.Files => FileSortType,
                Enums.SearchType.Symbols => CSharpSortType,
                Enums.SearchType.Commands => Enums.SortType.Alphabetical,
                Enums.SearchType.KnownCommands => Enums.SortType.Alphabetical,
                Enums.SearchType.FastFetchCommands => Enums.SortType.Alphabetical,
                Enums.SearchType.All => MixedSortType,
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
