using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using QuickJump2022.Data;
using QuickJump2022.Models;
using QuickJump2022.Tools;
using Document = EnvDTE.Document;

namespace QuickJump2022;

/// <summary>
/// Document parsing using manual compilation (not recommended)
/// </summary>
public partial class QuickJumpData {
    public List<CodeItem> GetCodeItemsUsingManualCompilation(Document document) {
        ThreadHelper.ThrowIfNotOnUIThread("GetCodeItems");
        var list = new List<CodeItem>();
        var path = document.ProjectItem.TryGetProperty<string>("FullPath");
        if (string.IsNullOrEmpty(path)) {
            return list;
        }
        var fileContent = File.ReadAllText(path);
        var syntaxTree = CSharpSyntaxTree.ParseText(fileContent, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest), "", null, default);
        var root = syntaxTree.GetRoot();

        // Create a simple compilation to get semantic information
        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var compilation = CSharpCompilation.Create("QuickJumpAnalysis", new[] { syntaxTree }, new[] { mscorlib });
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        // Process all descendant nodes
        ProcessSyntaxNode(root, list, document, semanticModel);

        GC.Collect();
        return list;
    }

    private void ProcessSyntaxNode(SyntaxNode node, List<CodeItem> list, Document document, SemanticModel semanticModel) {
        // Process all member declarations recursively
        foreach (var child in node.DescendantNodes()) {
            if (child is MemberDeclarationSyntax memberDeclaration) {
                var codeItem = ProcessMemberDeclaration(memberDeclaration, document, semanticModel);
                if (codeItem != null) {
                    list.Add(codeItem);
                }
            }
            // Handle local functions inside methods
            else if (child is LocalFunctionStatementSyntax localFunction) {
                var line = child.SyntaxTree.GetLineSpan(child.Span).StartLinePosition.Line + 1;
                var accessType = GetAccessTypeFromModifiers(localFunction.Modifiers);
                list.Add(new CodeItem {
                    ProjDocument = document,
                    Line = line,
                    BindType = Enums.EBindType.Method,
                    AccessType = accessType,
                    Name = localFunction.Identifier.Text + " (local)"
                });
            }
        }
    }

    private CodeItem ProcessMemberDeclaration(MemberDeclarationSyntax member, Document document, SemanticModel semanticModel) {
        var line = member.SyntaxTree.GetLineSpan(member.Span).StartLinePosition.Line + 1;
        var bindType = Enums.EBindType.None;
        var name = string.Empty;
        var accessType = Enums.EAccessType.None;

        switch (member) {
            case NamespaceDeclarationSyntax namespaceDecl:
                // Skip namespace declarations as they're not items we want to jump to
                return null;

            case FileScopedNamespaceDeclarationSyntax fileScopedNamespace:
                // Skip file-scoped namespace declarations
                return null;

            case ClassDeclarationSyntax classDecl:
                bindType = Enums.EBindType.Class;
                name = GetTypeNameWithGenerics(classDecl.Identifier, classDecl.TypeParameterList);
                accessType = GetAccessTypeFromModifiers(classDecl.Modifiers);
                break;

            case RecordDeclarationSyntax recordDecl:
                bindType = recordDecl.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
                    ? Enums.EBindType.RecordStruct
                    : Enums.EBindType.Record;
                name = GetTypeNameWithGenerics(recordDecl.Identifier, recordDecl.TypeParameterList);
                accessType = GetAccessTypeFromModifiers(recordDecl.Modifiers);
                break;

            case StructDeclarationSyntax structDecl:
                bindType = Enums.EBindType.Struct;
                name = GetTypeNameWithGenerics(structDecl.Identifier, structDecl.TypeParameterList);
                accessType = GetAccessTypeFromModifiers(structDecl.Modifiers);
                break;

            case InterfaceDeclarationSyntax interfaceDecl:
                bindType = Enums.EBindType.Interface;
                name = GetTypeNameWithGenerics(interfaceDecl.Identifier, interfaceDecl.TypeParameterList);
                accessType = GetAccessTypeFromModifiers(interfaceDecl.Modifiers);
                break;

            case EnumDeclarationSyntax enumDecl:
                bindType = Enums.EBindType.Enum;
                name = enumDecl.Identifier.Text;
                accessType = GetAccessTypeFromModifiers(enumDecl.Modifiers);
                break;

            case DelegateDeclarationSyntax delegateDecl:
                bindType = Enums.EBindType.Delegate;
                name = GetMethodNameWithGenerics(delegateDecl.Identifier, delegateDecl.TypeParameterList);
                accessType = GetAccessTypeFromModifiers(delegateDecl.Modifiers);
                break;

            case MethodDeclarationSyntax methodDecl:
                bindType = Enums.EBindType.Method;
                name = GetMethodNameWithGenerics(methodDecl.Identifier, methodDecl.TypeParameterList);
                accessType = GetAccessTypeFromModifiers(methodDecl.Modifiers);
                break;

            case ConstructorDeclarationSyntax constructorDecl:
                bindType = Enums.EBindType.Constructor;
                name = constructorDecl.Identifier.Text + "()";
                accessType = GetAccessTypeFromModifiers(constructorDecl.Modifiers);
                break;

            case DestructorDeclarationSyntax destructorDecl:
                bindType = Enums.EBindType.Method;
                name = "~" + destructorDecl.Identifier.Text + "()";
                accessType = GetAccessTypeFromModifiers(destructorDecl.Modifiers);
                break;

            case PropertyDeclarationSyntax propertyDecl:
                bindType = Enums.EBindType.Property;
                name = propertyDecl.Identifier.Text;
                accessType = GetAccessTypeFromModifiers(propertyDecl.Modifiers);
                break;

            case IndexerDeclarationSyntax indexerDecl:
                bindType = Enums.EBindType.Indexer;
                name = "this[]";
                accessType = GetAccessTypeFromModifiers(indexerDecl.Modifiers);
                break;

            case EventDeclarationSyntax eventDecl:
                bindType = Enums.EBindType.Event;
                name = eventDecl.Identifier.Text;
                accessType = GetAccessTypeFromModifiers(eventDecl.Modifiers);
                break;

            case EventFieldDeclarationSyntax eventFieldDecl:
                bindType = Enums.EBindType.Event;
                if (eventFieldDecl.Declaration.Variables.Any()) {
                    name = eventFieldDecl.Declaration.Variables.First().Identifier.Text;
                }
                accessType = GetAccessTypeFromModifiers(eventFieldDecl.Modifiers);
                break;

            case FieldDeclarationSyntax fieldDecl:
                bindType = Enums.EBindType.Field;
                if (fieldDecl.Declaration.Variables.Any()) {
                    name = fieldDecl.Declaration.Variables.First().Identifier.Text;
                }
                accessType = GetAccessTypeFromModifiers(fieldDecl.Modifiers);
                break;

            case OperatorDeclarationSyntax operatorDecl:
                bindType = Enums.EBindType.Operator;
                name = "operator " + operatorDecl.OperatorToken.Text;
                accessType = GetAccessTypeFromModifiers(operatorDecl.Modifiers);
                break;

            case ConversionOperatorDeclarationSyntax conversionOperatorDecl:
                bindType = Enums.EBindType.Operator;
                name = conversionOperatorDecl.ImplicitOrExplicitKeyword.Text + " operator " + conversionOperatorDecl.Type.ToString();
                accessType = GetAccessTypeFromModifiers(conversionOperatorDecl.Modifiers);
                break;

            default:
                return null;
        }

        if (bindType == Enums.EBindType.None || string.IsNullOrEmpty(name)) {
            return null;
        }

        return new CodeItem {
            ProjDocument = document,
            Line = line,
            BindType = bindType,
            AccessType = accessType,
            Name = name
        };
    }

    private string GetTypeNameWithGenerics(SyntaxToken identifier, TypeParameterListSyntax typeParameterList) {
        if (typeParameterList == null || !typeParameterList.Parameters.Any()) {
            return identifier.Text;
        }
        return identifier.Text + "<" + string.Join(", ", typeParameterList.Parameters.Select(p => p.Identifier.Text)) + ">";
    }

    private string GetMethodNameWithGenerics(SyntaxToken identifier, TypeParameterListSyntax typeParameterList) {
        var baseName = identifier.Text;
        if (typeParameterList != null && typeParameterList.Parameters.Any()) {
            baseName += "<" + string.Join(", ", typeParameterList.Parameters.Select(p => p.Identifier.Text)) + ">";
        }
        return baseName + "()";
    }

    private Enums.EAccessType GetAccessTypeFromModifiers(SyntaxTokenList modifiers) {
        var accessType = Enums.EAccessType.None;

        if (modifiers.Any(SyntaxKind.StaticKeyword)) {
            accessType |= Enums.EAccessType.Static;
        }
        if (modifiers.Any(SyntaxKind.ConstKeyword)) {
            accessType |= Enums.EAccessType.Const;
        }
        if (modifiers.Any(SyntaxKind.PublicKeyword)) {
            accessType |= Enums.EAccessType.Public;
        }
        if (modifiers.Any(SyntaxKind.PrivateKeyword)) {
            accessType |= Enums.EAccessType.Private;
        }
        if (modifiers.Any(SyntaxKind.ProtectedKeyword)) {
            accessType |= Enums.EAccessType.Protected;
        }
        if (modifiers.Any(SyntaxKind.InternalKeyword)) {
            accessType |= Enums.EAccessType.Internal;
        }
        if (modifiers.Any(SyntaxKind.AbstractKeyword)) {
            accessType |= Enums.EAccessType.Abstract;
        }
        if (modifiers.Any(SyntaxKind.VirtualKeyword)) {
            accessType |= Enums.EAccessType.Virtual;
        }
        if (modifiers.Any(SyntaxKind.OverrideKeyword)) {
            accessType |= Enums.EAccessType.Override;
        }
        if (modifiers.Any(SyntaxKind.SealedKeyword)) {
            accessType |= Enums.EAccessType.Sealed;
        }
        if (modifiers.Any(SyntaxKind.AsyncKeyword)) {
            accessType |= Enums.EAccessType.Async;
        }
        if (modifiers.Any(SyntaxKind.ReadOnlyKeyword)) {
            accessType |= Enums.EAccessType.Readonly;
        }
        if (modifiers.Any(SyntaxKind.PartialKeyword)) {
            accessType |= Enums.EAccessType.Partial;
        }
        if (modifiers.Any(SyntaxKind.ExternKeyword)) {
            accessType |= Enums.EAccessType.Extern;
        }

        // If no access modifier is specified, default to private for most members
        if ((accessType & (Enums.EAccessType.Public | Enums.EAccessType.Private | Enums.EAccessType.Protected | Enums.EAccessType.Internal)) == 0) {
            // In C#, members without explicit access modifiers are private by default
            accessType |= Enums.EAccessType.Private;
        }

        return accessType;
    }
}
