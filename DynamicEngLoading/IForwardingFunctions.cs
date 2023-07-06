using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static DynamicEngLoading.h_DynInv;

namespace DynamicEngLoading
{
    /// <summary>
    /// Holds the basic defenitions so the dynamic commands can call the same functions as the main engineer and still be able to be compiled on the teamserver without error
    /// </summary>
    public interface IForwardingFunctions
    {
        //interface def for Tasking.FillTaskResults(output, task, taskStatus, taskResponseType);
        public void FillTaskResults(object output, EngineerTask task, EngTaskStatus taskStatus, TaskResponseType taskResponseType);
        
    }

    /// <summary>
    /// Shared between the main engineer and new dynamic commands so they can call the same functions when the new commands are dynamically loaded
    /// </summary>
    public static class ForwardingFunctions
    {
        public static IForwardingFunctions ForwardingFunctionWrap { get; set; }
    }
}


