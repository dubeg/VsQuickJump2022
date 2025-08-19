using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.LanguageServices;
using QuickJump2022.Models;

namespace QuickJump2022.Services;

/// <summary>
/// Find symbols using Roslyn Workspace API
/// </summary>
public class SymbolService(VisualStudioWorkspace workspace) {

    public async Task<List<CodeItem>> GetCodeItemsForActiveDocumentAsync() {
        var documentView = await VS.Documents.GetActiveDocumentViewAsync();
        if (documentView is null) return new();
        var filePath = documentView.FilePath;
        var documentId = workspace.CurrentSolution.GetDocumentIdsWithFilePath(filePath).FirstOrDefault();
        if (documentId == null) return new();
        var document = workspace.CurrentSolution.GetDocument(documentId);
        if (document == null) return new();
        var semanticModel = await document.GetSemanticModelAsync();
        var syntaxRoot = await document.GetSyntaxRootAsync();
        return ProcessSyntaxNodeWithSemantics(documentId.Id, syntaxRoot, semanticModel);
    }

    private List<CodeItem> ProcessSyntaxNodeWithSemantics(Guid documentId, SyntaxNode node, SemanticModel semanticModel) {
        var codeItems = new List<CodeItem>();
        foreach (var child in node.DescendantNodes()) {
            if (child is FieldDeclarationSyntax fieldDeclaration) {
                foreach (var var in fieldDeclaration.Declaration.Variables) {
                    var symbol = semanticModel.GetDeclaredSymbol(var);
                    if (symbol != null) {
                        var codeItem = ConvertSymbolToCodeItem(symbol, documentId);
                        if (codeItem != null) {
                            codeItems.Add(codeItem);
                        }
                    }
                }
            }
            else if (child is MemberDeclarationSyntax memberDeclaration) {
                var symbol = semanticModel.GetDeclaredSymbol(memberDeclaration);
                if (symbol != null) {
                    var codeItem = ConvertSymbolToCodeItem(symbol, documentId);
                    if (codeItem != null) {
                        codeItems.Add(codeItem);
                    }
                }
            }
            else if (child is LocalFunctionStatementSyntax localFunction) {
                var symbol = semanticModel.GetDeclaredSymbol(localFunction);
                if (symbol != null) {
                    var codeItem = ConvertSymbolToCodeItem(symbol, documentId);
                    if (codeItem != null) {
                        codeItem.Name += " (local)";
                        codeItems.Add(codeItem);
                    }
                }
            }
        }
        return codeItems;
    }

    private CodeItem ConvertSymbolToCodeItem(ISymbol symbol, Guid documentId) {
        // Ignore enum members (they are represented as fields in Roslyn)
        if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.ContainingType?.TypeKind == TypeKind.Enum) {
            return null;
        }
        var location = symbol.Locations.FirstOrDefault();
        if (location == null || !location.IsInSource) {
            return null;
        }
        var lineSpan = location.GetLineSpan();
        var line = lineSpan.StartLinePosition.Line + 1;
        var bindType = GetBindTypeFromSymbol(symbol);
        var accessType = GetAccessTypeFromSymbol(symbol);
        var name = GetSymbolDisplayName(symbol);
        var nameOnly = GetSymbolName(symbol);
        var type = GetSymbolType(symbol);
        if (bindType == Enums.EBindType.None || string.IsNullOrEmpty(name)) {
            return null;
        }
        return new CodeItem {
            DocumentId = documentId,
            Line = line,
            BindType = bindType,
            AccessType = accessType,
            Name = name,
            NameOnly = nameOnly,
            Type = type
        };
    }

    private Enums.EBindType GetBindTypeFromSymbol(ISymbol symbol) {
        return symbol switch {
            INamedTypeSymbol namedType => namedType.TypeKind switch {
                TypeKind.Class => Enums.EBindType.Class,
                TypeKind.Struct => Enums.EBindType.Struct,
                TypeKind.Interface => Enums.EBindType.Interface,
                TypeKind.Enum => Enums.EBindType.Enum,
                TypeKind.Delegate => Enums.EBindType.Delegate,
                _ => Enums.EBindType.None
            },
            IMethodSymbol method => method.MethodKind switch {
                MethodKind.Constructor => Enums.EBindType.Constructor,
                MethodKind.Destructor => Enums.EBindType.Method,
                MethodKind.BuiltinOperator => Enums.EBindType.Operator,
                MethodKind.UserDefinedOperator => Enums.EBindType.Operator,
                MethodKind.Conversion => Enums.EBindType.Operator,
                _ => Enums.EBindType.Method
            },
            IPropertySymbol => Enums.EBindType.Property,
            IFieldSymbol => Enums.EBindType.Field,
            IEventSymbol => Enums.EBindType.Event,
            _ => Enums.EBindType.None
        };
    }

    private Enums.EAccessType GetAccessTypeFromSymbol(ISymbol symbol) {
        var accessType = Enums.EAccessType.None;
        // Access modifiers
        switch (symbol.DeclaredAccessibility) {
            case Accessibility.Public: accessType |= Enums.EAccessType.Public; break;
            case Accessibility.Private: accessType |= Enums.EAccessType.Private; break;
            case Accessibility.Protected: accessType |= Enums.EAccessType.Protected; break;
            case Accessibility.Internal: accessType |= Enums.EAccessType.Internal; break;
            case Accessibility.ProtectedOrInternal: accessType |= Enums.EAccessType.Protected | Enums.EAccessType.Internal; break;
        }
        // Other modifiers
        if (symbol.IsStatic) accessType |= Enums.EAccessType.Static;
        if (symbol.IsAbstract) accessType |= Enums.EAccessType.Abstract;
        if (symbol.IsVirtual) accessType |= Enums.EAccessType.Virtual;
        if (symbol.IsOverride) accessType |= Enums.EAccessType.Override;
        if (symbol.IsSealed) accessType |= Enums.EAccessType.Sealed;
        if (symbol.IsExtern) accessType |= Enums.EAccessType.Extern;
        if (symbol is IFieldSymbol field && field.IsConst) accessType |= Enums.EAccessType.Const;
        if (symbol is IFieldSymbol field2 && field2.IsReadOnly) accessType |= Enums.EAccessType.Readonly;
        if (symbol is IMethodSymbol method && method.IsAsync) accessType |= Enums.EAccessType.Async;
        if (symbol is INamedTypeSymbol type && type.IsRecord) accessType |= Enums.EAccessType.Record;
        return accessType;
    }

    private string GetSymbolName(ISymbol symbol) {
        if (symbol is IMethodSymbol method) {
            var format = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly, // Just the method name
                memberOptions: SymbolDisplayMemberOptions.IncludeParameters, 
                parameterOptions: SymbolDisplayParameterOptions.IncludeName, // Show only parameter names
                genericsOptions: SymbolDisplayGenericsOptions.None,
                kindOptions: SymbolDisplayKindOptions.None,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.None
            );
            var displayName = symbol.ToDisplayString(format);
            if (method.MethodKind == MethodKind.Destructor) {
                return $"~{method.ContainingType.Name}";
            }
            return displayName;
        }
        else if (symbol is IPropertySymbol property && property.IsIndexer) return "this";
        else if (symbol is IFieldSymbol field) return field.Name;
        else if (symbol is IEventSymbol eventSymbol) return eventSymbol.Name;
        else if (symbol is INamedTypeSymbol namedType) return namedType.Name;
        return symbol.Name;
    }

    private string GetSymbolDisplayName(ISymbol symbol) {
        var format = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeType,
            parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
        );
        var displayName = symbol.ToDisplayString(format);
        // Add special formatting for certain symbol types
        if (symbol is IMethodSymbol method) {
            if (method.MethodKind == MethodKind.Destructor) {
                displayName = $"~{method.ContainingType.Name}()";
            }
            else if (!displayName.EndsWith(")")) {
                displayName += "()";
            }
        }
        else if (symbol is IPropertySymbol property && property.IsIndexer) {
            displayName = "this[]";
        }
        return displayName;
    }

    private string GetSymbolType(ISymbol symbol) {
        if (symbol is IMethodSymbol method) {
            if (method.MethodKind == MethodKind.Constructor || method.MethodKind == MethodKind.Destructor) {
                return string.Empty; // Constructors and destructors don't have return types
            }

            var returnType = method.ReturnType;
            if (returnType.SpecialType == SpecialType.System_Void) {
                return "void"; // Void methods don't show return type
            }

            return returnType.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }
        else if (symbol is IPropertySymbol property) {
            return property.Type.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }
        else if (symbol is IFieldSymbol field) {
            return field.Type.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }
        else if (symbol is IEventSymbol eventSymbol) {
            return eventSymbol.Type.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }
        else if (symbol is INamedTypeSymbol namedType) {
            return string.Empty; // Types don't have a separate type
        }

        return string.Empty;
    }
}
