using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
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
using static QuickJump2022.TextEditor.EditorCommandFilter;

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
    public event InputEditorSpecialKeyHandler KeyPressed;

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

        var codeWindowBehaviourFlags = CodeWindowBehaviourFlags.CWB_DISABLEDROPDOWNBAR | CodeWindowBehaviourFlags.CWB_DISABLESPLITTER | CodeWindowBehaviourFlags.CWB_DISABLEDIFF;
        _codeWindow = VsEditorAdaptersFactoryService.CreateVsCodeWindowAdapter(OleServiceProvider);
        ((IVsCodeWindowEx)_codeWindow).Initialize((uint)codeWindowBehaviourFlags, VSUSERCONTEXTATTRIBUTEUSAGE.VSUC_Usage_Filter, string.Empty, string.Empty, 196608U, new INITVIEW[1]);
        var codeWindow = _codeWindow as IVsUserData;
        var textViewRolesGuid = VSConstants.VsTextBufferUserDataGuid.VsTextViewRoles_guid;
        codeWindow.SetData(ref textViewRolesGuid, (object)"Editable,Interactive");
        _codeWindow.SetBuffer(vsTextBuffer as IVsTextLines);
        _codeWindow.GetPrimaryView(out var textView);
        _wpfTextViewHost = VsEditorAdaptersFactoryService.GetWpfTextViewHost(textView);
        _wpfTextView = _wpfTextViewHost.TextView;

        //var vsTextBuffer = VsEditorAdaptersFactoryService.CreateVsTextBufferAdapter(OleServiceProvider, contentType);
        //vsTextBuffer.InitializeContent(initialText, initialText.Length);
        //var textBuffer = VsEditorAdaptersFactoryService.GetDataBuffer(vsTextBuffer);
        //var roleSet = TextEditorFactoryService.CreateTextViewRoleSet([
        //    PredefinedTextViewRoles.Editable,
        //    PredefinedTextViewRoles.Interactive,
        //]);

        CommandFilter = new EditorCommandFilter(AllowedCommands);
        textView.AddCommandFilter(CommandFilter, out var nextCmdTarget);
        CommandFilter.NextCommandTarget = nextCmdTarget;

        //var vsTextView = VsEditorAdaptersFactoryService.CreateVsTextViewAdapter(OleServiceProvider, roleSet);
        //((IVsTextEditorPropertyCategoryContainer)vsTextView).GetPropertyCategory(DefGuidList.guidEditPropCategoryViewMasterSettings, out var propContainer);
        //propContainer.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewComposite_AllCodeWindowDefaults, true);
        //propContainer.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewGlobalOpt_AutoScrollCaretOnTextEntry, true);
        //_vsTextView = vsTextView;
        //vsTextView.AddCommandFilter(CommandFilter, out var nextCmdTarget);
        //CommandFilter.NextCommandTarget = nextCmdTarget;
        //vsTextView.Initialize(
        //    (IVsTextLines)vsTextBuffer,
        //    IntPtr.Zero,
        //    (uint)TextViewInitFlags.VIF_DEFAULT | (uint)TextViewInitFlags3.VIF_NO_HWND_SUPPORT,
        //    [new INITVIEW { fSelectionMargin = 0, fWidgetMargin = 0, fVirtualSpace = 0, fDragDropMove = 0, }]
        //);
        //_wpfTextViewHost = VsEditorAdaptersFactoryService.GetWpfTextViewHost(vsTextView);
        //_wpfTextView = VsEditorAdaptersFactoryService.GetWpfTextView(vsTextView);

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
        // ------------------
        // Hack to intercept Ctrl+` 
        // ------------------
        // Ctrl + (VK_OEM_3) intercept before VS handles it 
        const int WM_KEYDOWN = 0x0100;
        const int WM_SYSKEYDOWN = 0x0104;
        const int VK_OEM_3 = 0xC0;
        // backtick/tilde on US layout
        if ((msg.message == WM_KEYDOWN || msg.message == WM_SYSKEYDOWN)
            && msg.wParam.ToInt32() == VK_OEM_3
            && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control
            && _wpfTextViewHost?.HostControl?.IsKeyboardFocusWithin == true
            ) {
            handled = true;
            var keyInfo = new VsKeyInfo() { 
                ControlPressed = true,
                Key = System.Windows.Input.Key.OemTilde,
                KeyChar = '`',
                VirtualKey = VK_OEM_3,
                KeyStates = System.Windows.Input.KeyStates.Down
            };
            KeyPressed?.Invoke(this, keyInfo);
            return;
        }
        // ------------------
        Microsoft.VisualStudio.OLE.Interop.MSG msg1 = new Microsoft.VisualStudio.OLE.Interop.MSG() {
            hwnd = msg.hwnd,
            lParam = msg.lParam,
            wParam = msg.wParam,
            message = (uint)msg.message
        };
        if (!ErrorHandler.Succeeded(this.VsFilterKeys.TranslateAcceleratorEx(
            [msg1], 
            (uint)(
                __VSTRANSACCELEXFLAGS.VSTAEXF_NoFireCommand 
                | __VSTRANSACCELEXFLAGS.VSTAEXF_UseTextEditorKBScope 
                | __VSTRANSACCELEXFLAGS.VSTAEXF_AllowModalState
            ),
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
        int hr = VsFilterKeys.TranslateAcceleratorEx([msg1], (uint)(__VSTRANSACCELEXFLAGS.VSTAEXF_UseTextEditorKBScope | __VSTRANSACCELEXFLAGS.VSTAEXF_AllowModalState), 0U, [], out _, out _, out _, out _);
        handled = ErrorHandler.Succeeded(hr);
    }

    public void StopProcessing() => Close();

    public void Close() {
        if (_closed) return; _closed = true;
        VsRegisterPriorityCommandTarget.UnregisterPriorityCommandTarget(_unregisterPriorityCommandTargetCookie);
        ComponentDispatcher.ThreadFilterMessage -= _threadFilter;
        _wpfTextView.Close();
        var uiShell = ServiceProvider.GlobalProvider.GetService<SVsUIShell, IVsUIShell>();
        uiShell?.UpdateCommandUI(0); // 0 means immediate update
    }

    bool _closed = false;
    private IVsCodeWindow _codeWindow;
}