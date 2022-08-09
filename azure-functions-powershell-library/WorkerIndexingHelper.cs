//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctions.PowerShell.SDK;
using AzureFunctions.PowerShell.SDK.Common;
using Common;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShellWorker
{
    internal class WorkerIndexingHelper
    {
        private static FunctionInformation currentFunction = new FunctionInformation();
        private static List<BindingInformation> inputBindings = new List<BindingInformation>();
        private static List<BindingInformation> outputBindings = new List<BindingInformation>();

        private static List<ErrorRecord> errorRecords = new List<ErrorRecord>();

        private static void AddInputBinding(BindingInformation inputBinding)
        {
            if (inputBindings.Where(x => x.Name == inputBinding.Name).Count() == 0)
            {
                inputBindings.Add(inputBinding);
            }
            else
            {
                throw new Exception($"Multiple bindings with name {inputBinding.Name} in function {currentFunction.Name}");
            }
        }

        private static void AddOutputBinding(BindingInformation outputBinding)
        {
            if (outputBindings.Where(x => x.Name == outputBinding.Name).Count() == 0)
            {
                outputBindings.Add(outputBinding);
            }
            else
            {
                throw new Exception($"Multiple bindings with name {outputBinding.Name} in function {currentFunction.Name}");
            }
        }

        internal static List<FunctionInformation> IndexFunctions(string baseDir, out List<ErrorRecord> errors)
        {
            errorRecords = new List<ErrorRecord>();
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

            errors = errorRecords;
            return rpcFunctionMetadatas;
        }

        private static IEnumerable<FunctionInformation> IndexFunctionsInFile(FileInfo powerShellFile)
        {
            List<FunctionInformation> fileFunctions = new List<FunctionInformation>();
            ScriptBlockAst? fileAst = Parser.ParseFile(powerShellFile.FullName, out _, out ParseError[] errors);
            if (errors.Any())
            {
                errorRecords.Add(new ErrorRecord(new Exception($"Couldn't parse the file: {powerShellFile.FullName}"), "Failed to parse file", ErrorCategory.ParserError, powerShellFile));
                return fileFunctions;
            }
            if (string.Equals(powerShellFile.Extension, Constants.Ps1FileExtension, StringComparison.OrdinalIgnoreCase)) 
            {
                // parse only the file param block, return one RpcFunctionMetadata assuming the file is the entry point
                ParamBlockAst paramAsts = fileAst.ParamBlock;
                if (paramAsts != null && paramAsts.Attributes.Where(x => x.TypeName.ToString() == Constants.AttributeNames.Function).Any())
                {
                    // This is a function, return it 
                    FunctionInformation functionInformation = CreateFunctionInformationFromFile(powerShellFile.FullName);
                    AddFunctionIfNameUnique(functionInformation, fileFunctions);
                }
            }
            else if (string.Equals(powerShellFile.Extension, Constants.Psm1FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                // parse all function definitions, return as many RpcFunctionMetadatas as exist in the file
                IEnumerable<Ast> potentialFunctions = fileAst.FindAll(x => x is FunctionDefinitionAst, false);
                foreach (Ast potentialFunction in potentialFunctions)
                {
                    IEnumerable<Ast> matchingBlocks = potentialFunction.FindAll(x => x is ParamBlockAst && 
                    ((ParamBlockAst)x).Attributes.Where(z => z.TypeName.ToString() == Constants.AttributeNames.Function).Any(), true);
                    if (matchingBlocks.Any()) {
                        //This function is one we need to register
                        FunctionInformation functionInformation = CreateFunctionInformationFromFunctionAst(powerShellFile.FullName, (FunctionDefinitionAst)potentialFunction);
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

        private static FunctionInformation CreateFunctionInformationFromFile(string powerShellFile)
        {
            ScriptBlockAst? fileAst = Parser.ParseFile(powerShellFile, out _, out ParseError[] errors);

            FunctionInformation thisFunction = new FunctionInformation();
            currentFunction = thisFunction;
            thisFunction.Directory = new FileInfo(powerShellFile).Directory!.FullName;
            thisFunction.ScriptFile = powerShellFile;
            thisFunction.Name = Path.GetFileName(powerShellFile).Split('.').First();

            thisFunction.FunctionId = Guid.NewGuid().ToString();
            ExtractBindings(thisFunction, fileAst.ParamBlock);

            return thisFunction;
        }

        private static FunctionInformation CreateFunctionInformationFromFunctionAst(string powerShellFile, FunctionDefinitionAst potentialFunction)
        {
            FunctionInformation thisFunction = new FunctionInformation();
            currentFunction = thisFunction;

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

            inputBindings = new List<BindingInformation>();
            outputBindings = new List<BindingInformation>();

            GetInputBindingInfo(paramBlock);
            GetOutputBindingInfo(paramBlock.Attributes);
            List<BindingInformation> missingBindings = BindingExtractor.GetDefaultBindings(inputBindings, outputBindings);

            thisFunction.Bindings.AddRange(inputBindings);
            thisFunction.Bindings.AddRange(outputBindings);
            thisFunction.Bindings.AddRange(missingBindings);
        }

        private static void GetInputBindingInfo(ParamBlockAst paramBlock)
        {
            foreach (ParameterAst parameter in paramBlock.Parameters)
            {
                foreach (AttributeAst attribute in parameter.Attributes)
                {
                    BindingInformation? bindingInfo = BindingExtractor.ExtractInputBinding(attribute, parameter);
                    if (bindingInfo is not null)
                    {
                        AddInputBinding(bindingInfo);
                    }
                }
            }
        }

        private static void GetOutputBindingInfo(ReadOnlyCollection<AttributeAst> attributes)
        {
            foreach (AttributeAst attribute in attributes)
            {
                BindingInformation? bindingInformation = BindingExtractor.ExtractOutputBinding(attribute);
                if (bindingInformation is not null)
                {
                    AddOutputBinding(bindingInformation);
                }
            }
        }

        public static void AddBindingInformation(string bindingName, string name, object value)
        {
            IEnumerable<BindingInformation> matchingInputBindings = inputBindings.Where(x => x.Name == bindingName);
            if (matchingInputBindings.Count() == 1)
            {
                matchingInputBindings.First().otherInformation.Add(name, value);
                return;
            }

            IEnumerable<BindingInformation> matchingOutputBindings = outputBindings.Where(x => x.Name == bindingName);
            if (matchingOutputBindings.Count() == 1)
            {
                matchingOutputBindings.First().otherInformation.Add(name, value);
                return;
            }

            //Console.WriteLine("Failed to add info thing");
            errorRecords.Add(new ErrorRecord(new Exception($"Could not add additional information with name {name} to binding {bindingName} because no binding with this name was found"),
                "Failed to add binding information", ErrorCategory.ObjectNotFound, bindingName));
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
            else if (expressionAst.GetType() == typeof(ParenExpressionAst))
            {
                List<string> values = new List<string>();
                IEnumerable<Ast>? arrayValues = ((ParenExpressionAst)expressionAst).FindAll(x => x is StringConstantExpressionAst, false);
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
                FileInfo[] functionJsonFiles = folder.GetFiles(Constants.FunctionJson, SearchOption.TopDirectoryOnly);
                if (functionJsonFiles.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
