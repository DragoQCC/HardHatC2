
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
