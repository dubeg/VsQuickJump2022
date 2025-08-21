using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using QuickJump2022.Models;

namespace QuickJump2022.Tools;

public static class KnownMonikerUtils {

    public static ImageMoniker GetCodeMoniker(Enums.TokenType bindType) {
        return bindType switch {
            Enums.TokenType.Namespace => KnownMonikers.Namespace,
            Enums.TokenType.Class => KnownMonikers.Class,
            Enums.TokenType.Constructor => KnownMonikers.Method,
            Enums.TokenType.Method => KnownMonikers.Method,
            Enums.TokenType.Property => KnownMonikers.Property,
            Enums.TokenType.Field => KnownMonikers.Field,
            Enums.TokenType.Enum => KnownMonikers.Enumeration,
            Enums.TokenType.Delegate => KnownMonikers.Delegate,
            Enums.TokenType.Event => KnownMonikers.Event,
            Enums.TokenType.Interface => KnownMonikers.Interface,
            Enums.TokenType.Struct => KnownMonikers.Structure,
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
