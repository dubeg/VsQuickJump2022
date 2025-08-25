using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace QuickJump2022.VisualStudio.Interop;

public enum __SearchCandidateProcessingFlags {
    ConsiderOnlyEnabledCommands = 1,
    ConsiderOnlyVisibleCommands = 2,
    ConsiderDynamicText = 4,
    ConsiderCommandWellOnlyCommands = 8,
    ExpandDynamicItemStartCommands = 0x10
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("3411987D-3CB3-49D4-82D5-5364D1207B08")]
public interface SVsCommandSearchPrivate { }

[ComImport]
[Guid("78789DC6-FFBE-4E26-BE7E-B4F64BB80F0B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IVsFastFetchCommands {
    [MethodImpl(MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.Interface)]
    IVsFastFetchCommandsResult GetCommandEnumerator([In][ComAliasName("VsShellPrivate110.SearchCandidateProcessingFlags")] uint searchCandidateProcessingFlags, [In] uint scopeCount, [In][ComAliasName("Microsoft.Internal.VisualStudio.Shell.Interop.ScopeLocation2")][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ScopeLocation2[] scopeLocations);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("61A675A4-5FC0-453B-B509-31625242A139")]
public interface IVsFastFetchCommandsResult {
    [DispId(1610678272)]
    int Count {
        [MethodImpl(MethodImplOptions.InternalCall)]
        get;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    void GetCommands(int offset, int Count, [Out][ComAliasName("Microsoft.Internal.VisualStudio.Shell.Interop.CommandMetadata")][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] CommandMetadata[] commandsInfo);
}

public struct CommandMetadata {
    [ComAliasName("VsShellInterop160.VSCommandId")]
    public VSCommandId CommandId;

    [ComAliasName("ImageParameters140.ImageMoniker")]
    public ImageMoniker Icon;

    public uint DiscoveryOrder;

    [MarshalAs(UnmanagedType.BStr)]
    public string CommandPlacementText;

    [MarshalAs(UnmanagedType.BStr)]
    public string CommandKeyBinding;
}

public struct ScopeLocation2 {
    [ComAliasName("VsShellPrivate110.WellKnownScopeId")]
    public uint ScopeIds;

    public Guid ScopeGuid;

    public uint ScopeDWord;
}