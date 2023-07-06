using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEngLoading
{
    public class ModuleAttribute : System.Attribute
    {
        public string Name { get; private set; }

        public ModuleAttribute(string Name)
        {
            this.Name = Name;
        }
    }
}
