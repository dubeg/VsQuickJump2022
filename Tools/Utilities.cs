using EnvDTE;

namespace QuickJump2022.Tools;

public static class Utilities {
    public static T TryGetProperty<T>(this ProjectItem projectItem, string property) {
        ThreadHelper.ThrowIfNotOnUIThread("TryGetProperty");
        try {
            return (T)projectItem.Properties.Item((object)property).Value;
        }
        catch (Exception) {
            return default(T);
        }
    }
}
