//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctions.PowerShell.SDK;
using AzureFunctions.PowerShell.SDK.Common;
using Common;
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
                throw new DirectoryNotFoundException();
            }
            if (ContainsLegacyFunctions(Directory.CreateDirectory(baseDir)))
            {
                throw new Exception("This function app directory contains functions which rely on host indexing, " +
                    "please remove them or configure this app for host indexing");
            }
            List<FileInfo> powerShellFiles = GetPowerShellFiles(new DirectoryInfo(baseDir));
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
            }
            if (string.Equals(powerShellFile.Extension, Constants.Ps1FileExtension, StringComparison.OrdinalIgnoreCase)) 
            {
                // parse only the file param block, return one RpcFunctionMetadata assuming the file is the entry point
                ParamBlockAst paramAsts = fileAst.ParamBlock;
                if (paramAsts != null && paramAsts.Attributes.Where(x => x.TypeName.ToString() == Constants.AttributeNames.Function).Any())
                {
                    // This is a function, return it 
                    FunctionInformation functionInformation = CreateRpcMetadataFromFile(powerShellFile.FullName);
                    AddFunctionIfNameUnique(functionInformation, fileFunctions);
                }
            }
            else if (string.Equals(powerShellFile.Extension, Constants.Psm1FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                // parse all function definitions, return as many RpcFunctionMetadatas as exist in the file
                IEnumerable<Ast>? potentialFunctions = fileAst.FindAll(x => x is FunctionDefinitionAst, false);
                foreach (Ast potentialFunction in potentialFunctions)
                {
                    IEnumerable<Ast>? matchingBlocks = potentialFunction.FindAll(x => x is ParamBlockAst && 
                    ((ParamBlockAst)x).Attributes.Where(z => z.TypeName.ToString() == Constants.AttributeNames.Function).Any(), true);
                    if (matchingBlocks.Any()) {
                        //This function is one we need to register
                        FunctionInformation functionInformation = CreateRpcMetadataFromFunctionAst(powerShellFile.FullName, (FunctionDefinitionAst)potentialFunction);
                        AddFunctionIfNameUnique(functionInformation, fileFunctions);
                    }
                    // If there are no matching blocks, this is a helper function that will live in the file
                    // but shall not be indexed as an Azure Function
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

            IEnumerable<AttributeAst>? functionAttribute = paramBlock.Attributes.Where(x => x.TypeName.Name == Constants.AttributeNames.Function && x.PositionalArguments.Count > 0);
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

        /// <summary>
        /// This method takes an ExpressionAst which is the value passed into a parameter, and attempts to parse it to a List<string>.
        /// If the user passes a single string to the parameter, it returns a list with only that string
        /// If the user passes a PowerShell array expression, it returns a list will all of the elements of the array as strings
        /// eg. ('Get', 'Post') ~= new List<string>() { "Get", "Post" }
        /// </summary>
        /// <param name="expressionAst"></param>
        /// <returns>List of values represented in the ExpressionAst</returns>
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
            List<FileInfo> files = baseDir.GetFiles("*" + Constants.Ps1FileExtension, SearchOption.TopDirectoryOnly).ToList();
            files.AddRange(baseDir.GetFiles("*" + Constants.Psm1FileExtension, SearchOption.TopDirectoryOnly).ToList());
            if (depth > 0)
            {
                foreach (DirectoryInfo d in baseDir.GetDirectories())
                {
                    files.AddRange(GetPowerShellFiles(d, depth - 1));
                }
            }
            return files;
        }

        private static bool ContainsLegacyFunctions(DirectoryInfo baseDir)
        {
            List<DirectoryInfo> folders = baseDir.GetDirectories().ToList();
            foreach (DirectoryInfo folder in folders)
            {
                var functionJsonFiles = folder.GetFiles(Constants.FunctionJson, SearchOption.TopDirectoryOnly);
                if (functionJsonFiles.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
