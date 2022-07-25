//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctionsSDK;
using AzureFunctionsSDK.BundledBindings;
using Newtonsoft.Json;
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
            //this is only necessary until we fix the worker init crap
            powerShellFiles = powerShellFiles.OrderBy(x => x.FullName.Split(Path.DirectorySeparatorChar).Count() - baseDir.Split(Path.DirectorySeparatorChar).Count() == 2 ? 0 : 1).ToList();

            List<FunctionInformation> rpcFunctionMetadatas = new List<FunctionInformation>();
            //rpcFunctionMetadatas.Add(CreateFirstFunction());
            foreach (FileInfo powerShellFile in powerShellFiles)
            {
                rpcFunctionMetadatas.AddRange(IndexFunctionsInFile(powerShellFile));
            }

            return rpcFunctionMetadatas;
        }

        private static IEnumerable<FunctionInformation> IndexFunctionsInFile(FileInfo powerShellFile)
        {
            List<FunctionInformation> fileFunctions = new List<FunctionInformation>();
            var fileAst = Parser.ParseFile(powerShellFile.FullName, out _, out ParseError[] errors);
            if (errors.Any())
            {
                throw new Exception("Couldn't parse this file");
                // TODO: Probably don't throw here?
                //return fileFunctions;
            }
            if (powerShellFile.Extension == ".ps1") 
            {
                // parse only the file param block, return one RpcFunctionMetadata assuming the file is the entry point
                var paramAsts = fileAst.ParamBlock;
                if (paramAsts != null && paramAsts.Attributes.Where(x => x.TypeName.ToString() == "Function").Any()) 
                {
                    // This is a function, return it 
                    fileFunctions.Add(CreateRpcMetadataFromFile(powerShellFile.FullName));
                }
            }
            else if (powerShellFile.Extension == ".psm1")
            {
                var potentialFunctions = fileAst.FindAll(x => x is FunctionDefinitionAst, false);
                foreach (var potentialFunction in potentialFunctions)
                {
                    var matchingBlocks = potentialFunction.FindAll(x => x is ParamBlockAst && ((ParamBlockAst)x).Attributes.Where(z => z.TypeName.ToString() == "Function").Any(), true);
                    if (matchingBlocks.Any()) {
                        //This function is one we need to register
                        fileFunctions.Add(CreateRpcMetadataFromFunctionAst(powerShellFile.FullName, (FunctionDefinitionAst)potentialFunction));
                    }
                }
                // parse all function definitions, return as many RpcFunctionMetadatas as exist in the file
            }
            return fileFunctions;
        }

        private static FunctionInformation CreateRpcMetadataFromFile(string powerShellFile)
        {
            var fileAst = Parser.ParseFile(powerShellFile, out _, out ParseError[] errors);

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

            var functionAttribute = paramBlock.Attributes.Where(x => x.TypeName.Name == "Function" && x.PositionalArguments.Count > 0);
            if (functionAttribute.Any() && functionAttribute.First().PositionalArguments[0].GetType() == typeof(StringConstantExpressionAst))
            {
                thisFunction.Name = ((StringConstantExpressionAst)functionAttribute.First().PositionalArguments[0]).Value;
            }

            //Input bindings first
            //thisFunction.Bindings.AddRange(GetInputBindingInfo(paramBlock));
            List<BindingInformation> inputBindings = GetInputBindingInfo(paramBlock);

            // Then parse output bindings
            List<BindingInformation> outputBindings = GetOutputBindingInfo(paramBlock.Attributes);
            List<BindingInformation> missingBindings = GetMissingOutputBindings(inputBindings, outputBindings);
            thisFunction.Bindings.AddRange(inputBindings);
            thisFunction.Bindings.AddRange(outputBindings);
            thisFunction.Bindings.AddRange(missingBindings);
        }

        private static List<BindingInformation> GetMissingOutputBindings(List<BindingInformation> inputBindings, List<BindingInformation> outputBindings)
        {
            List<BindingInformation> missingBindings = new List<BindingInformation>();
            if (inputBindings.Where(x => x.Type == "httpTrigger").Count() > 0 && outputBindings.Where(x => x.Type == "httpOutput").Count() == 0)
            {
                missingBindings.Add(HttpOutputBinding.Create());
            }
            return missingBindings;
        }

        private static List<BindingInformation> GetInputBindingInfo(ParamBlockAst paramBlock)
        {
            List<BindingInformation> outputBindingInfo = new List<BindingInformation>();
            foreach (ParameterAst parameter in paramBlock.Parameters)
            {
                foreach (AttributeAst attribute in parameter.Attributes)
                {
                    BindingInformation? bindingInfo = BindingExtractor.extractInputBinding(attribute, parameter);
                    if (bindingInfo is not null)
                    {
                        outputBindingInfo.Add(bindingInfo);
                    }
                }
            }
            return outputBindingInfo;
        }

        public static string? GetPositionalArgumentStringValue(AttributeAst attribute, int attributeIndex, string? defaultValue=null)
        {
            return attribute.PositionalArguments.Count > attributeIndex 
                   && attribute.PositionalArguments[attributeIndex].GetType() == typeof(StringConstantExpressionAst)
                ? ((StringConstantExpressionAst)attribute.PositionalArguments[attributeIndex]).Value : defaultValue;
        }

        private static List<BindingInformation> GetOutputBindingInfo(ReadOnlyCollection<AttributeAst> attributes)
        {
            List<BindingInformation> outputBindingInfo = new List<BindingInformation>();
            foreach (AttributeAst attribute in attributes)
            {
                BindingInformation? bindingInformation = BindingExtractor.extractOutputBinding(attribute);
                if (bindingInformation is not null)
                {
                    outputBindingInfo.Add(bindingInformation);
                }
            }
            return outputBindingInfo;
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
                var arrayValues = ((ArrayExpressionAst)expressionAst).FindAll(x => x is StringConstantExpressionAst, false);
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
