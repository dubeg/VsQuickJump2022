using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using QuickJump2022.Models;
using System.Collections.Generic;
using System.Windows.Media;

namespace QuickJump2022.Services;

public class ClassificationService(IServiceProvider serviceProvider) {
    private Dictionary<Enums.EBindType, Brush> _cache = new();
    private Brush _defaultBrush;

    public void PreloadCommonBrushes() {
        var mappings = new Dictionary<Enums.EBindType, string> {
            { Enums.EBindType.Class, "class name" },
            { Enums.EBindType.Method, "method name" },
            { Enums.EBindType.Property, "property name" },
            { Enums.EBindType.Field, "field name" },
            { Enums.EBindType.Enum, "enum name" },
            { Enums.EBindType.Interface, "interface name" },
            { Enums.EBindType.Struct, "struct name" },
            { Enums.EBindType.Delegate, "delegate name" },
            { Enums.EBindType.Constructor, "constructor name" },
            // "event name"
            // "operator"
            // "property name"
            // "record class name"
            // "record struct name"
            // "text"
        };
        var componentModel = serviceProvider.GetService<SComponentModel, IComponentModel>();
        var registryService = componentModel.GetService<IClassificationTypeRegistryService>();
        var classificationFormatService = componentModel.GetService<IClassificationFormatMapService>();
        var classificationFormatMap = classificationFormatService.GetClassificationFormatMap(category: "text");
        foreach (var mapping in mappings) {
            var classificationType = registryService.GetClassificationType(mapping.Value);
            if (classificationType != null) {
                var props = classificationFormatMap.GetTextProperties(classificationType);
                _cache.Add(mapping.Key, props.ForegroundBrush);
            }
        }
        _defaultBrush = GetClassificationFormat(PredefinedClassificationTypeNames.Text).ForegroundBrush;
    }

    public Brush GetFgColorForClassification(Enums.EBindType type) {
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
        var componentModel = serviceProvider.GetService<SComponentModel, IComponentModel>();
        var registryService = componentModel.GetService<IClassificationTypeRegistryService>();
        var classificationFormatService = componentModel.GetService<IClassificationFormatMapService>();
        var classificationFormatMap = classificationFormatService.GetClassificationFormatMap(category: appearanceCategory);
        var classificationType = registryService.GetClassificationType(classificationTypeName);
        var props = classificationFormatMap.GetTextProperties(classificationType);
        return props;
    }
}
