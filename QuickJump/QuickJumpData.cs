using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using QuickJump2022.Data;
using QuickJump2022.Models;
using QuickJump2022.Options;
using QuickJump2022.Tools;
using DocumentEvents = EnvDTE.DocumentEvents;
using Project = EnvDTE.Project;

namespace QuickJump2022;

public class QuickJumpData {
    public GeneralOptionsPage GeneralOptions;

    public static QuickJumpData Instance;

    public DocumentEvents DocEvents;

    public EnvDTE.WindowEvents WinEvents;

    public DTEEvents DteEvents;

    public DTE Dte;

    private QuickJump2022Package m_Package;

    public static void Create(QuickJump2022Package package, GeneralOptionsPage generalOptions) {
        ThreadHelper.ThrowIfNotOnUIThread("Create");
        Instance = new QuickJumpData {
            Dte = package.GetService<DTE, DTE2>(),
            m_Package = package
        };
        Instance.DocEvents = Instance.Dte.Events.DocumentEvents;
        Instance.WinEvents = Instance.Dte.Events.WindowEvents;
        Instance.DteEvents = Instance.Dte.Events.DTEEvents;
        Instance.GeneralOptions = generalOptions;
        Instance.LoadSettings();
        Utilities.PreloadCodeIcons();
        Instance.DteEvents.OnBeginShutdown += new _dispDTEEvents_OnBeginShutdownEventHandler(DTEEvents_OnBeginShutdown);
    }

    private static void DTEEvents_OnBeginShutdown() => Instance.SaveSettings();

    private void LoadSettings() {
        var userSettingsStore = new ShellSettingsManager((IServiceProvider)(object)m_Package).GetReadOnlySettingsStore((SettingsScope)2);
        if (userSettingsStore.CollectionExists("General")) {
            if (userSettingsStore.PropertyExists("General", "ItemSeperatorColor")) {
                Instance.GeneralOptions.ItemSeperatorColor = Color.FromName(userSettingsStore.GetString("General", "ItemSeperatorColor"));
            }
            if (userSettingsStore.PropertyExists("General", "UseModernIcons")) {
                Instance.GeneralOptions.UseModernIcons = userSettingsStore.GetBoolean("General", "UseModernIcons");
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
        }
    }

    private void SaveSettings() {
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        WritableSettingsStore writableSettingsStore = ((SettingsManager)new ShellSettingsManager((IServiceProvider)(object)m_Package)).GetWritableSettingsStore((SettingsScope)2);
        writableSettingsStore.SetString("General", "ItemSeperatorColor", Instance.GeneralOptions.ItemSeperatorColor.Name);
        writableSettingsStore.SetBoolean("General", "UseModernIcons", Instance.GeneralOptions.UseModernIcons);
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
        FontConverter fontConverter = new FontConverter();
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

    public List<ProjectItem> GetDocFilenames() {
        ThreadHelper.ThrowIfNotOnUIThread("GetDocFilenames");
        List<ProjectItem> list = new List<ProjectItem>();
        foreach (Project project2 in Dte.Solution.Projects) {
            Project project = project2;
            InternalGetDocFilenames(project.ProjectItems, list);
        }
        return list;
    }

    private void InternalGetDocFilenames(ProjectItems projItems, List<ProjectItem> list) {
        ThreadHelper.ThrowIfNotOnUIThread("InternalGetDocFilenames");
        if (projItems == null) {
            return;
        }
        foreach (ProjectItem projItem2 in projItems) {
            ProjectItem projItem = projItem2;
            if (projItem.ProjectItems != null && projItem.ProjectItems.Count > 0) {
                InternalGetDocFilenames(projItem.ProjectItems, list);
            }
            string path = projItem.TryGetProperty<string>("FullPath");
            if (projItem.Name.Contains(".") && !string.IsNullOrEmpty(path) && File.Exists(path)) {
                list.Add(projItem);
            }
        }
    }

    public List<CodeItem> GetCodeItems(Document document) {
        ThreadHelper.ThrowIfNotOnUIThread("GetCodeItems");
        var list = new List<CodeItem>();
        var path = document.ProjectItem.TryGetProperty<string>("FullPath");
        if (string.IsNullOrEmpty(path)) {
            return list;
        }
        var fileContent = File.ReadAllText(path);
        var syntaxTree = CSharpSyntaxTree.ParseText(fileContent, CSharpParseOptions.Default, "", null, default);
        var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
        NodeHandler(root.Members, list, document);
        GC.Collect();
        return list;
    }

    private void NodeHandler(SyntaxList<MemberDeclarationSyntax> nodes, List<CodeItem> list, Document document) {
        SyntaxList<MemberDeclarationSyntax>.Enumerator enumerator = nodes.GetEnumerator();
        while (enumerator.MoveNext()) {
            MemberDeclarationSyntax current = enumerator.Current;
            LinePosition startLinePosition = current.SyntaxTree.GetLineSpan(current.Span).StartLinePosition;
            SyntaxKind syntaxKind = current.Kind();
            int line = startLinePosition.Line + 1;
            Enums.EBindType bindType = Enums.EBindType.None;
            string name = string.Empty;
            int flagType = -1;
            switch (syntaxKind) {
                case SyntaxKind.NamespaceDeclaration:
                    NodeHandler(((NamespaceDeclarationSyntax)current).Members, list, document);
                    continue;
                case SyntaxKind.ClassDeclaration: {
                        ClassDeclarationSyntax declarationSyntax10 = (ClassDeclarationSyntax)current;
                        flagType = (int)GetFlagType(declarationSyntax10.Modifiers);
                        bindType = Enums.EBindType.Class;
                        name = declarationSyntax10.Identifier.Text;
                        NodeHandler(declarationSyntax10.Members, list, document);
                        break;
                    }
                case SyntaxKind.PropertyDeclaration: {
                        PropertyDeclarationSyntax declarationSyntax9 = (PropertyDeclarationSyntax)current;
                        flagType = (int)GetFlagType(declarationSyntax9.Modifiers);
                        bindType = Enums.EBindType.Property;
                        name = declarationSyntax9.Identifier.Text;
                        break;
                    }
                case SyntaxKind.MethodDeclaration: {
                        MethodDeclarationSyntax declarationSyntax8 = (MethodDeclarationSyntax)current;
                        flagType = (int)GetFlagType(declarationSyntax8.Modifiers);
                        bindType = Enums.EBindType.Method;
                        name = declarationSyntax8.Identifier.Text;
                        break;
                    }
                case SyntaxKind.EnumDeclaration: {
                        EnumDeclarationSyntax declarationSyntax7 = (EnumDeclarationSyntax)current;
                        flagType = (int)GetFlagType(declarationSyntax7.Modifiers);
                        bindType = Enums.EBindType.Enum;
                        name = declarationSyntax7.Identifier.Text;
                        break;
                    }
                case SyntaxKind.DelegateDeclaration: {
                        DelegateDeclarationSyntax declarationSyntax6 = (DelegateDeclarationSyntax)current;
                        flagType = (int)GetFlagType(declarationSyntax6.Modifiers);
                        bindType = Enums.EBindType.Delegate;
                        name = declarationSyntax6.Identifier.Text;
                        break;
                    }
                case SyntaxKind.EventDeclaration: {
                        EventDeclarationSyntax declarationSyntax5 = (EventDeclarationSyntax)current;
                        flagType = (int)GetFlagType(declarationSyntax5.Modifiers);
                        bindType = Enums.EBindType.Event;
                        name = declarationSyntax5.Identifier.Text;
                        break;
                    }
                case SyntaxKind.EventFieldDeclaration: {
                        EventFieldDeclarationSyntax declarationSyntax4 = (EventFieldDeclarationSyntax)current;
                        flagType = (int)GetFlagType(declarationSyntax4.Modifiers);
                        VariableDeclaratorSyntax variableDeclaratorSyntax2 = declarationSyntax4.Declaration.Variables.First();
                        bindType = Enums.EBindType.Event;
                        name = variableDeclaratorSyntax2.Identifier.Text;
                        break;
                    }
                case SyntaxKind.InterfaceDeclaration: {
                        InterfaceDeclarationSyntax declarationSyntax3 = (InterfaceDeclarationSyntax)current;
                        flagType = (int)GetFlagType(declarationSyntax3.Modifiers);
                        bindType = Enums.EBindType.Interface;
                        name = declarationSyntax3.Identifier.Text;
                        NodeHandler(declarationSyntax3.Members, list, document);
                        break;
                    }
                case SyntaxKind.StructDeclaration: {
                        StructDeclarationSyntax declarationSyntax2 = (StructDeclarationSyntax)current;
                        flagType = (int)GetFlagType(declarationSyntax2.Modifiers);
                        bindType = Enums.EBindType.Struct;
                        name = declarationSyntax2.Identifier.Text;
                        NodeHandler(declarationSyntax2.Members, list, document);
                        break;
                    }
                case SyntaxKind.FieldDeclaration: {
                        FieldDeclarationSyntax declarationSyntax = (FieldDeclarationSyntax)current;
                        flagType = (int)GetFlagType(declarationSyntax.Modifiers);
                        VariableDeclaratorSyntax variableDeclaratorSyntax = declarationSyntax.Declaration.Variables.First();
                        bindType = Enums.EBindType.Field;
                        name = variableDeclaratorSyntax.Identifier.Text;
                        break;
                    }
                default:
                    continue;
            }
            list.Add(new CodeItem {
                ProjDocument = document,
                Line = line,
                BindType = bindType,
                AccessType = (Enums.EAccessType)flagType,
                Name = name
            });
        }
    }

    private uint GetFlagType(SyntaxTokenList modifiers) {
        uint num = 0u;
        modifiers.Any(SyntaxKind.SealedKeyword);
        if (modifiers.Any(SyntaxKind.StaticKeyword)) {
            num |= 1;
        }
        if (modifiers.Any(SyntaxKind.ConstKeyword)) {
            num |= 2;
        }
        if (modifiers.Any(SyntaxKind.PublicKeyword)) {
            num |= 4;
        }
        if (modifiers.Any(SyntaxKind.PrivateKeyword)) {
            num |= 8;
        }
        if (modifiers.Any(SyntaxKind.ProtectedKeyword)) {
            num |= 0x10;
        }
        if (modifiers.Any(SyntaxKind.InternalKeyword)) {
            num |= 0x20;
        }
        return num;
    }
}
