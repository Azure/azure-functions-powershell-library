using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsSDK
{
    public class BindingInformation
    {
        public enum Directions
        {
            In = 0,
            Out = 1, 
            Inout = 2
        }

        public int Direction { get; set; } = -1;
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public Dictionary<string, Object> otherInformation { get; set; } = new Dictionary<string, Object>();
    }
}
