using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickJump2022.Tools;

public static class DocUtils {
    /// <summary>
    /// Get the document cookies for the documents in the running document table
    /// </summary>
    /// <remarks>
    /// This method simple asks for the cookies and hence won't force the document to be loaded
    /// if it is being loaded in a lazy fashion
    /// </remarks>
    public static List<uint> GetRunningDocumentCookies(this IVsRunningDocumentTable runningDocumentTable) {
        var list = new List<uint>();
        if (!ErrorHandler.Succeeded(runningDocumentTable.GetRunningDocumentsEnum(out IEnumRunningDocuments enumDocuments))) {
            return list;
        }
        uint[] array = new uint[1];
        uint pceltFetched = 0;
        while (ErrorHandler.Succeeded(enumDocuments.Next(1, array, out pceltFetched)) && (pceltFetched == 1)) {
            list.Add(array[0]);
        }
        return list;
    }
}
