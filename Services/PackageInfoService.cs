using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;

namespace QuickJump2022.Services;

public class PackageInfoService {
    public const string PackagesPath = "Packages";
    private SettingsStore _configurationStore;
    private List<PackageInfo> _packages = new();

    public PackageInfoService(IVsSettingsManager settingsManager) {
        var shellSettingsManager = new ShellSettingsManager(settingsManager);
        _configurationStore = shellSettingsManager.GetReadOnlySettingsStore(SettingsScope.Configuration);
    }

    public void PreloadPackagesCache() {
        foreach (var packageGuidString in _configurationStore.GetSubCollectionNames(PackagesPath)) {
            var packageGuid = Guid.Empty;
            if (!Guid.TryParse(packageGuidString, out packageGuid)) {
                continue;
            }
            var pkg = new PackageInfo(packageGuid, _configurationStore);
            _packages.Add(pkg);
        }
    }

    public List<PackageInfo> GetPackages() => _packages;
}

public class PackageInfo {
    public Guid PackageGuid { get; private set; }
    public string PackageName { get; private set; }
    public string ClassName { get; set; }
    public string CodeBase { get; set; }
    public bool IsAsyncPackage { get; private set; }

    public PackageInfo(Guid packageGuid, SettingsStore configurationStore) {
        PackageGuid = packageGuid;
        var packagePath = Path.Combine(PackageInfoService.PackagesPath, packageGuid.ToString("B"));
        if (configurationStore.CollectionExists(packagePath)) {
            PackageName =
                configurationStore.GetString(packagePath, string.Empty, "")
                ?? configurationStore.GetString(packagePath, "ProductName", "");
            ClassName = configurationStore.GetString(packagePath, "Class", "");
            CodeBase = configurationStore.GetString(packagePath, "CodeBase", "");
            IsAsyncPackage = configurationStore.GetBoolean(packagePath, "AllowsBackgroundLoad", false);
        }
    }
}