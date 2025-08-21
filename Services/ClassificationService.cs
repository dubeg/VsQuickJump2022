using System.Collections.Generic;
using System.Windows.Media;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using QuickJump2022.Models;

namespace QuickJump2022.Services;

public class ClassificationService {
    private IServiceProvider _serviceProvider;
    private Dictionary<Enums.TokenType, Brush> _cache = new();
    private Brush _defaultBrush;
    private Dictionary<Enums.TokenType, string> _mappings = new Dictionary<Enums.TokenType, string> {
        { Enums.TokenType.Text, "text" },
        { Enums.TokenType.Class, "class name" },
        { Enums.TokenType.Method, "method name" },
        { Enums.TokenType.Property, "property name" },
        { Enums.TokenType.Field, "field name" },
        { Enums.TokenType.Enum, "enum name" },
        { Enums.TokenType.Interface, "interface name" },
        { Enums.TokenType.Struct, "struct name" },
        { Enums.TokenType.Delegate, "delegate name" },
        { Enums.TokenType.Constructor, "method name" }, // "constructor name"
        { Enums.TokenType.ParameterName, "parameter name"},
        // "event name"
        // "operator"
        // "record class name"
        // "record struct name"
    };

    public ClassificationService(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
        VSColorTheme.ThemeChanged += (_) => PreloadCommonBrushes();
    }

    public void PreloadCommonBrushes() {
        _cache.Clear();
        var componentModel = _serviceProvider.GetService<SComponentModel, IComponentModel>();
        var registryService = componentModel.GetService<IClassificationTypeRegistryService>();
        var classificationFormatService = componentModel.GetService<IClassificationFormatMapService>();
        var classificationFormatMap = classificationFormatService.GetClassificationFormatMap(category: "text");
        foreach (var mapping in _mappings) {
            var classificationType = registryService.GetClassificationType(mapping.Value);
            if (classificationType != null) {
                var props = classificationFormatMap.GetTextProperties(classificationType);
                _cache.Add(mapping.Key, props.ForegroundBrush);
            }
        }
        _defaultBrush = GetClassificationFormat(PredefinedClassificationTypeNames.Text).ForegroundBrush;
    }

    public Brush GetFgColorForClassification(Enums.TokenType type) {
        if (_cache.TryGetValue(type, out var brush)) {
            return brush;
        }
        return _defaultBrush;
    }

    /// <summary>
    /// Get the styles defined for a token/symbol type.
    /// </summary>
    /// <param name="classificationTypeName">
    /// You can use PredefinedClassificationTypeNames to see predefined type names.
    /// Eg. Comment, Method, Type, ...
    /// </param>
    /// <remarks>
    /// You can find examples defined in a private field of "registryService":
    /// * parameter name
    /// * type parameter name
    /// * method name
    /// * extension method name
    /// * constant name
    /// * field name
    /// * enum name
    /// * enum member name
    /// * interface name
    /// * struct name
    /// * delegate name
    /// * class name
    /// * string
    /// * number
    /// * character
    /// * text
    /// * keyword
    /// * comment
    /// * identifier
    /// ...
    /// </remarks>
    private TextFormattingRunProperties GetClassificationFormat(string classificationTypeName, string appearanceCategory = "text") {
        var componentModel = _serviceProvider.GetService<SComponentModel, IComponentModel>();
        var registryService = componentModel.GetService<IClassificationTypeRegistryService>();
        var classificationFormatService = componentModel.GetService<IClassificationFormatMapService>();
        var classificationFormatMap = classificationFormatService.GetClassificationFormatMap(category: appearanceCategory);
        var classificationType = registryService.GetClassificationType(classificationTypeName);
        var props = classificationFormatMap.GetTextProperties(classificationType);
        return props;
    }
}
