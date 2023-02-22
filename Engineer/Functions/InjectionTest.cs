using System;
using System.Diagnostics;

namespace Engineer.Functions;

public class InjectionTest
{
    public static bool Injection_Test()
    {
        bool isInjected = false;
        //if the current appDomain name does not equal the process name then isInjected is true
        string appName = AppDomain.CurrentDomain.FriendlyName;
        //remove the .exe from the end of the appName
        appName = appName.Remove(appName.Length - 4);
        string processName = Process.GetCurrentProcess().ProcessName;
        if (appName != processName)
        {
            isInjected = true;
        }
        return isInjected;
    }
}