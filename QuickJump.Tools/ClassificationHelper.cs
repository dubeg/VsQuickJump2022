using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;

namespace QuickJump2022.QuickJump.Tools;
public static class ClassificationHelper {

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
    public static TextFormattingRunProperties GetClassificationFormat(string classificationTypeName, string appearanceCategory = "text") {
        var componentModel =
            QuickJumpData.Instance.Package.GetService<SComponentModel, IComponentModel>();
        // (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
        var registryService = componentModel.GetService<IClassificationTypeRegistryService>();
        var classificationFormatService = componentModel.GetService<IClassificationFormatMapService>();
        var classificationFormatMap = classificationFormatService.GetClassificationFormatMap(category: appearanceCategory);
        var classificationType = registryService.GetClassificationType(classificationTypeName);
        var props = classificationFormatMap.GetTextProperties(classificationType);
        return props;
    }
}
