//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class HttpTriggerBinding : IInputBinding
    {
        public HttpTriggerBinding()
        {
            defaultOutputBindings.Add(HttpOutputBinding.Create());
        }

        public override string BindingAttributeName => Constants.AttributeNames.HttpTrigger;

        public override string BindingType => Constants.BindingNames.HttpTrigger;

        public override BindingInformation? ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation
            {
                Name = parameter.Name.VariablePath.UserPath
            };
            
            string bindingAuthLevel = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, "AuthLevel", Constants.DefaultHttpAuthLevel);
            object bindingMethods = WorkerIndexingHelper.GetNamedArgumentDefaultTypeValue(attribute, "Methods", Constants.DefaultHttpMethods);
            string route = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, "Route");

            bindingInformation.Direction = BindingDirection;
            bindingInformation.Type = BindingType;

            bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.AuthLevel, bindingAuthLevel);
            bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.Methods, bindingMethods);
            if (!string.IsNullOrWhiteSpace(route))
            {
                bindingInformation.otherInformation.Add(Constants.JsonPropertyNames.Route, route);
            }

            return bindingInformation;
        }
        
        public override bool ShouldUseDefaultOutputBindings(List<BindingInformation> existingOutputBindings)
        {
            IEnumerable<BindingInformation> httpOutputBindings = existingOutputBindings.Where(x => x.Type == Constants.BindingNames.Http && 
                                                                                              x.Direction == BindingInformation.Directions.Out);
            return !httpOutputBindings.Any();
        }
    }
}
