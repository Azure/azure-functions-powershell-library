//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System.Management.Automation.Language;

namespace AzureFunctions.PowerShell.SDK.BundledBindings
{
    internal class HttpOutputBinding : IOutputBinding
    {
        public override string BindingAttributeName => "HttpOutput";

        public override string BindingType => "http";

        public override BindingInformation ExtractBinding(AttributeAst attribute)
        {
            string outputBindingName = attribute.PositionalArguments.Count > 0 &&
                                       attribute.PositionalArguments[0].GetType() == typeof(StringConstantExpressionAst) ?
                                               ((StringConstantExpressionAst)attribute.PositionalArguments[0]).Value : "Response";
            return Create(outputBindingName);
        }

        public static BindingInformation Create(string name = "Response")
        {
            BindingInformation defaultOutputInfo = new BindingInformation();
            defaultOutputInfo.Type = "http";
            defaultOutputInfo.Direction = (int)BindingInformation.Directions.Out;
            defaultOutputInfo.Name = name;
            return defaultOutputInfo;
        }
    }
}
