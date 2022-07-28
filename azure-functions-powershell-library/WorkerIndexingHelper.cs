//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctions.PowerShell.SDK;
using System.Collections.ObjectModel;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShellWorker
{
    internal class WorkerIndexingHelper
    {
        internal static List<FunctionInformation> IndexFunctions(string baseDir)
        {
            if (!Directory.Exists(baseDir))
            {
                throw new FileNotFoundException();
            }
            List<FileInfo> powerShellFiles = GetPowerShellFiles(Directory.CreateDirectory(baseDir));
            List<FunctionInformation> rpcFunctionMetadatas = new List<FunctionInformation>();

            foreach (FileInfo powerShellFile in powerShellFiles)
            {
                rpcFunctionMetadatas.AddRange(IndexFunctionsInFile(powerShellFile));
            }

            return rpcFunctionMetadatas;
        }

        private static IEnumerable<FunctionInformation> IndexFunctionsInFile(FileInfo powerShellFile)
        {
            List<FunctionInformation> fileFunctions = new List<FunctionInformation>();
            ScriptBlockAst? fileAst = Parser.ParseFile(powerShellFile.FullName, out _, out ParseError[] errors);
            if (errors.Any())
            {
                throw new Exception($"Couldn't parse the file: {powerShellFile.FullName}");
                // TODO: Probably don't throw here?
                //return fileFunctions;
            }
            if (powerShellFile.Extension == ".ps1") 
            {
                // parse only the file param block, return one RpcFunctionMetadata assuming the file is the entry point
                ParamBlockAst paramAsts = fileAst.ParamBlock;
                if (paramAsts != null && paramAsts.Attributes.Where(x => x.TypeName.ToString() == "Function").Any())
                {
                    // This is a function, return it 
                    FunctionInformation functionInformation = CreateRpcMetadataFromFile(powerShellFile.FullName);
                    AddFunctionIfNameUnique(functionInformation, fileFunctions);
                }
            }
            else if (powerShellFile.Extension == ".psm1")
            {
                // parse all function definitions, return as many RpcFunctionMetadatas as exist in the file
                IEnumerable<Ast>? potentialFunctions = fileAst.FindAll(x => x is FunctionDefinitionAst, false);
                foreach (Ast potentialFunction in potentialFunctions)
                {
                    IEnumerable<Ast>? matchingBlocks = potentialFunction.FindAll(x => x is ParamBlockAst && ((ParamBlockAst)x).Attributes.Where(z => z.TypeName.ToString() == "Function").Any(), true);
                    if (matchingBlocks.Any()) {
                        //This function is one we need to register
                        FunctionInformation functionInformation = CreateRpcMetadataFromFunctionAst(powerShellFile.FullName, (FunctionDefinitionAst)potentialFunction);
                        AddFunctionIfNameUnique(functionInformation, fileFunctions);
                    }
                }
            }
            return fileFunctions;
        }

        private static void AddFunctionIfNameUnique(FunctionInformation functionInformation, List<FunctionInformation> fileFunctions)
        {
            if (!fileFunctions.Select(x => x.Name).Contains(functionInformation.Name))
            {
                fileFunctions.Add(functionInformation);
            }
            else
            {
                throw new Exception($"Multiple functions declared with name: {functionInformation.Name}");
            }
        }

        private static FunctionInformation CreateRpcMetadataFromFile(string powerShellFile)
        {
            ScriptBlockAst? fileAst = Parser.ParseFile(powerShellFile, out _, out ParseError[] errors);

            FunctionInformation thisFunction = new FunctionInformation();

            thisFunction.Directory = new FileInfo(powerShellFile).Directory!.FullName;
            thisFunction.ScriptFile = powerShellFile;
            thisFunction.Name = Path.GetFileName(powerShellFile).Split('.').First();

            thisFunction.FunctionId = Guid.NewGuid().ToString();
            ExtractBindings(thisFunction, fileAst.ParamBlock);

            return thisFunction;
        }
        private static FunctionInformation CreateRpcMetadataFromFunctionAst(string powerShellFile, FunctionDefinitionAst potentialFunction)
        {
            FunctionInformation thisFunction = new FunctionInformation();

            thisFunction.Directory = new FileInfo(powerShellFile).Directory!.FullName;
            thisFunction.ScriptFile = powerShellFile;
            thisFunction.Name = potentialFunction.Name;
            thisFunction.EntryPoint = potentialFunction.Name;

            thisFunction.FunctionId = Guid.NewGuid().ToString();

            ParamBlockAst paramBlock = (ParamBlockAst)potentialFunction.Find(x => x is ParamBlockAst, true);
            ExtractBindings(thisFunction, paramBlock);

            return thisFunction;
        }

        private static void ExtractBindings(FunctionInformation thisFunction, ParamBlockAst paramBlock)
        {
            if (paramBlock == null)
            {
                return;
            }

            IEnumerable<AttributeAst>? functionAttribute = paramBlock.Attributes.Where(x => x.TypeName.Name == "Function" && x.PositionalArguments.Count > 0);
            if (functionAttribute.Any() && functionAttribute.First().PositionalArguments[0].GetType() == typeof(StringConstantExpressionAst))
            {
                thisFunction.Name = ((StringConstantExpressionAst)functionAttribute.First().PositionalArguments[0]).Value;
            }

            List<BindingInformation> inputBindings = GetInputBindingInfo(paramBlock);
            List<BindingInformation> outputBindings = GetOutputBindingInfo(paramBlock.Attributes);
            List<BindingInformation> missingBindings = BindingExtractor.GetDefaultBindings(inputBindings, outputBindings);

            thisFunction.Bindings.AddRange(inputBindings);
            thisFunction.Bindings.AddRange(outputBindings);
            thisFunction.Bindings.AddRange(missingBindings);
        }
        private static List<BindingInformation> GetInputBindingInfo(ParamBlockAst paramBlock)
        {
            List<BindingInformation> outputBindingInfo = new List<BindingInformation>();
            foreach (ParameterAst parameter in paramBlock.Parameters)
            {
                foreach (AttributeAst attribute in parameter.Attributes)
                {
                    BindingInformation? bindingInfo = BindingExtractor.ExtractInputBinding(attribute, parameter);
                    if (bindingInfo is not null)
                    {
                        outputBindingInfo.Add(bindingInfo);
                    }
                }
            }
            return outputBindingInfo;
        }

        private static List<BindingInformation> GetOutputBindingInfo(ReadOnlyCollection<AttributeAst> attributes)
        {
            List<BindingInformation> outputBindingInfo = new List<BindingInformation>();
            foreach (AttributeAst attribute in attributes)
            {
                BindingInformation? bindingInformation = BindingExtractor.ExtractOutputBinding(attribute);
                if (bindingInformation is not null)
                {
                    outputBindingInfo.Add(bindingInformation);
                }
            }
            return outputBindingInfo;
        }

        // Everything below this point are static methods that may be used by classes implementing IBinding to extract information from 
        //   the AST. Perhaps there is a better place for these to live?

        public static string? GetPositionalArgumentStringValue(AttributeAst attribute, int attributeIndex, string? defaultValue=null)
        {
            return attribute.PositionalArguments.Count > attributeIndex 
                   && attribute.PositionalArguments[attributeIndex].GetType() == typeof(StringConstantExpressionAst)
                ? ((StringConstantExpressionAst)attribute.PositionalArguments[attributeIndex]).Value : defaultValue;
        }

        public static List<string>? ExtractOneOrMore(ExpressionAst expressionAst)
        {
            if (expressionAst.GetType() == typeof(StringConstantExpressionAst)) 
            {
                return new List<string> { ((StringConstantExpressionAst)expressionAst).Value };
            }
            else if (expressionAst.GetType() == typeof(ArrayExpressionAst))
            {
                List<string> values = new List<string>();
                IEnumerable<Ast>? arrayValues = ((ArrayExpressionAst)expressionAst).FindAll(x => x is StringConstantExpressionAst, false);
                foreach (StringConstantExpressionAst one in arrayValues)
                {
                    values.Add(one.Value);
                }
                return values;
            }
            return null;
        }

        private static List<FileInfo> GetPowerShellFiles(DirectoryInfo baseDir, int depth=0)
        {
            List<FileInfo> files = baseDir.GetFiles("*.ps1", SearchOption.TopDirectoryOnly).ToList();
            files.AddRange(baseDir.GetFiles("*.psm1", SearchOption.TopDirectoryOnly).ToList());
            if (depth > 0)
            {
                foreach (DirectoryInfo d in baseDir.GetDirectories())
                {
                    //folders.Add(d);
                    // if (MasterFolderCounter > maxFolders) 
                    files.AddRange(GetPowerShellFiles(d, depth - 1));
                }
            }
            return files;
        }
    }
}
