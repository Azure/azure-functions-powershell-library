using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Management.Automation.Language;

namespace AzureFunctionsSDK.BundledBindings
{
    public abstract class IBinding
    {
        public IBinding() { } //This is necessary for Activator
        public abstract string BindingAttributeName { get; }
        public abstract string BindingType { get; }

        public List<BindingInformation> defaultOutputBindings = new List<BindingInformation>();

        public bool BindingMatches(AttributeAst attribute)
        {
            if (attribute.TypeName.Name == BindingAttributeName)
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

        public bool ShouldUseDefaultOutputBindings(List<BindingInformation> existingOutputBindings)
        {
            return false;
        }

    }
}
