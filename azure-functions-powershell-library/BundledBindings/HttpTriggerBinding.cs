//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctions.PowerShell.SDK.Common;
using Common;
using Microsoft.Azure.Functions.PowerShellWorker;
using System.Management.Automation.Language;

namespace AzureFunctions.PowerShell.SDK.BundledBindings
{
    public class HttpTriggerBinding : IInputBinding
    {
        public HttpTriggerBinding()
        {
            defaultOutputBindings.Add(HttpOutputBinding.Create());
        }

        public override string BindingAttributeName => Constants.AttributeNames.HttpTrigger;

        public override string BindingType => Constants.BindingNames.HttpTrigger;

        public override BindingInformation ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation
            {
                Name = parameter.Name.VariablePath.UserPath
            };
            //Todo: Support for named arguments: https://github.com/Azure/azure-functions-powershell-library/issues/11
            string? bindingAuthLevel = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 0, Constants.DefaultHttpAuthLevel);
            List<string>? bindingMethods = attribute.PositionalArguments.Count > 1 ? 
                                                WorkerIndexingHelper.ExtractOneOrMore(attribute.PositionalArguments[1]) : 
                                                Constants.DefaultHttpMethods;
            string? route = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 2);
            if (bindingMethods == null)
            {
                bindingMethods = Constants.DefaultHttpMethods;
            }
            bindingInformation.Direction = BindingDirection;
            bindingInformation.Type = BindingType;
            if (bindingAuthLevel != null)
            {
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.AuthLevel, bindingAuthLevel);
            }
            if (bindingMethods != null)
            {
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.Methods, bindingMethods);
            }
            if (route != null)
            {
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.Route, route);
            }
            return bindingInformation;
        }
        
        public override bool ShouldUseDefaultOutputBindings(List<BindingInformation> existingOutputBindings)
        {
            var httpOutputBindings = existingOutputBindings.Where(x => x.Type == Constants.BindingNames.Http && 
                                                                       x.Direction == BindingInformation.Directions.Out);
            return !httpOutputBindings.Any();
        }
    }
}
