using System.Drawing;
using System.Linq.Expressions;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using QuickJump2022.Models;
using QuickJump2022.Options;

namespace QuickJump2022.Services;
public class SettingsService(GeneralOptionsPage GeneralOptions, IServiceProvider serviceProvider) {
    private string Category = "General";
    private SettingsScope UserSettings => SettingsScope.UserSettings;
    private SettingsStore GetReadOnlyStore() => new ShellSettingsManager(serviceProvider).GetReadOnlySettingsStore(UserSettings);
    private WritableSettingsStore GetWriteStore() => new ShellSettingsManager(serviceProvider).GetWritableSettingsStore(UserSettings);

    public void LoadSettings() {
        var store = GetReadOnlyStore();
        if (!store.CollectionExists(Category)) return;
        
        void LoadAndAssign<T>(Expression<Func<GeneralOptionsPage, T>> propertyExpression) {
            var memberExpr = propertyExpression.Body as System.Linq.Expressions.MemberExpression;
            if (memberExpr == null) throw new ArgumentException("Expression must be a property access", nameof(propertyExpression));
            var propertyName = memberExpr.Member.Name;
            var propertyInfo = typeof(GeneralOptionsPage).GetProperty(propertyName);
            if (propertyInfo == null || !store.PropertyExists(Category, propertyName)) {
                return;
            }
            object value = null;
            var propertyType = propertyInfo.PropertyType;
            switch (propertyType) {
                case Type t when t == typeof(bool):
                    value = store.GetBoolean(Category, propertyName);
                    break;
                case Type t when t == typeof(Color):
                    value = Color.FromName(store.GetString(Category, propertyName));
                    break;
                case Type t when t == typeof(Enums.SortType):
                    value = (Enums.SortType)store.GetInt32(Category, propertyName);
                    break;
                case Type t when t == typeof(int):
                    value = store.GetInt32(Category, propertyName);
                    break;
                case Type t when t == typeof(string):
                    value = store.GetString(Category, propertyName);
                    break;
                default:
                    throw new NotSupportedException($"Type {propertyType} is not supported for settings storage");
            }
            propertyInfo.SetValue(GeneralOptions, value);
        }

        // Boolean properties
        LoadAndAssign(x => x.ShowIcons);
        
        // Color properties
        LoadAndAssign(x => x.FileBackgroundColor);
        LoadAndAssign(x => x.FileDescriptionForegroundColor);
        LoadAndAssign(x => x.FileForegroundColor);
        LoadAndAssign(x => x.FileSelectedBackgroundColor);
        LoadAndAssign(x => x.FileSelectedDescriptionForegroundColor);
        LoadAndAssign(x => x.FileSelectedForegroundColor);
        LoadAndAssign(x => x.CodeBackgroundColor);
        LoadAndAssign(x => x.CodeDescriptionForegroundColor);
        LoadAndAssign(x => x.CodeForegroundColor);
        LoadAndAssign(x => x.CodeSelectedBackgroundColor);
        LoadAndAssign(x => x.CodeSelectedDescriptionForegroundColor);
        LoadAndAssign(x => x.CodeSelectedForegroundColor);
        
        // Enum properties
        LoadAndAssign(x => x.FileSortType);
        LoadAndAssign(x => x.CSharpSortType);
        LoadAndAssign(x => x.MixedSortType);
    }

    public void SaveSettings() {
        var store = GetWriteStore();
        store.SetBoolean(Category, "ShowIcons", GeneralOptions.ShowIcons);
        store.SetString(Category, "FileBackgroundColor", GeneralOptions.FileBackgroundColor.Name);
        store.SetString(Category, "FileDescriptionForegroundColor", GeneralOptions.FileDescriptionForegroundColor.Name);
        store.SetString(Category, "FileForegroundColor", GeneralOptions.FileForegroundColor.Name);
        store.SetString(Category, "FileSelectedBackgroundColor", GeneralOptions.FileSelectedBackgroundColor.Name);
        store.SetString(Category, "FileSelectedDescriptionForegroundColor", GeneralOptions.FileSelectedDescriptionForegroundColor.Name);
        store.SetString(Category, "FileSelectedForegroundColor", GeneralOptions.FileSelectedForegroundColor.Name);
        store.SetString(Category, "CodeBackgroundColor", GeneralOptions.CodeBackgroundColor.Name);
        store.SetString(Category, "CodeDescriptionForegroundColor", GeneralOptions.CodeDescriptionForegroundColor.Name);
        store.SetString(Category, "CodeForegroundColor", GeneralOptions.CodeForegroundColor.Name);
        store.SetString(Category, "CodeSelectedBackgroundColor", GeneralOptions.CodeSelectedBackgroundColor.Name);
        store.SetString(Category, "CodeSelectedDescriptionForegroundColor", GeneralOptions.CodeSelectedDescriptionForegroundColor.Name);
        store.SetString(Category, "CodeSelectedForegroundColor", GeneralOptions.CodeSelectedForegroundColor.Name);
        store.SetInt32(Category, "FileSortType", (int)GeneralOptions.FileSortType);
        store.SetInt32(Category, "CSharpSortType", (int)GeneralOptions.CSharpSortType);
        store.SetInt32(Category, "MixedSortType", (int)GeneralOptions.MixedSortType);
    }
}
