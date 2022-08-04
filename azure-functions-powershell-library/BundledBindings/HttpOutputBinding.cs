//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using AzureFunctions.PowerShell.SDK.Common;
using Common;
using System.Management.Automation.Language;

namespace AzureFunctions.PowerShell.SDK.BundledBindings
{
    internal class HttpOutputBinding : IOutputBinding
    {
        public override string BindingAttributeName => Constants.AttributeNames.HttpOutput;

        public override string BindingType => Constants.BindingNames.Http;

        public override BindingInformation ExtractBinding(AttributeAst attribute)
        {
            string outputBindingName = attribute.PositionalArguments.Count > 0 &&
                                       attribute.PositionalArguments[0].GetType() == typeof(StringConstantExpressionAst) ?
                                               ((StringConstantExpressionAst)attribute.PositionalArguments[0]).Value : Constants.DefaultHttpResponseName;
            return Create(outputBindingName);
        }

        public static BindingInformation Create(string name = Constants.DefaultHttpResponseName)
        {
            BindingInformation defaultOutputInfo = new BindingInformation();
            defaultOutputInfo.Type = Constants.BindingNames.Http;
            defaultOutputInfo.Direction = BindingInformation.Directions.Out;
            defaultOutputInfo.Name = name;
            return defaultOutputInfo;
        }
    }
}
