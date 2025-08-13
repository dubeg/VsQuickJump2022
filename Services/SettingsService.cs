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
        // Enum properties
        LoadAndAssign(x => x.FileSortType);
        LoadAndAssign(x => x.CSharpSortType);
        LoadAndAssign(x => x.MixedSortType);
        LoadAndAssign(x => x.UseSymbolColors);
    }

    public void SaveSettings() {
        var store = GetWriteStore();
        store.SetInt32(Category, nameof(GeneralOptions.FileSortType), (int)GeneralOptions.FileSortType);
        store.SetInt32(Category, nameof(GeneralOptions.CSharpSortType), (int)GeneralOptions.CSharpSortType);
        store.SetInt32(Category, nameof(GeneralOptions.MixedSortType), (int)GeneralOptions.MixedSortType);
        store.SetBoolean(Category, nameof(GeneralOptions.UseSymbolColors), GeneralOptions.UseSymbolColors);
    }
}
