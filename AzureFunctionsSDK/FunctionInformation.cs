using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsSDK
{
    public class FunctionInformation
    {
        public string Directory { get; set; } = "";
        public string ScriptFile { get; set; } = "";
        public string Name { get; set; } = "";
        public string EntryPoint { get; set; } = "";
        public string FunctionId { get; set; } = "";
        public List<BindingInformation> Bindings { get; set; } = new List<BindingInformation>();
    }
}
