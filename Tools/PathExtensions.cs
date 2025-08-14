using System;
using System.IO;
using System.Linq;

namespace QuickJump2022.Tools;

public static class PathExtensions {
    /// <summary>
    /// Reverses a file path by reversing the order of its components and joining them with forward slashes.
    /// For example: "C:\Folder\Subfolder\File" becomes "File/Subfolder/Folder/C:"
    /// </summary>
    /// <param name="path">The path to reverse</param>
    /// <returns>The reversed path with components joined by forward slashes</returns>
    public static string ReversePath(this string path) {
        if (string.IsNullOrWhiteSpace(path)) return path;
        var parts = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
        var reversedParts = parts.Reverse();
        return string.Join("/", reversedParts);
    }
}
