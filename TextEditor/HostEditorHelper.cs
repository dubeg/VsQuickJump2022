using Microsoft.CodeAnalysis.Classification;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJump2022.TextEditor;

internal static class HostEditorHelper {
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
    private static Services services;
    private static Services VsServices {
        get {
            services ??= new Services();
            return services;
        }
    }
    private class Services {
        public Services() {
            ContentTypeRegistryService = VS.GetMefService<IContentTypeRegistryService>();
            EditorOptionsFactoryService = VS.GetMefService<IEditorOptionsFactoryService>();
            VsEditorAdaptersFactoryService = VS.GetMefService<IVsEditorAdaptersFactoryService>();
            TextBufferFactoryService = VS.GetMefService<ITextBufferFactoryService>();
            TextEditorFactoryService = VS.GetMefService<ITextEditorFactoryService>();
            VsRegisterPriorityCommandTarget = VS.GetRequiredService<SVsRegisterPriorityCommandTarget, IVsRegisterPriorityCommandTarget>();
            VsFilterKeys = VS.GetRequiredService<SVsFilterKeys, IVsFilterKeys2>();

        }
        public IContentTypeRegistryService ContentTypeRegistryService { get; }
        public IEditorOptionsFactoryService EditorOptionsFactoryService { get; }
        public IVsEditorAdaptersFactoryService VsEditorAdaptersFactoryService { get; }
        public ITextBufferFactoryService TextBufferFactoryService { get; }
        public ITextEditorFactoryService TextEditorFactoryService { get; }
        public IVsRegisterPriorityCommandTarget VsRegisterPriorityCommandTarget { get; }
        public IVsFilterKeys2 VsFilterKeys { get; }
    }

    public static IVsTextLines GetTextLines(string contentTypeAsString, string content) {
        var contentType = VsServices.ContentTypeRegistryService.GetContentType(contentTypeAsString);
        var vsTextBuffer = VsServices.VsEditorAdaptersFactoryService.CreateVsTextBufferAdapter(OleServiceProvider, contentType);
        vsTextBuffer.InitializeContent(content, content.Length);
        return vsTextBuffer as IVsTextLines;
    }

    public static IWpfTextViewHost GetHost(string contentTypeAsString, string content) {
        var contentType = VsServices.ContentTypeRegistryService.GetContentType(contentTypeAsString);
        var vsTextBuffer = VsServices.TextBufferFactoryService.CreateTextBuffer(content, contentType);
        // it should have DefaultRoles
        var textView = VsServices.TextEditorFactoryService.CreateTextView(vsTextBuffer);

        return VsServices.TextEditorFactoryService.CreateTextViewHost(textView, true);
    }
    public static Hoster CreateHoster(
        string content,
        string contentTypeAsString,
        IEnumerable<string> textViewRoles,
        Dictionary<Guid, uint[]> allowedCommands,
        Action<IWpfTextView> initializeTextView,
        bool isReadOnly
    ) {
        return CreateHoster(GetTextLines(contentTypeAsString, content), textViewRoles, allowedCommands, initializeTextView, isReadOnly);
    }

    public static Hoster CreateHoster(
        IVsTextLines pBuffer,
        IEnumerable<string> textViewRoles,
        Dictionary<Guid, uint[]> allowedCommands,
        Action<IWpfTextView> initializeTextView,
        bool isReadOnly
    ) {
        return new Hoster(
            VsServices.VsRegisterPriorityCommandTarget,
            VsServices.VsEditorAdaptersFactoryService,
            OleServiceProvider,
            VsServices.VsFilterKeys,
            pBuffer,
            textViewRoles,
            allowedCommands,
            initializeTextView,
            isReadOnly
        );
    }
}
