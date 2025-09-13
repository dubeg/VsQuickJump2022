using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace QuickJump2022.TextEditor;

public class Hoster : IDisposable {
    private readonly IVsRegisterPriorityCommandTarget vsRegisterPriorityCommandTarget;
    private readonly IVsEditorAdaptersFactoryService vsEditorAdaptersFactory;
    private readonly Microsoft.VisualStudio.OLE.Interop.IServiceProvider oleServiceProvider;
    private readonly IVsTextLines pBuffer;
    private readonly string textViewRoles;
    private readonly Action<IWpfTextView> initializeTextView;
    private readonly Dictionary<Guid, uint[]> allowedCommands;
    private readonly bool isReadOnly;
    private readonly CodeWindowBehaviourFlags codeWindowBehaviourFlags;
    private IVsCodeWindow codeWindow;
    private IVsTextView textView;
    private IWpfTextViewHost wpfTextViewHost;
    private bool hasFocus;
    private CodeEditorCommandFilter commandFilter;
    private readonly IVsFilterKeys2 filterKeys;
    private uint unregisterPriorityCommandTargetCookie;
    private bool disposedValue;
    
    // Expose the command filter so clients can subscribe to events
    internal CodeEditorCommandFilter CommandFilter => commandFilter;

    public static readonly IEnumerable<string> ReadOnlyDefaultTextViewRoles = new string[]
    {
            PredefinedTextViewRoles.Analyzable,
            PredefinedTextViewRoles.Document,
            PredefinedTextViewRoles.Interactive,
            PredefinedTextViewRoles.Structured,
            PredefinedTextViewRoles.PrimaryDocument
    };


    public static readonly IEnumerable<string> DefaultTextViewRoles = new string[] {
            PredefinedTextViewRoles.Analyzable,
            PredefinedTextViewRoles.Document,
            PredefinedTextViewRoles.Editable,
            PredefinedTextViewRoles.Interactive,
            PredefinedTextViewRoles.Structured,
            PredefinedTextViewRoles.PrimaryDocument
        };

    public static readonly IEnumerable<string> AllEditorPredefinedTextViewRoles = new string[] {
            PredefinedTextViewRoles.Analyzable,
            PredefinedTextViewRoles.Debuggable,
            PredefinedTextViewRoles.Document,
            PredefinedTextViewRoles.Editable,
            PredefinedTextViewRoles.Interactive,
            PredefinedTextViewRoles.Structured,
            PredefinedTextViewRoles.Zoomable,
            PredefinedTextViewRoles.PrimaryDocument
        };

    public static readonly IEnumerable<string> ReadOnlyAllEditorPredefinedTextViewRoles = new string[] {
            PredefinedTextViewRoles.Analyzable,
            PredefinedTextViewRoles.Debuggable,
            PredefinedTextViewRoles.Document,
            PredefinedTextViewRoles.Interactive,
            PredefinedTextViewRoles.Structured,
            PredefinedTextViewRoles.Zoomable,
            PredefinedTextViewRoles.PrimaryDocument
        };

    public static readonly IEnumerable<string> ReadOnlyAllPredefinedTextViewRoles = new string[] {
            PredefinedTextViewRoles.Analyzable,
            PredefinedTextViewRoles.ChangePreview,
            PredefinedTextViewRoles.CodeDefinitionView,
            PredefinedTextViewRoles.Debuggable,
            PredefinedTextViewRoles.Document,
            PredefinedTextViewRoles.EmbeddedPeekTextView,
            PredefinedTextViewRoles.Interactive,
            PredefinedTextViewRoles.PreviewTextView,
            PredefinedTextViewRoles.PrimaryDocument,
            PredefinedTextViewRoles.Printable,
            PredefinedTextViewRoles.Structured,
            PredefinedTextViewRoles.Zoomable,
        };

    public static readonly IEnumerable<string> AllPredefinedTextViewRoles = new string[] {
            PredefinedTextViewRoles.Analyzable,
            PredefinedTextViewRoles.ChangePreview,
            PredefinedTextViewRoles.CodeDefinitionView,
            PredefinedTextViewRoles.Debuggable,
            PredefinedTextViewRoles.Document,
            PredefinedTextViewRoles.Editable,
            PredefinedTextViewRoles.EmbeddedPeekTextView,
            PredefinedTextViewRoles.Interactive,
            PredefinedTextViewRoles.PreviewTextView,
            PredefinedTextViewRoles.PrimaryDocument,
            PredefinedTextViewRoles.Printable,
            PredefinedTextViewRoles.Structured,
            PredefinedTextViewRoles.Zoomable,
        };


    public Hoster(
        IVsRegisterPriorityCommandTarget vsRegisterPriorityCommandTarget,
        IVsEditorAdaptersFactoryService vsEditorAdaptersFactory,
        Microsoft.VisualStudio.OLE.Interop.IServiceProvider oleServiceProvider,
        IVsFilterKeys2 filterKeys,
        IVsTextLines pBuffer,
        IEnumerable<string> textViewRoles,
        Dictionary<Guid, uint[]> allowedCommands,
        Action<IWpfTextView> initializeTextView,
        bool isReadOnly,
        CodeWindowBehaviourFlags codeWindowBehaviourFlags = CodeWindowBehaviourFlags.CWB_DISABLEDROPDOWNBAR | CodeWindowBehaviourFlags.CWB_DISABLESPLITTER | CodeWindowBehaviourFlags.CWB_DISABLEDIFF
    ) {
        this.vsRegisterPriorityCommandTarget = vsRegisterPriorityCommandTarget;
        this.vsEditorAdaptersFactory = vsEditorAdaptersFactory;
        this.oleServiceProvider = oleServiceProvider;
        this.filterKeys = filterKeys;
        this.pBuffer = pBuffer;
        this.textViewRoles = String.Join(",", textViewRoles);
        this.initializeTextView = initializeTextView;
        this.allowedCommands = allowedCommands;
        this.isReadOnly = isReadOnly;
        this.codeWindowBehaviourFlags = codeWindowBehaviourFlags;
    }

    private void CreateTextViewHost() {
        this.codeWindow = vsEditorAdaptersFactory.CreateVsCodeWindowAdapter(this.oleServiceProvider);
        ((IVsCodeWindowEx)this.codeWindow).Initialize((uint)codeWindowBehaviourFlags, VSUSERCONTEXTATTRIBUTEUSAGE.VSUC_Usage_Filter, string.Empty, string.Empty, 196608U, new INITVIEW[1]);
        var codeWindow = this.codeWindow as IVsUserData;
        Guid textViewRolesGuid = VSConstants.VsTextBufferUserDataGuid.VsTextViewRoles_guid;
        codeWindow.SetData(ref textViewRolesGuid, (object)textViewRoles);
        this.codeWindow.SetBuffer(pBuffer);
        IVsTextView ppView;
        this.codeWindow.GetPrimaryView(out ppView);
        this.textView = ppView;
        this.wpfTextViewHost = vsEditorAdaptersFactory.GetWpfTextViewHost(this.textView);
        IWpfTextView textView = this.wpfTextViewHost.TextView;
        if (isReadOnly) {
            textView.Options.SetOptionValue<bool>(DefaultTextViewOptions.ViewProhibitUserInputId, true);
        }
        this.initializeTextView(textView);
    }

    public IWpfTextViewHost GetTextViewHost() {
        ThreadHelper.ThrowIfNotOnUIThread();
        CreateTextViewHost();
        this.wpfTextViewHost.HostControl.IsKeyboardFocusWithinChanged += HostControl_IsKeyboardFocusWithinChanged;
        this.wpfTextViewHost.HostControl.TabIndex = 0;
        this.hasFocus = this.wpfTextViewHost.HostControl.IsKeyboardFocusWithin;

        this.commandFilter = new CodeEditorCommandFilter(this.allowedCommands);
        this.vsRegisterPriorityCommandTarget.RegisterPriorityCommandTarget(0U, this.commandFilter, out unregisterPriorityCommandTargetCookie);
        this.commandFilter.HasFocus = this.hasFocus;
        this.commandFilter.NextCommandTarget = EmbeddedObjectHelper.GetOleCommandTarget((UIElement)this.wpfTextViewHost.HostControl);

        // does not show in a tooltip
        //return (object)(UIElement)((WindowPane)this.codeWindow).Content;

        // this does 
        return this.wpfTextViewHost;
    }

    // When is necessary - if I remove the handler a-z, backspace, arrow key presses are still received by the CodeEditorCommandFilter....
    private void HostControl_IsKeyboardFocusWithinChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) {
        this.hasFocus = (bool)e.NewValue;
        if (this.commandFilter != null)
            this.commandFilter.HasFocus = this.hasFocus;
        if (this.hasFocus) {
            //https://learn.microsoft.com/en-us/dotnet/api/system.windows.interop.componentdispatcher.threadfiltermessage?view=windowsdesktop-7.0
            // ComponentDispatcher - Enables shared control of the message pump between Win32 and WPF in interoperation scenarios.
            ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(this.FilterThreadMessage);
        }
        else {
            ComponentDispatcher.ThreadFilterMessage -= new ThreadMessageEventHandler(this.FilterThreadMessage);
        }
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
        if (this.filterKeys == null || msg.message < 256 || msg.message > 264)
            return;
        Microsoft.VisualStudio.OLE.Interop.MSG msg1 = new Microsoft.VisualStudio.OLE.Interop.MSG() {
            hwnd = msg.hwnd,
            lParam = msg.lParam,
            wParam = msg.wParam,
            message = (uint)msg.message
        };
        Guid pguidCmd;
        uint pdwCmd;
        int num1;
        int num2;
        if (!ErrorHandler.Succeeded(this.filterKeys.TranslateAcceleratorEx(new Microsoft.VisualStudio.OLE.Interop.MSG[1]
        {
        msg1
        }, 21U, 0U, new Guid[0], out pguidCmd, out pdwCmd, out num1, out num2)))
            return;

        var cmdAndId = $"{pguidCmd} {pdwCmd}";
        if (!this.commandFilter.IsCommandAllowed(ref pguidCmd, pdwCmd)) {
            Debug.WriteLine($"Command {cmdAndId} is not allowed");
            return;
        }
        Debug.WriteLine($"Executing {cmdAndId}");
        int hr = this.filterKeys.TranslateAcceleratorEx(new Microsoft.VisualStudio.OLE.Interop.MSG[1]
        {
        msg1
        }, 20U, 0U, new Guid[0], out Guid _, out uint _, out num2, out num1);
        handled = ErrorHandler.Succeeded(hr);
    }

    protected virtual void Dispose(bool disposing) {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (!disposedValue) {
            if (disposing) {
                // TODO: dispose managed state (managed objects
                this.vsRegisterPriorityCommandTarget.UnregisterPriorityCommandTarget(this.unregisterPriorityCommandTargetCookie);
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Hoster()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}