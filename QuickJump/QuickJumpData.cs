using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using QuickJump2022.Data;
using QuickJump2022.Models;
using QuickJump2022.Options;
using QuickJump2022.Tools;
using Document = EnvDTE.Document;
using DocumentEvents = EnvDTE.DocumentEvents;
using Project = EnvDTE.Project;

namespace QuickJump2022;

public partial class QuickJumpData {
    public GeneralOptionsPage GeneralOptions;

    public static QuickJumpData Instance;

    public DocumentEvents DocEvents;

    public EnvDTE.WindowEvents WinEvents;

    public DTEEvents DteEvents;

    public DTE Dte;

    public QuickJump2022Package Package {  get; private set; }

    private VisualStudioWorkspace _workspace;
    
    public KnownMonikerService MonikerService { get; private set; }

    public static async Task CreateAsync(QuickJump2022Package package, GeneralOptionsPage generalOptions) {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = package.GetService<DTE, DTE2>();
        var componentModel = package.GetService<SComponentModel, IComponentModel>();
        var workspace = componentModel.GetService<VisualStudioWorkspace>();
        Instance = new QuickJumpData {
            Dte = dte,
            Package = package,
            _workspace = workspace
        };
        Instance.DocEvents = Instance.Dte.Events.DocumentEvents;
        Instance.WinEvents = Instance.Dte.Events.WindowEvents;
        Instance.DteEvents = Instance.Dte.Events.DTEEvents;
        Instance.GeneralOptions = generalOptions;
        Instance.LoadSettings();
        
        // Initialize KnownMonikerService
        Instance.MonikerService = new KnownMonikerService();
        await Instance.MonikerService.PreloadCommonIconsAsync();
        
        Instance.DteEvents.OnBeginShutdown += new _dispDTEEvents_OnBeginShutdownEventHandler(DTEEvents_OnBeginShutdown);
    }

    private static void DTEEvents_OnBeginShutdown() => Instance.SaveSettings();

    private void LoadSettings() {
        var userSettingsStore = new ShellSettingsManager((IServiceProvider)(object)Package).GetReadOnlySettingsStore((SettingsScope)2);
        if (userSettingsStore.CollectionExists("General")) {
            if (userSettingsStore.PropertyExists("General", "ItemSeparatorColor")) {
                Instance.GeneralOptions.ItemSeparatorColor = Color.FromName(userSettingsStore.GetString("General", "ItemSeparatorColor"));
            }
            if (userSettingsStore.PropertyExists("General", "ShowStatusBar")) {
                Instance.GeneralOptions.ShowStatusBar = userSettingsStore.GetBoolean("General", "ShowStatusBar");
            }
            if (userSettingsStore.PropertyExists("General", "ShowIcons")) {
                Instance.GeneralOptions.ShowIcons = userSettingsStore.GetBoolean("General", "ShowIcons");
            }
            if (userSettingsStore.PropertyExists("General", "FileBackgroundColor")) {
                Instance.GeneralOptions.FileBackgroundColor = Color.FromName(userSettingsStore.GetString("General", "FileBackgroundColor"));
            }
            if (userSettingsStore.PropertyExists("General", "FileDescriptionForegroundColor")) {
                Instance.GeneralOptions.FileDescriptionForegroundColor = Color.FromName(userSettingsStore.GetString("General", "FileDescriptionForegroundColor"));
            }
            if (userSettingsStore.PropertyExists("General", "FileForegroundColor")) {
                Instance.GeneralOptions.FileForegroundColor = Color.FromName(userSettingsStore.GetString("General", "FileForegroundColor"));
            }
            if (userSettingsStore.PropertyExists("General", "FileSelectedBackgroundColor")) {
                Instance.GeneralOptions.FileSelectedBackgroundColor = Color.FromName(userSettingsStore.GetString("General", "FileSelectedBackgroundColor"));
            }
            if (userSettingsStore.PropertyExists("General", "FileSelectedDescriptionForegroundColor")) {
                Instance.GeneralOptions.FileSelectedDescriptionForegroundColor = Color.FromName(userSettingsStore.GetString("General", "FileSelectedDescriptionForegroundColor"));
            }
            if (userSettingsStore.PropertyExists("General", "FileSelectedForegroundColor")) {
                Instance.GeneralOptions.FileSelectedForegroundColor = Color.FromName(userSettingsStore.GetString("General", "FileSelectedForegroundColor"));
            }
            if (userSettingsStore.PropertyExists("General", "CodeBackgroundColor")) {
                Instance.GeneralOptions.CodeBackgroundColor = Color.FromName(userSettingsStore.GetString("General", "CodeBackgroundColor"));
            }
            if (userSettingsStore.PropertyExists("General", "CodeDescriptionForegroundColor")) {
                Instance.GeneralOptions.CodeDescriptionForegroundColor = Color.FromName(userSettingsStore.GetString("General", "CodeDescriptionForegroundColor"));
            }
            if (userSettingsStore.PropertyExists("General", "CodeForegroundColor")) {
                Instance.GeneralOptions.CodeForegroundColor = Color.FromName(userSettingsStore.GetString("General", "CodeForegroundColor"));
            }
            if (userSettingsStore.PropertyExists("General", "CodeSelectedBackgroundColor")) {
                Instance.GeneralOptions.CodeSelectedBackgroundColor = Color.FromName(userSettingsStore.GetString("General", "CodeSelectedBackgroundColor"));
            }
            if (userSettingsStore.PropertyExists("General", "CodeSelectedDescriptionForegroundColor")) {
                Instance.GeneralOptions.CodeSelectedDescriptionForegroundColor = Color.FromName(userSettingsStore.GetString("General", "CodeSelectedDescriptionForegroundColor"));
            }
            if (userSettingsStore.PropertyExists("General", "CodeSelectedForegroundColor")) {
                Instance.GeneralOptions.CodeSelectedForegroundColor = Color.FromName(userSettingsStore.GetString("General", "CodeSelectedForegroundColor"));
            }
            FontConverter fontConverter = new FontConverter();
            if (userSettingsStore.PropertyExists("General", "ItemFont")) {
                Instance.GeneralOptions.ItemFont = (Font)fontConverter.ConvertFromInvariantString(userSettingsStore.GetString("General", "ItemFont"));
            }
            if (userSettingsStore.PropertyExists("General", "SearchFont")) {
                Instance.GeneralOptions.SearchFont = (Font)fontConverter.ConvertFromInvariantString(userSettingsStore.GetString("General", "SearchFont"));
            }
            if (userSettingsStore.PropertyExists("General", "OffsetTop")) {
                Instance.GeneralOptions.OffsetTop = userSettingsStore.GetInt32("General", "OffsetTop");
            }
            if (userSettingsStore.PropertyExists("General", "OffsetLeft")) {
                Instance.GeneralOptions.OffsetLeft = userSettingsStore.GetInt32("General", "OffsetLeft");
            }
            if (userSettingsStore.PropertyExists("General", "Width")) {
                Instance.GeneralOptions.Width = userSettingsStore.GetInt32("General", "Width");
            }
            if (userSettingsStore.PropertyExists("General", "MaxHeight")) {
                Instance.GeneralOptions.MaxHeight = userSettingsStore.GetInt32("General", "MaxHeight");
            }
            if (userSettingsStore.PropertyExists("General", "FileSortType")) {
                Instance.GeneralOptions.FileSortType = (Enums.SortType)userSettingsStore.GetInt32("General", "FileSortType");
            }
            else {
                Instance.GeneralOptions.FileSortType = Enums.SortType.Alphabetical;
            }
            if (userSettingsStore.PropertyExists("General", "CSharpSortType")) {
                Instance.GeneralOptions.CSharpSortType = (Enums.SortType)userSettingsStore.GetInt32("General", "CSharpSortType");
            }
            else {
                Instance.GeneralOptions.CSharpSortType = Enums.SortType.LineNumber;
            }
            if (userSettingsStore.PropertyExists("General", "MixedSortType")) {
                Instance.GeneralOptions.MixedSortType = (Enums.SortType)userSettingsStore.GetInt32("General", "MixedSortType");
            }
            else {
                Instance.GeneralOptions.MixedSortType = Enums.SortType.Alphabetical;
            }
            if (userSettingsStore.PropertyExists("General", "BorderColor")) {
                Instance.GeneralOptions.BorderColor = Color.FromName(userSettingsStore.GetString("General", "BorderColor"));
            }
        }
    }

    private void SaveSettings() {
        WritableSettingsStore writableSettingsStore = ((SettingsManager)new ShellSettingsManager((IServiceProvider)(object)Package)).GetWritableSettingsStore((SettingsScope)2);
        writableSettingsStore.SetString("General", "ItemSeparatorColor", Instance.GeneralOptions.ItemSeparatorColor.Name);
        writableSettingsStore.SetBoolean("General", "ShowStatusBar", Instance.GeneralOptions.ShowStatusBar);
        writableSettingsStore.SetBoolean("General", "ShowIcons", Instance.GeneralOptions.ShowIcons);
        writableSettingsStore.SetString("General", "FileBackgroundColor", Instance.GeneralOptions.FileBackgroundColor.Name);
        writableSettingsStore.SetString("General", "FileDescriptionForegroundColor", Instance.GeneralOptions.FileDescriptionForegroundColor.Name);
        writableSettingsStore.SetString("General", "FileForegroundColor", Instance.GeneralOptions.FileForegroundColor.Name);
        writableSettingsStore.SetString("General", "FileSelectedBackgroundColor", Instance.GeneralOptions.FileSelectedBackgroundColor.Name);
        writableSettingsStore.SetString("General", "FileSelectedDescriptionForegroundColor", Instance.GeneralOptions.FileSelectedDescriptionForegroundColor.Name);
        writableSettingsStore.SetString("General", "FileSelectedForegroundColor", Instance.GeneralOptions.FileSelectedForegroundColor.Name);
        writableSettingsStore.SetString("General", "CodeBackgroundColor", Instance.GeneralOptions.CodeBackgroundColor.Name);
        writableSettingsStore.SetString("General", "CodeDescriptionForegroundColor", Instance.GeneralOptions.CodeDescriptionForegroundColor.Name);
        writableSettingsStore.SetString("General", "CodeForegroundColor", Instance.GeneralOptions.CodeForegroundColor.Name);
        writableSettingsStore.SetString("General", "CodeSelectedBackgroundColor", Instance.GeneralOptions.CodeSelectedBackgroundColor.Name);
        writableSettingsStore.SetString("General", "CodeSelectedDescriptionForegroundColor", Instance.GeneralOptions.CodeSelectedDescriptionForegroundColor.Name);
        writableSettingsStore.SetString("General", "CodeSelectedForegroundColor", Instance.GeneralOptions.CodeSelectedForegroundColor.Name);
        writableSettingsStore.SetString("General", "BorderColor", Instance.GeneralOptions.BorderColor.Name);
        var fontConverter = new FontConverter();
        writableSettingsStore.SetString("General", "ItemFont", fontConverter.ConvertToInvariantString(Instance.GeneralOptions.ItemFont));
        writableSettingsStore.SetString("General", "SearchFont", fontConverter.ConvertToInvariantString(Instance.GeneralOptions.SearchFont));
        writableSettingsStore.SetInt32("General", "OffsetTop", Instance.GeneralOptions.OffsetTop);
        writableSettingsStore.SetInt32("General", "OffsetLeft", Instance.GeneralOptions.OffsetLeft);
        writableSettingsStore.SetInt32("General", "Width", Instance.GeneralOptions.Width);
        writableSettingsStore.SetInt32("General", "MaxHeight", Instance.GeneralOptions.MaxHeight);
        writableSettingsStore.SetInt32("General", "FileSortType", (int)Instance.GeneralOptions.FileSortType);
        writableSettingsStore.SetInt32("General", "CSharpSortType", (int)Instance.GeneralOptions.CSharpSortType);
        writableSettingsStore.SetInt32("General", "MixedSortType", (int)Instance.GeneralOptions.MixedSortType);
    }

    public List<ProjectItem> GetProjectItems() {
        ThreadHelper.ThrowIfNotOnUIThread("GetDocFilenames");
        var list = new List<ProjectItem>();
        foreach (Project project in Dte.Solution.Projects) {
            InternalGetProjectItems(project.ProjectItems, list);
        }
        return list;
    }

    private void InternalGetProjectItems(ProjectItems projItems, List<ProjectItem> list) {
        ThreadHelper.ThrowIfNotOnUIThread("InternalGetDocFilenames");
        if (projItems is null) {
            return;
        }
        foreach (ProjectItem projItem in projItems) {
            if (projItem.ProjectItems != null && projItem.ProjectItems.Count > 0) {
                InternalGetProjectItems(projItem.ProjectItems, list);
            }
            var path = projItem.TryGetProperty<string>("FullPath");
            if (projItem.Name.Contains(".") && !string.IsNullOrEmpty(path) && File.Exists(path)) {
                list.Add(projItem);
            }
        }
    }
}
