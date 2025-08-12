using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using QuickJump2022.Models;

namespace QuickJump2022.Tools;

public static class KnownMonikerUtils {

    public static ImageMoniker GetCodeMoniker(Enums.EBindType bindType) {
        return bindType switch {
            Enums.EBindType.Namespace => KnownMonikers.Namespace,
            Enums.EBindType.Class => KnownMonikers.Class,
            Enums.EBindType.Constructor => KnownMonikers.Method,
            Enums.EBindType.Method => KnownMonikers.Method,
            Enums.EBindType.Property => KnownMonikers.Property,
            Enums.EBindType.Field => KnownMonikers.Field,
            Enums.EBindType.Enum => KnownMonikers.Enumeration,
            Enums.EBindType.Delegate => KnownMonikers.Delegate,
            Enums.EBindType.Event => KnownMonikers.Event,
            Enums.EBindType.Interface => KnownMonikers.Interface,
            Enums.EBindType.Struct => KnownMonikers.Structure,
            _ => KnownMonikers.QuestionMark
        };
    }

    public static ImageMoniker GetFileMoniker(string fileExtension) {
        return fileExtension?.ToLowerInvariant() switch {
            ".cs" => KnownMonikers.CSFileNode,
            ".vb" => KnownMonikers.VBFileNode,
            ".fs" => KnownMonikers.FSFileNode,
            ".cpp" or ".cc" or ".cxx" => KnownMonikers.CPPFileNode,
            ".h" or ".hpp" => KnownMonikers.CPPHeaderFile,
            ".js" or ".ts" => KnownMonikers.JSScript,
            ".json" => KnownMonikers.JSONScript,
            ".xml" => KnownMonikers.XMLFile,
            ".xaml" => KnownMonikers.WPFFile,
            ".html" or ".htm" => KnownMonikers.HTMLFile,
            ".css" => KnownMonikers.StyleSheet,
            ".sql" => KnownMonikers.SQLQueryUnchecked,
            ".txt" => KnownMonikers.TextFile,
            ".md" => KnownMonikers.MarkdownFile,
            ".config" => KnownMonikers.ConfigurationFile,
            ".resx" => KnownMonikers.LocalResources, // Wrong icon
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" => KnownMonikers.Image,
            ".sln" => KnownMonikers.Solution,
            ".csproj" or ".vbproj" or ".fsproj" => KnownMonikers.CSProjectNode,
            _ => KnownMonikers.Document
        };
    }
}
