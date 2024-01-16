using HardHatCore.ContractorSystem.Contexts;
using Metalama.Framework.Aspects;

namespace HardHatCore.ContractorSystem.InvocationPoints
{
    public class FuncCallAspect : OverrideMethodAspect
    {
        //name that will be used to check the InvocationData for the corresponding method ex. OnManagerCreate
        private string ContextName { get; set; }

        public FuncCallAspect(string contextName)
        {
            ContextName = contextName;
        }

        public override dynamic? OverrideMethod()
        {
            var functionName = meta.Target.Method.Name;
            Console.WriteLine($"Hooked on {functionName} method start");
            try
            {
                // Use the dictionary to find and invoke the corresponding method
                if (InvocationData.InvocationStartItems.ContainsKey(ContextName))
                {

                }
                else
                {
                    // Handle the case where the method name is not found in the dictionary
                    Console.WriteLine($"No action found for method / Context: {ContextName}");
                }
                // Let the method do its own thing.
                //Anything before this point is executed before the method but after its invocation. 
                var result = meta.Proceed();
                Console.WriteLine($"Method{meta.Target.Method} returned result");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
            finally
            {
                Console.WriteLine($"Hooked on {functionName} method end");
                // Use the dictionary to find and invoke the corresponding method
                string endFunctionName = $"{ContextName}_End";
                if (InvocationData.InvocationEndItems.ContainsKey(endFunctionName))
                {

                }
                else
                {
                    // Handle the case where the method name is not found in the dictionary
                    Console.WriteLine($"No action found for method / Context: {endFunctionName}");
                }
            }
        }
    }
}
