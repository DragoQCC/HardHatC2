using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ContractorSystem.Contexts
{
    public static class InvocationData
    {
        // Define a list to hold the mappings
        internal static Dictionary<string, InvocationItemProps> InvocationStartItems { get; set; } = new();

        internal static Dictionary<string, InvocationItemProps> InvocationEndItems { get; set; } = new();

        internal static List<string> DataChangeNames { get; set; } = new();

        internal static List<InvocationPointItem> InvocationItems { get; set; } = new();

        public static async Task FillInvocationPropertyInfo(List<InvocationPointItem> invocationPointItems)
        {
            foreach(var invocationPointItem in invocationPointItems)
            {
                InvocationItems.Add(invocationPointItem);
                if (invocationPointItem._ContextType is ContextType.Event)
                {
                    InvocationItemProps tempProp = new InvocationItemProps()
                    {
                        _propertyNames = invocationPointItem.MethodInfo?.GetParameters().Select(x => x.Name).ToArray(),
                        _propertyTypes = invocationPointItem.MethodInfo?.GetParameters().Select(x => x.ParameterType).ToArray()
                    };
                    InvocationStartItems.Add(invocationPointItem.Name, tempProp);

                    InvocationItemProps tempProp_end = new InvocationItemProps()
                    {
                        //for the end, we need to add the return type as the first item in the array
                        _propertyNames = new string[] { invocationPointItem.MethodInfo?.ReturnType.Name },
                        _propertyTypes = new Type[] { invocationPointItem.MethodInfo?.ReturnType }
                    };

                    string invocationEndName = invocationPointItem.Name + "_end";
                    InvocationEndItems.Add(invocationEndName, tempProp_end);
                }
                else if (invocationPointItem._ContextType is ContextType.DataChange)
                {
                    DataChangeNames.Add(invocationPointItem.Name);
                }
            }
        }
    }

    public class InvocationPointItem
    {
        public string Name { get; set; }
        public MethodInfo? MethodInfo { get; set; }
        public ContextType _ContextType { get; set; }
    }

    internal class InvocationItemProps
    {
        public string[]? _propertyNames { get; set; }
        public Type[]? _propertyTypes { get; set; }
    }
}
