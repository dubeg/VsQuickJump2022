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
    Enums.ESearchType SearchType,
    Enums.SortType FileSortType,
    Enums.SortType CSharpSortType,
    Enums.SortType MixedSortType
) {
    public List<ListItemFile> Files { get; set; } = new();
    public List<ListItemSymbol> Symbols { get; set; } = new();

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
    }

    public List<ListItemBase> Search(string searchText) {
        var results = new List<ListItemBase>(20);
        
        bool FuzzyMatch(string str, string srcStr) {
            if (srcStr.Length > 0) {
                return FuzzySearch.IsMatch(str, srcStr);
            }
            return true;
        }

        if (SearchType is Enums.ESearchType.Files or Enums.ESearchType.All && Files.Count > 0) {
            foreach (var file in Files) {
                if (FuzzyMatch(file.Name, searchText)) {
                    results.Add(file);
                }
            }
        }

        if (SearchType is Enums.ESearchType.Methods or Enums.ESearchType.All && Symbols.Count > 0) {
            foreach (var symbol in Symbols) {
                if (FuzzyMatch(symbol.Name, searchText)) {
                    results.Add(symbol);
                }
            }
        }
        // -------------------
        // Fuzzy search
        // -------------------
        if (!string.IsNullOrEmpty(searchText)) {
            foreach (var item in results) {
                var fuzzyScore = FuzzySearch.ScoreFuzzy(item.Name, searchText);
                item.Weight = fuzzyScore.Score;
            }

            results.Sort((a, b) => {
                if (a.Weight != b.Weight) {
                    return b.Weight.CompareTo(a.Weight);
                }
                var sortType = SearchType switch {
                    Enums.ESearchType.Files => FileSortType,
                    Enums.ESearchType.Methods => CSharpSortType,
                    Enums.ESearchType.All => MixedSortType,
                    _ => throw new NotImplementedException()
                };
                return GetSortComparison(a, b, sortType);
            });
        }
        else {
            // -------------------
            // Standard search
            // -------------------
            foreach (var result in results) {
                result.Weight = result is ListItemFile file ? file.Name.Length - searchText.Length :
                    result is ListItemSymbol symbol ? symbol.Name.Length - searchText.Length :
                    0;
            }
            SortObjects(results, SearchType switch {
                Enums.ESearchType.Files => FileSortType,
                Enums.ESearchType.Methods => CSharpSortType,
                Enums.ESearchType.All => MixedSortType,
                _ => throw new NotImplementedException()
            });
        }
        return results;
    }

    private void SortObjects(List<ListItemBase> objectList, Enums.SortType sortType) {
        switch (sortType) {
            case Enums.SortType.Alphabetical:
                objectList.Sort(Sort.Alphabetical);
                break;
            case Enums.SortType.AlphabeticalReverse:
                objectList.Sort(Sort.AlphabeticalReverse);
                break;
            case Enums.SortType.LineNumber:
                objectList.Sort(Sort.LineNumber);
                break;
            case Enums.SortType.LineNumberReverse:
                objectList.Sort(Sort.LineNumberReverse);
                break;
            case Enums.SortType.Weight:
                objectList.Sort(Sort.Weight);
                break;
            case Enums.SortType.WeightReverse:
                objectList.Sort(Sort.WeightReverse);
                break;
        }
    }

    private int GetSortComparison(ListItemBase a, ListItemBase b, Enums.SortType sortType) {
        switch (sortType) {
            case Enums.SortType.Alphabetical:
                return Sort.Alphabetical(a, b);
            case Enums.SortType.AlphabeticalReverse:
                return Sort.AlphabeticalReverse(a, b);
            case Enums.SortType.LineNumber:
                return Sort.LineNumber(a, b);
            case Enums.SortType.LineNumberReverse:
                return Sort.LineNumberReverse(a, b);
            case Enums.SortType.Weight:
                return Sort.Weight(a, b);
            case Enums.SortType.WeightReverse:
                return Sort.WeightReverse(a, b);
            case Enums.SortType.Fuzzy:
                return Sort.Fuzzy(a, b);
            case Enums.SortType.FuzzyReverse:
                return Sort.FuzzyReverse(a, b);
            default:
                return Sort.Alphabetical(a, b);
        }
    }
}
