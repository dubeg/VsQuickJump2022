using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;

namespace QuickJump2022.Tools;

public class ClassificationService(IServiceProvider serviceProvider) {
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
    public TextFormattingRunProperties GetClassificationFormat(string classificationTypeName, string appearanceCategory = "text") {
        var componentModel = serviceProvider.GetService<SComponentModel, IComponentModel>();
        var registryService = componentModel.GetService<IClassificationTypeRegistryService>();
        var classificationFormatService = componentModel.GetService<IClassificationFormatMapService>();
        var classificationFormatMap = classificationFormatService.GetClassificationFormatMap(category: appearanceCategory);
        var classificationType = registryService.GetClassificationType(classificationTypeName);
        var props = classificationFormatMap.GetTextProperties(classificationType);
        return props;
    }
}
