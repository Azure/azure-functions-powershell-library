//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System.Management.Automation.Language;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    public class GenericBinding : IInputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.GenericBinding;

        public override string BindingType => Constants.BindingNames.NOT_USED;

        public override BindingInformation? ExtractBinding(AttributeAst attribute, ParameterAst parameter)
        {
            BindingInformation bindingInformation = new BindingInformation();
            string? bindingType = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 0, Constants.DefaultHttpAuthLevel);
            string? bindingName = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 1, Constants.DefaultHttpAuthLevel);
            string? bindingDirection = WorkerIndexingHelper.GetPositionalArgumentStringValue(attribute, 2, Constants.DefaultHttpAuthLevel);

            List<string> problems = new List<string>();

            if (string.IsNullOrEmpty(bindingType))
            {
                problems.Add("Missing Type parameter");
            }
            if (string.IsNullOrEmpty(bindingName))
            {
                problems.Add("Missing Name parameter");
            }
            if (string.IsNullOrEmpty(bindingDirection))
            {
                problems.Add("Missing Direction parameter");
            }

            if (bindingDirection != null && Enum.GetNames(typeof(BindingInformation.Directions)).Select(x => x.ToLower()).Contains(bindingDirection.ToLower()))
            {
                foreach (BindingInformation.Directions direction in Enum.GetValues(typeof(BindingInformation.Directions)))
                {
                    if (Enum.GetName(direction)?.ToLower() == bindingDirection.ToLower())
                    {
                        bindingInformation.Direction = direction;
                        break;
                    }
                }
            }
            
            if (bindingInformation.Direction == BindingInformation.Directions.Unknown)
            {
                problems.Add("Must specify a valid binding direction");
            }

            if (problems.Count > 0)
            {
                throw new Exception("The following problems exist with GenericBinding: \n" + string.Join("\n", problems));
            }

            if (!string.IsNullOrEmpty(bindingType) && !string.IsNullOrEmpty(bindingName))
            {
                bindingInformation.Type = bindingType;
                bindingInformation.Name = bindingName;
            }

            //foreach (KeyValuePair<string, object> kvp in otherBindingInformation)
            //{
            //    bindingInformation.otherInformation.Add(kvp.Key, kvp.Value);
            //}

            return bindingInformation;
        }
    }
}
