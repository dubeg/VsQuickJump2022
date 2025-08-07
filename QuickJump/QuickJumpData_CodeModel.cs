//using System.Collections.Generic;
//using EnvDTE;
//using EnvDTE80;
//using Microsoft.VisualStudio.Shell;
//using QuickJump2022.Data;
//using QuickJump2022.Models;

//namespace QuickJump2022;

///// <summary>
///// Document parsing using EnvDTE CodeModel API
///// </summary>
//public partial class QuickJumpData
//{
//    /// <summary>
//    /// Get code items using EnvDTE CodeModel - VS maintains this automatically!
//    /// </summary>
//    public List<CodeItem> GetCodeItemsUsingCodeModel(Document document)
//    {
//        ThreadHelper.ThrowIfNotOnUIThread();

//        var list = new List<CodeItem>();

//        // Get the FileCodeModel from the document
//        FileCodeModel2 fileCodeModel = document.ProjectItem?.FileCodeModel as FileCodeModel2;
//        if (fileCodeModel == null)
//        {
//            return list;
//        }

//        // Process all code elements in the file
//        ProcessCodeElements(fileCodeModel.CodeElements, list, document);

//        return list;
//    }

//    private void ProcessCodeElements(CodeElements elements, List<CodeItem> list, Document document)
//    {
//        ThreadHelper.ThrowIfNotOnUIThread();

//        if (elements == null)
//            return;

//        foreach (CodeElement2 element in elements)
//        {
//            var codeItem = ConvertCodeElementToCodeItem(element, document);
//            if (codeItem != null)
//            {
//                list.Add(codeItem);
//            }

//            // Recursively process child elements
//            if (element.Children != null && element.Children.Count > 0)
//            {
//                ProcessCodeElements(element.Children, list, document);
//            }

//            // Special handling for classes/interfaces to get their members
//            if (element is CodeClass2 codeClass)
//            {
//                ProcessCodeElements(codeClass.Members, list, document);
//            }
//            else if (element is CodeInterface2 codeInterface)
//            {
//                ProcessCodeElements(codeInterface.Members, list, document);
//            }
//            else if (element is CodeStruct2 codeStruct)
//            {
//                ProcessCodeElements(codeStruct.Members, list, document);
//            }
//            else if (element is CodeEnum codeEnum)
//            {
//                ProcessCodeElements(codeEnum.Members, list, document);
//            }
//            else if (element is CodeNamespace codeNamespace)
//            {
//                ProcessCodeElements(codeNamespace.Members, list, document);
//            }
//        }
//    }

//    private CodeItem ConvertCodeElementToCodeItem(CodeElement2 element, Document document)
//    {
//        ThreadHelper.ThrowIfNotOnUIThread();

//        var bindType = GetBindTypeFromCodeElement(element);
//        if (bindType == Enums.EBindType.None)
//        {
//            return null;
//        }

//        // Skip namespace declarations if needed
//        if (element.Kind == vsCMElement.vsCMElementNamespace)
//        {
//            return null;
//        }

//        var startPoint = element.GetStartPoint(vsCMPart.vsCMPartHeader);
//        var line = startPoint.Line;
//        var name = GetCodeElementDisplayName(element);
//        var accessType = GetAccessTypeFromCodeElement(element);

//        return new CodeItem
//        {
//            ProjDocument = document,
//            Line = line,
//            BindType = bindType,
//            AccessType = accessType,
//            Name = name
//        };
//    }

//    private Enums.EBindType GetBindTypeFromCodeElement(CodeElement element)
//    {
//        ThreadHelper.ThrowIfNotOnUIThread();

//        return element.Kind switch
//        {
//            vsCMElement.vsCMElementClass => Enums.EBindType.Class,
//            vsCMElement.vsCMElementInterface => Enums.EBindType.Interface,
//            vsCMElement.vsCMElementStruct => Enums.EBindType.Struct,
//            vsCMElement.vsCMElementEnum => Enums.EBindType.Enum,
//            vsCMElement.vsCMElementDelegate => Enums.EBindType.Delegate,
//            vsCMElement.vsCMElementFunction => Enums.EBindType.Method,
//            vsCMElement.vsCMElementProperty => Enums.EBindType.Property,
//            vsCMElement.vsCMElementVariable => Enums.EBindType.Field,
//            vsCMElement.vsCMElementEvent => Enums.EBindType.Event,
//            _ => Enums.EBindType.None
//        };
//    }

//    private Enums.EAccessType GetAccessTypeFromCodeElement(CodeElement element)
//    {
//        ThreadHelper.ThrowIfNotOnUIThread();

//        var accessType = Enums.EAccessType.None;

//        // Get access modifier based on element type
//        vsCMAccess access = vsCMAccess.vsCMAccessPrivate;
//        bool isStatic = false;
//        bool isAbstract = false;
//        bool isVirtual = false;
//        bool isOverride = false;
//        bool isSealed = false;
//        bool isConst = false;
//        bool isReadOnly = false;

//        switch (element)
//        {
//            case CodeClass2 codeClass:
//                access = codeClass.Access;
//                isAbstract = codeClass.IsAbstract;
//                isSealed = (codeClass.InheritanceKind & vsCMInheritanceKind.vsCMInheritanceKindSealed) != 0;
//                break;

//            case CodeInterface2 codeInterface:
//                access = codeInterface.Access;
//                break;

//            case CodeStruct2 codeStruct:
//                access = codeStruct.Access;
//                break;

//            case CodeEnum codeEnum:
//                access = codeEnum.Access;
//                break;

//            case CodeDelegate2 codeDelegate:
//                access = codeDelegate.Access;
//                break;

//            case CodeFunction2 codeFunction:
//                access = codeFunction.Access;
//                isStatic = codeFunction.IsShared;
//                isAbstract = (codeFunction.FunctionKind & vsCMFunction.vsCMFunctionAbstract) != 0;
//                isVirtual = (codeFunction.FunctionKind & vsCMFunction.vsCMFunctionVirtual) != 0;
//                isOverride = codeFunction.OverrideKind != vsCMOverrideKind.vsCMOverrideKindNone;
//                break;

//            case CodeProperty2 codeProperty:
//                access = codeProperty.Access;
//                break;

//            case CodeVariable2 codeVariable:
//                access = codeVariable.Access;
//                isStatic = codeVariable.IsShared;
//                isConst = codeVariable.IsConstant;
//                break;

//            case CodeEvent codeEvent:
//                access = codeEvent.Access;
//                isStatic = codeEvent.IsShared;
//                break;
//        }

//        // Convert access modifier
//        switch (access)
//        {
//            case vsCMAccess.vsCMAccessPublic:
//                accessType |= Enums.EAccessType.Public;
//                break;
//            case vsCMAccess.vsCMAccessPrivate:
//                accessType |= Enums.EAccessType.Private;
//                break;
//            case vsCMAccess.vsCMAccessProtected:
//                accessType |= Enums.EAccessType.Protected;
//                break;
//            case vsCMAccess.vsCMAccessProject:
//                accessType |= Enums.EAccessType.Internal;
//                break;
//            case vsCMAccess.vsCMAccessProjectOrProtected:
//                accessType |= Enums.EAccessType.Protected | Enums.EAccessType.Internal;
//                break;
//        }

//        // Add other modifiers
//        if (isStatic)
//            accessType |= Enums.EAccessType.Static;
//        if (isAbstract)
//            accessType |= Enums.EAccessType.Abstract;
//        if (isVirtual)
//            accessType |= Enums.EAccessType.Virtual;
//        if (isOverride)
//            accessType |= Enums.EAccessType.Override;
//        if (isSealed)
//            accessType |= Enums.EAccessType.Sealed;
//        if (isConst)
//            accessType |= Enums.EAccessType.Const;
//        if (isReadOnly)
//            accessType |= Enums.EAccessType.Readonly;

//        return accessType;
//    }

//    private string GetCodeElementDisplayName(CodeElement element)
//    {
//        ThreadHelper.ThrowIfNotOnUIThread();

//        var name = element.Name;

//        // Add special formatting based on element type
//        switch (element)
//        {
//            case CodeFunction2 func:
//                name = func.FullName;
//                if (func.FunctionKind == vsCMFunction.vsCMFunctionConstructor)
//                {
//                    // Constructor
//                    name = func.Name + "()";
//                }
//                else if (func.FunctionKind == vsCMFunction.vsCMFunctionDestructor)
//                {
//                    // Destructor
//                    name = "~" + func.Name + "()";
//                }
//                else if (func.FunctionKind == vsCMFunction.vsCMFunctionOperator)
//                {
//                    // Operator
//                    name = "operator " + func.Name;
//                }
//                else
//                {
//                    // Regular method - add parentheses
//                    if (!name.EndsWith(")"))
//                    {
//                        name += "()";
//                    }
//                }
//                break;

//            case CodeProperty2 prop:
//                // Check if it's an indexer
//                if (prop.Name == "this")
//                {
//                    name = "this[]";
//                }
//                break;

//            case CodeClass2 cls:
//                // Add generic parameters if any
//                if (cls.IsGeneric)
//                {
//                    var genericName = cls.FullName;
//                    if (genericName.Contains("<"))
//                    {
//                        var startIdx = genericName.IndexOf('<');
//                        var endIdx = genericName.LastIndexOf('>');
//                        if (startIdx >= 0 && endIdx > startIdx)
//                        {
//                            name = cls.Name + genericName.Substring(startIdx, endIdx - startIdx + 1);
//                        }
//                    }
//                }
//                break;
//        }

//        return name;
//    }
//}
