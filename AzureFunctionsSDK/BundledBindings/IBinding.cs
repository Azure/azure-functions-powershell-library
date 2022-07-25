using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Management.Automation.Language;

namespace AzureFunctionsSDK.BundledBindings
{
    public abstract class IBinding
    {
        public IBinding() { } //This is necessary for Activator
        public abstract string BindingName { get; }

        public bool BindingMatches(AttributeAst attribute)
        {
            if (attribute.TypeName.Name == BindingName)
            {
                return true;
            }
            return false;
        }

        public void AddToExtractor()
        {
            if (!BindingExtractor.hasSupportedBinding(GetType()))
            {
                BindingExtractor.addSupportedBinding(this);
            }
        }
    }
}
