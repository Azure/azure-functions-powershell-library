using Microsoft.Azure.Functions.PowerShell.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Functions.PowerShell.SDK.BundledBindings
{
    internal interface IAdditionalInformation
    {
        public void AddAdditionalInformation(AttributeAst attribute)
        {
            string bindingName = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, Constants.BindingPropertyNames.BindingName);
            string name = WorkerIndexingHelper.GetNamedArgumentStringValue(attribute, Constants.BindingPropertyNames.Name);
            object value = WorkerIndexingHelper.GetNamedArgumentDefaultTypeValue(attribute, Constants.BindingPropertyNames.Value, "");

            List<string> problems = new List<string>();

            if (string.IsNullOrWhiteSpace(bindingName))
            {
                problems.Add(AzPowerShellSdkStrings.MissingBindingName);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                problems.Add(AzPowerShellSdkStrings.MissingName);
            }

            if (value.GetType() == typeof(string) && string.IsNullOrWhiteSpace((string)value))
            {
                problems.Add(AzPowerShellSdkStrings.MissingValue);
            }

            if (problems.Count > 0)
            {
                throw new Exception(string.Format(AzPowerShellSdkStrings.AdditionalInformationProblemsExist, string.Join("\n", problems)));
            }

            //This condition will never be false but hey, type enforcement makes us add it anyway thanks logic
            if (!string.IsNullOrEmpty(bindingName) && !string.IsNullOrEmpty(name) && value != null)
            {
                WorkerIndexingHelper.AddBindingInformation(bindingName, name, value);
            }
        }
    }
}
