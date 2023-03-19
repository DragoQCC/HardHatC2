// using System;
// using System.IO;
// using System.Management;
// using System.Threading.Tasks;
// using Engineer.Functions;
// using Engineer.Models;
//
// namespace Engineer.Commands;
//
// public class Ping : EngineerCommand
// {
//     public override string Name  => "ping";
//     public override Task Execute(EngineerTask task)
//     {
//         task.Arguments.TryGetValue("/address", out string address);
//         //check if address is null or empty 
//         if (String.IsNullOrEmpty(address))
//         {
//             Tasking.FillTaskResults("Address is null or empty", task,EngTaskStatus.FailedWithWarnings);
//         }
//         else
//         {
//             //ping the address
//             //use system management to upload and execute the binary on the target machine
//             ManagementScope scope = new ManagementScope($@"\\root\cimv2");
//             scope.Connect();
//             ManagementClass pingClass = new ManagementClass(scope, new ManagementPath("Win32_PingStatus"), null);
//             //set the filter to the address
//             ManagementBaseObject inParams = pingClass.Qualifiers
//
//             inParams["Address"] = address;
//             ManagementBaseObject outParams = processClass.InvokeMethod("Create", inParams, null);
//             if (pingReply.Status == IPStatus.Success)
//             {
//                 Tasking.FillTaskResults("Ping successful", task,EngTaskStatus.Complete);
//             }
//             else
//             {
//                 Tasking.FillTaskResults("Ping failed", task,EngTaskStatus.FailedWithWarnings);
//             }
//         }
//         
//     }
// }