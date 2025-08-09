using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using EnvDTE;
using Microsoft.CodeAnalysis.Elfie.Model;
using QuickJump2022.Data;
using QuickJump2022.Models;
using QuickJump2022.Tools;

namespace QuickJump2022.Tools;

public class SearchController {
    public QuickJump2022Package Package { get; set; }
    public Enums.ESearchType SearchType { get; set; }
    public List<ListItemFile> Files { get; set; } = new();
    public List<ListItemSymbol> Symbols { get; set; } = new();
    public string SolutionName { get; set; } = "";

    public SearchController(
        QuickJump2022Package package,
        Enums.ESearchType type
    ) {
        Package = package;
        SearchType = type;
    }

    public async Task LoadDataThreadSafeAsync() {
        await ThreadHelper.JoinableTaskFactory.RunAsync(async delegate {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            await LoadDataAsync();
        });
    }

    private async Task LoadDataAsync() {

        string GetFileDescription(string fullPath) {
            if (string.IsNullOrEmpty(fullPath)) return string.Empty;
            var lastSlash = fullPath.LastIndexOf('\\');
            return lastSlash >= 0 ? fullPath.Remove(lastSlash).TrimEnd('\\') : string.Empty;
        }

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
        
        // -----------
        // Files
        // -----------
        if (SearchType == Enums.ESearchType.Files || SearchType == Enums.ESearchType.All) {
            var items = QuickJumpData.Instance.GetProjectItems();
            Files = new List<ListItemFile>(items.Count);
            foreach (var projFile in items) {
                var filePath = projFile.TryGetProperty<string>("FullPath");
                Files.Add(
                    new ListItemFile {
                        Name = projFile.Name,
                        FullPath = filePath,
                        ProjectItem = projFile,
                        Weight = 0,
                        Description = GetFileDescription(filePath),
                        Line = 1,
                        IconImage = Utilities.GetMimeTypeIcon(filePath)
                    }
                );
            }
        }
        // -----------
        // Symbols
        // -----------
        if (SearchType == Enums.ESearchType.Methods || SearchType == Enums.ESearchType.All) {
            var document = QuickJumpData.Instance.Dte.ActiveWindow.Document;
            var codeItems = await QuickJumpData.Instance.GetCodeItemsUsingWorkspaceAsync(document);
            Symbols = new List<ListItemSymbol>(codeItems.Count);
            foreach (var item in codeItems) {
                var accessType = FormatAccessType(item.AccessType);
                Symbols.Add(
                    new ListItemSymbol {
                        Name = item.NameOnly,
                        Type = item.Type,
                        Line = item.Line,
                        Weight = 0,
                        Description = $"{accessType} {item.BindType}",
                        Document = item.ProjDocument,
                        BindType = item.BindType,
                        IconImage = Utilities.GetCodeIcon(item.BindType)
                    }
                );
            }
        }
        // -----------
        // Metadata
        // -----------
        SolutionName = QuickJumpData.Instance.Dte.Solution.FullName;
    }

    public List<ListItemBase> Search(string searchText) {
        var results = new List<ListItemBase>(20);
        
        bool Filter(string str, string srcStr) {
            if (srcStr.Length > 0) {
                // Use fuzzy search instead of simple substring matching
                return FuzzySearch.IsMatch(str, srcStr);
            }
            return true;
        }

        if ((SearchType is Enums.ESearchType.Files or Enums.ESearchType.All) && Files.Count > 0) {
            foreach (var file in Files) {
                if (Filter(file.Name, searchText)) {
                    results.Add(file);
                }
            }
        }

        if ((SearchType == Enums.ESearchType.Methods || SearchType == Enums.ESearchType.All) && Symbols.Count > 0) {
            foreach (var symbol in Symbols) {
                if (Filter(symbol.Name, searchText)) {
                    results.Add(symbol);
                }
            }
        }
        // -------------------
        // Fuzzy search
        // -------------------
        var options = QuickJumpData.Instance.GeneralOptions;
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
                    Enums.ESearchType.Files => options.FileSortType,
                    Enums.ESearchType.Methods => options.CSharpSortType,
                    _ => options.MixedSortType,
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
                Enums.ESearchType.Files => options.FileSortType,
                Enums.ESearchType.Methods => options.CSharpSortType,
                _ => options.MixedSortType,
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
