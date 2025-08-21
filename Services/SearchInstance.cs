using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.CodeCleanUp;
using Newtonsoft.Json.Linq;
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
    KnownCommandService knownCommandService,
    Enums.SearchType SearchType,
    Enums.SortType FileSortType,
    Enums.SortType CSharpSortType,
    Enums.SortType MixedSortType
) {
    public List<ListItemFile> Files { get; set; } = new();
    public List<ListItemSymbol> Symbols { get; set; } = new();
    public List<ListItemCommand> Commands { get; set; } = new();
    public List<ListItemKnownCommand> KnownCommands { get; set; } = new();

    public async Task LoadDataAsync() {
        // -----------
        // Files
        // -----------
        if (SearchType is Enums.SearchType.Files or Enums.SearchType.All) {
            var items = await projectFileService.GetFilesInSolutionAsync();
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
    }

    public List<ListItemBase> Search(string searchText) {
        var results = new List<ListItemBase>(20);

        void FilterItems<T>(List<T> items) where T : ListItemBase {
            if (string.IsNullOrEmpty(searchText)) results.AddRange(items);
            else {
                foreach (var item in items) {
                    // Show
                    //var match = FuzzySearch.ScoreFuzzy(item.Name, searchText, true);
                    //if (match.MatchPositions.Length > 0) {
                    //    item.Weight = match.Score;
                    //    results.Add(item);
                    //}

                    // Fastest
                    if (Fts.FuzzyMatch(searchText, item.Name, out var score)) {
                        item.Weight = score;
                        results.Add(item);
                    }

                    //if (item.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) {
                    //    item.Weight = searchText.Length - item.Name.Length;
                    //    results.Add(item);
                    //}
                }
            }
        }

        if (SearchType is Enums.SearchType.Files or Enums.SearchType.All) FilterItems(Files);
        if (SearchType is Enums.SearchType.Symbols or Enums.SearchType.All) FilterItems(Symbols);
        if (SearchType is Enums.SearchType.Commands or Enums.SearchType.All) FilterItems(Commands);
        if (SearchType is Enums.SearchType.KnownCommands or Enums.SearchType.All) FilterItems(KnownCommands);

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
