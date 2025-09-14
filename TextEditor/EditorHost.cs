using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace QuickJump2022.TextEditor;

public partial class EditorHost {
    private static Microsoft.VisualStudio.OLE.Interop.IServiceProvider cachedOleServiceProvider;
    private static Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider {
        get {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (cachedOleServiceProvider == null) {
                IObjectWithSite objWithSite = ServiceProvider.GlobalProvider;
                Guid interfaceIID = typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider).GUID;
                objWithSite.GetSite(ref interfaceIID, out IntPtr rawSP);
                try {
                    if (rawSP != IntPtr.Zero) {
                        cachedOleServiceProvider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)Marshal.GetObjectForIUnknown(rawSP);
                    }
                }
                finally {
                    if (rawSP != IntPtr.Zero) {
                        Marshal.Release(rawSP);
                    }
                }
            }

            return cachedOleServiceProvider;
        }
    }
    private IWpfTextView _wpfTextView;
    private IWpfTextViewHost _wpfTextViewHost;
    private uint _unregisterPriorityCommandTargetCookie;
    private IVsTextView _vsTextView;
    private ThreadMessageEventHandler _threadFilter;

    public IWpfTextView WpfTextView => _wpfTextView;
    public EditorCommandFilter CommandFilter { get; private set; }
    private IContentTypeRegistryService ContentTypeRegistryService { get; }
    private IEditorOptionsFactoryService EditorOptionsFactoryService { get; }
    private IVsEditorAdaptersFactoryService VsEditorAdaptersFactoryService { get; }
    private ITextBufferFactoryService TextBufferFactoryService { get; }
    private ITextEditorFactoryService TextEditorFactoryService { get; }
    private IVsRegisterPriorityCommandTarget VsRegisterPriorityCommandTarget { get; }
    private IVsFilterKeys2 VsFilterKeys { get; }

    public EditorHost() {
        ContentTypeRegistryService = VS.GetMefService<IContentTypeRegistryService>();
        EditorOptionsFactoryService = VS.GetMefService<IEditorOptionsFactoryService>();
        VsEditorAdaptersFactoryService = VS.GetMefService<IVsEditorAdaptersFactoryService>();
        TextBufferFactoryService = VS.GetMefService<ITextBufferFactoryService>();
        TextEditorFactoryService = VS.GetMefService<ITextEditorFactoryService>();
        VsRegisterPriorityCommandTarget = VS.GetRequiredService<SVsRegisterPriorityCommandTarget, IVsRegisterPriorityCommandTarget>();
        VsFilterKeys = VS.GetRequiredService<SVsFilterKeys, IVsFilterKeys2>();
    }

    public IWpfTextViewHost InitializeHost() {
        ThreadHelper.ThrowIfNotOnUIThread();
        var initialText = string.Empty;
        var contentTypeAsString = "text"; // Content type (language)
        var contentType = ContentTypeRegistryService.GetContentType(contentTypeAsString);

        var vsTextBuffer = VsEditorAdaptersFactoryService.CreateVsTextBufferAdapter(OleServiceProvider, contentType);
        vsTextBuffer.InitializeContent(initialText, initialText.Length);

        var textBuffer = VsEditorAdaptersFactoryService.GetDataBuffer(vsTextBuffer);
        var roleSet = TextEditorFactoryService.CreateTextViewRoleSet([
            PredefinedTextViewRoles.Editable,
            PredefinedTextViewRoles.Interactive,
        ]);

        CommandFilter = new EditorCommandFilter(AllowedCommands);

        var vsTextView = VsEditorAdaptersFactoryService.CreateVsTextViewAdapter(OleServiceProvider, roleSet);
        ((IVsTextEditorPropertyCategoryContainer)vsTextView).GetPropertyCategory(DefGuidList.guidEditPropCategoryViewMasterSettings, out var propContainer);
        propContainer.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewComposite_AllCodeWindowDefaults, true);
        propContainer.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewGlobalOpt_AutoScrollCaretOnTextEntry, true);
        _vsTextView = vsTextView;
        vsTextView.AddCommandFilter(CommandFilter, out var nextCmdTarget);
        CommandFilter.NextCommandTarget = nextCmdTarget;
        vsTextView.Initialize(
            (IVsTextLines)vsTextBuffer,
            IntPtr.Zero,
            (uint)TextViewInitFlags.VIF_DEFAULT | (uint)TextViewInitFlags3.VIF_NO_HWND_SUPPORT,
            [new INITVIEW { fSelectionMargin = 0, fWidgetMargin = 0, fVirtualSpace = 0, fDragDropMove = 0, }]
        );
        _wpfTextViewHost = VsEditorAdaptersFactoryService.GetWpfTextViewHost(vsTextView);
        _wpfTextView = VsEditorAdaptersFactoryService.GetWpfTextView(vsTextView);

        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.EnableFileHealthIndicatorOptionId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.EditingStateMarginOptionId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.SourceImageMarginEnabledOptionId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.ShowChangeTrackingMarginOptionId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.VerticalScrollBarId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.HorizontalScrollBarId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.LineNumberMarginId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.OutliningMarginId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.GlyphMarginId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.SuggestionMarginId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.SelectionMarginId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.ZoomControlId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewHostOptions.ShowMarksOptionId, false);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewOptions.WordWrapStyleId, WordWrapStyles.None);
        _wpfTextView.Options.SetOptionValue(DefaultTextViewOptions.UseVisibleWhitespaceId, true);
        _wpfTextView.Options.SetOptionValue(DefaultOptions.FallbackFontId, "Consolas"); // Unused

        _threadFilter = new ThreadMessageEventHandler(this.FilterThreadMessage);
        ComponentDispatcher.ThreadFilterMessage += _threadFilter;
        this._wpfTextViewHost.HostControl.TabIndex = 0;

        VsRegisterPriorityCommandTarget.RegisterPriorityCommandTarget(0U, CommandFilter, out _unregisterPriorityCommandTargetCookie);

        return this._wpfTextViewHost;
    }

    private void FilterThreadMessage(ref System.Windows.Interop.MSG msg, ref bool handled) {
        ThreadHelper.ThrowIfNotOnUIThread();
        /*
            https://wiki.winehq.org/List_Of_Windows_Messages
            0100		256		WM_KEYDOWN
            0100		256		WM_KEYFIRST
            0101		257		WM_KEYUP
            0102		258		WM_CHAR
            0103		259		WM_DEADCHAR
            0104		260		WM_SYSKEYDOWN
            0105		261		WM_SYSKEYUP
            0106		262		WM_SYSCHAR
            0107		263		WM_SYSDEADCHAR
        */
        if (this.VsFilterKeys == null || msg.message < 256 || msg.message > 264) {
            return;
        }
        Microsoft.VisualStudio.OLE.Interop.MSG msg1 = new Microsoft.VisualStudio.OLE.Interop.MSG() {
            hwnd = msg.hwnd,
            lParam = msg.lParam,
            wParam = msg.wParam,
            message = (uint)msg.message
        };
        if (!ErrorHandler.Succeeded(this.VsFilterKeys.TranslateAcceleratorEx(
            [msg1], 
            (uint)(__VSTRANSACCELEXFLAGS.VSTAEXF_NoFireCommand | __VSTRANSACCELEXFLAGS.VSTAEXF_UseTextEditorKBScope | __VSTRANSACCELEXFLAGS.VSTAEXF_AllowModalState),
            0U, [], out var pguidCmd, out var pdwCmd, out _, out _))
        ) {
            return;
        }
        var cmdAndId = $"{pguidCmd} {pdwCmd}";
        if (!CommandFilter.IsCommandAllowed(ref pguidCmd, pdwCmd)) {
            Debug.WriteLine($"Command {cmdAndId} is not allowed");
            return;
        }
        Debug.WriteLine($"Executing {cmdAndId}");
        int hr = VsFilterKeys.TranslateAcceleratorEx([msg1], 20U, 0U, [], out _, out _, out _, out _);
        handled = ErrorHandler.Succeeded(hr);
    }

    public void Close() {
        VsRegisterPriorityCommandTarget.UnregisterPriorityCommandTarget(_unregisterPriorityCommandTargetCookie);
        ComponentDispatcher.ThreadFilterMessage -= _threadFilter;
        _wpfTextView.Close();
        var uiShell = ServiceProvider.GlobalProvider.GetService<SVsUIShell, IVsUIShell>();
        uiShell?.UpdateCommandUI(0); // 0 means immediate update
    }
}