using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using QuickJump2022.Data;
using QuickJump2022.Models;
using QuickJump2022.Tools;
using Document = EnvDTE.Document;

namespace QuickJump2022;

/// <summary>
/// Document parsing using Roslyn Workspace API (recommended)
/// </summary>
public partial class QuickJumpData {
    private VisualStudioWorkspace _workspace;
    
    /// <summary>
    /// Initialize the Roslyn workspace service
    /// </summary>
    private void InitializeWorkspace() {
        ThreadHelper.ThrowIfNotOnUIThread();
        var componentModel = m_Package.GetService<SComponentModel, IComponentModel>();
        _workspace = componentModel.GetService<VisualStudioWorkspace>();
    }
    
    /// <summary>
    /// Get code items using Roslyn Workspace API - no file reading required!
    /// </summary>
    public async Task<List<CodeItem>> GetCodeItemsUsingWorkspaceAsync(Document dteDocument)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        
        var list = new List<CodeItem>();
        var filePath = dteDocument.ProjectItem.TryGetProperty<string>("FullPath");
        if (string.IsNullOrEmpty(filePath))
        {
            return list;
        }
        
        // Find the Roslyn document in the workspace
        var documentId = _workspace.CurrentSolution.GetDocumentIdsWithFilePath(filePath).FirstOrDefault();
        if (documentId == null)
        {
            return list;
        }
        
        var document = _workspace.CurrentSolution.GetDocument(documentId);
        if (document == null)
        {
            return list;
        }
        
        // Get the semantic model - VS already has this cached!
        var semanticModel = await document.GetSemanticModelAsync();
        var syntaxRoot = await document.GetSyntaxRootAsync();
        
        // Process syntax nodes with full semantic information
        ProcessSyntaxNodeWithSemantics(syntaxRoot, list, dteDocument, semanticModel);
        
        return list;
    }
    
    /// <summary>
    /// Alternative: Use SymbolFinder to get all symbols in a document
    /// </summary>
    public async Task<List<CodeItem>> GetCodeItemsUsingSymbolFinderAsync(Document dteDocument)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        
        var list = new List<CodeItem>();
        var filePath = dteDocument.ProjectItem.TryGetProperty<string>("FullPath");
        if (string.IsNullOrEmpty(filePath))
        {
            return list;
        }
        
        var documentId = _workspace.CurrentSolution.GetDocumentIdsWithFilePath(filePath).FirstOrDefault();
        if (documentId == null)
        {
            return list;
        }
        
        var document = _workspace.CurrentSolution.GetDocument(documentId);
        if (document == null)
        {
            return list;
        }
        
        // Get all declared symbols in the document
        var symbols = await SymbolFinder.FindSourceDeclarationsAsync(
            document.Project, 
            symbol => true, // Get all symbols
            CancellationToken.None);
        
        var semanticModel = await document.GetSemanticModelAsync();
        
        foreach (var symbol in symbols.Where(s => s.Locations.Any(l => l.SourceTree?.FilePath == filePath)))
        {
            var codeItem = ConvertSymbolToCodeItem(symbol, dteDocument);
            if (codeItem != null)
            {
                list.Add(codeItem);
            }
        }
        
        return list.OrderBy(item => item.Line).ToList();
    }
    
    private CodeItem ConvertSymbolToCodeItem(ISymbol symbol, Document dteDocument)
    {
        var location = symbol.Locations.FirstOrDefault();
        if (location == null || !location.IsInSource)
        {
            return null;
        }
        
        var lineSpan = location.GetLineSpan();
        var line = lineSpan.StartLinePosition.Line + 1;
        
        var bindType = GetBindTypeFromSymbol(symbol);
        var accessType = GetAccessTypeFromSymbol(symbol);
        var name = GetSymbolDisplayName(symbol);
        var nameOnly = GetSymbolName(symbol);
        var type = GetSymbolType(symbol);
        
        if (bindType == Enums.EBindType.None || string.IsNullOrEmpty(name))
        {
            return null;
        }
        
        return new CodeItem
        {
            ProjDocument = dteDocument,
            Line = line,
            BindType = bindType,
            AccessType = accessType,
            Name = name,
            NameOnly = nameOnly,
            Type = type
        };
    }
    
    private Enums.EBindType GetBindTypeFromSymbol(ISymbol symbol)
    {
        return symbol switch
        {
            INamedTypeSymbol namedType => namedType.TypeKind switch
            {
                TypeKind.Class => Enums.EBindType.Class,
                TypeKind.Struct => Enums.EBindType.Struct,
                TypeKind.Interface => Enums.EBindType.Interface,
                TypeKind.Enum => Enums.EBindType.Enum,
                TypeKind.Delegate => Enums.EBindType.Delegate,
                _ => Enums.EBindType.None
            },
            IMethodSymbol method => method.MethodKind switch
            {
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
    
    private Enums.EAccessType GetAccessTypeFromSymbol(ISymbol symbol)
    {
        var accessType = Enums.EAccessType.None;
        
        // Access modifiers
        switch (symbol.DeclaredAccessibility)
        {
            case Accessibility.Public:
                accessType |= Enums.EAccessType.Public;
                break;
            case Accessibility.Private:
                accessType |= Enums.EAccessType.Private;
                break;
            case Accessibility.Protected:
                accessType |= Enums.EAccessType.Protected;
                break;
            case Accessibility.Internal:
                accessType |= Enums.EAccessType.Internal;
                break;
            case Accessibility.ProtectedOrInternal:
                accessType |= Enums.EAccessType.Protected | Enums.EAccessType.Internal;
                break;
        }
        
        // Other modifiers
        if (symbol.IsStatic)
            accessType |= Enums.EAccessType.Static;
        
        if (symbol.IsAbstract)
            accessType |= Enums.EAccessType.Abstract;
        
        if (symbol.IsVirtual)
            accessType |= Enums.EAccessType.Virtual;
        
        if (symbol.IsOverride)
            accessType |= Enums.EAccessType.Override;
        
        if (symbol.IsSealed)
            accessType |= Enums.EAccessType.Sealed;
        
        if (symbol.IsExtern)
            accessType |= Enums.EAccessType.Extern;
        
        if (symbol is IFieldSymbol field && field.IsConst)
            accessType |= Enums.EAccessType.Const;
        
        if (symbol is IFieldSymbol field2 && field2.IsReadOnly)
            accessType |= Enums.EAccessType.Readonly;
        
        if (symbol is IMethodSymbol method && method.IsAsync)
            accessType |= Enums.EAccessType.Async;
        
        if (symbol is INamedTypeSymbol type && type.IsRecord)
            accessType |= Enums.EAccessType.Record;
        
        return accessType;
    }
    
    private string GetSymbolName(ISymbol symbol)
    {
        if (symbol is IMethodSymbol method)
        {
            var format = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                // Just the method name
                memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
                // Show only parameter names
                parameterOptions: SymbolDisplayParameterOptions.IncludeName,
                // Suppress everything else
                genericsOptions: SymbolDisplayGenericsOptions.None,
                kindOptions: SymbolDisplayKindOptions.None,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.None
            );

            var displayName = symbol.ToDisplayString(format);

            if (method.MethodKind == MethodKind.Destructor)
            {
                return $"~{method.ContainingType.Name}";
            }
            return displayName;
        }
        else if (symbol is IPropertySymbol property && property.IsIndexer)
        {
            return "this";
        }
        else if (symbol is IFieldSymbol field)
        {
            return field.Name;
        }
        else if (symbol is IEventSymbol eventSymbol)
        {
            return eventSymbol.Name;
        }
        else if (symbol is INamedTypeSymbol namedType)
        {
            return namedType.Name;
        }
        
        return symbol.Name;
    }

    private string GetSymbolDisplayName(ISymbol symbol)
    {
        var format = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions: SymbolDisplayMemberOptions.IncludeParameters | 
                          SymbolDisplayMemberOptions.IncludeType,
            parameterOptions: SymbolDisplayParameterOptions.IncludeType | 
                             SymbolDisplayParameterOptions.IncludeName,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
        
        var displayName = symbol.ToDisplayString(format);
        
        // Add special formatting for certain symbol types
        if (symbol is IMethodSymbol method)
        {
            if (method.MethodKind == MethodKind.Destructor)
            {
                displayName = $"~{method.ContainingType.Name}()";
            }
            else if (!displayName.EndsWith(")"))
            {
                displayName += "()";
            }
        }
        else if (symbol is IPropertySymbol property && property.IsIndexer)
        {
            displayName = "this[]";
        }
        
        return displayName;
    }

    private string GetSymbolType(ISymbol symbol)
    {
        if (symbol is IMethodSymbol method)
        {
            if (method.MethodKind == MethodKind.Constructor || method.MethodKind == MethodKind.Destructor)
            {
                return string.Empty; // Constructors and destructors don't have return types
            }
            
            var returnType = method.ReturnType;
            if (returnType.SpecialType == SpecialType.System_Void)
            {
                return string.Empty; // Void methods don't show return type
            }
            
            return returnType.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }
        else if (symbol is IPropertySymbol property)
        {
            return property.Type.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }
        else if (symbol is IFieldSymbol field)
        {
            return field.Type.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }
        else if (symbol is IEventSymbol eventSymbol)
        {
            return eventSymbol.Type.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }
        else if (symbol is INamedTypeSymbol namedType)
        {
            return string.Empty; // Types don't have a separate type
        }
        
        return string.Empty;
    }
    
    private void ProcessSyntaxNodeWithSemantics(SyntaxNode node, List<CodeItem> list, 
        Document document, SemanticModel semanticModel)
    {
        // Similar to your existing ProcessSyntaxNode but with semantic information available
        foreach (var child in node.DescendantNodes())
        {
            if (child is MemberDeclarationSyntax memberDeclaration)
            {
                var symbol = semanticModel.GetDeclaredSymbol(memberDeclaration);
                if (symbol != null)
                {
                    var codeItem = ConvertSymbolToCodeItem(symbol, document);
                    if (codeItem != null)
                    {
                        list.Add(codeItem);
                    }
                }
            }
            else if (child is LocalFunctionStatementSyntax localFunction)
            {
                var symbol = semanticModel.GetDeclaredSymbol(localFunction);
                if (symbol != null)
                {
                    var codeItem = ConvertSymbolToCodeItem(symbol, document);
                    if (codeItem != null)
                    {
                        codeItem.Name += " (local)";
                        list.Add(codeItem);
                    }
                }
            }
        }
    }
}
