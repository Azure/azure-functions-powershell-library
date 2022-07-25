using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsSDK.BundledBindings
{
    public abstract class IOutputBinding : IBinding
    {
        public const BindingInformation.Directions BindingDirection = BindingInformation.Directions.Out;
        public abstract BindingInformation ExtractBinding(AttributeAst attribute);
    }
}
