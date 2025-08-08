using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using QuickJump2022.Data;
using QuickJump2022.Forms;
using QuickJump2022.Tools;

namespace QuickJump2022;

[Command(PackageIds.ShowAllSearchForm)]
internal sealed class ShowAllSearchForm : BaseCommand<ShowAllSearchForm> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (QuickJumpData.Instance.Dte.ActiveWindow?.Document is null) return; // No active document.
        var path = QuickJumpData.Instance.Dte.ActiveWindow.Document.ProjectItem.TryGetProperty<string>("FullPath");
        if (string.IsNullOrEmpty(path)) {
            // Unsaved document (?)
            await VS.MessageBox.ShowWarningAsync(nameof(ShowAllSearchForm), "The active document doesn't have a path.");
            return;
        }
        var file = new FileInfo(path);
        var isCSharp = (file.Extension ?? "") == ".cs";
        var searchType = isCSharp ? Enums.ESearchType.All : Enums.ESearchType.Files;
        if (QuickJumpData.Instance.GeneralOptions.UseWPFInterface) {
            // Show WPF form 
            SearchFormWpf.ShowNonBlockingModal(searchType);
        }
        else {
            // Show WinForms form
            new SearchForm(searchType).Show();
        }
    }
}

