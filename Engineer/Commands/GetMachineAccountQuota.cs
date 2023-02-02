using Engineer.Commands;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class GetMachineAccountQuota : EngineerCommand
    {
        public override string Name => "GetMachineAccountQuota";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //search the domain for the machine account quota value 
                var targetDomain = task.Arguments.TryGetValue("/domain", out string domainName) ? domainName : null;
                //get the task.Arguments values for username and password
                var username = task.Arguments.TryGetValue("/username", out string usernameName) ? usernameName : null;
                var password = task.Arguments.TryGetValue("/password", out string passwordName) ? passwordName : null;
            

                //if domain is null get the current domain
                //if domain is null get the current domain and find all the domain controllers
                if (targetDomain == null)
                {
                    targetDomain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                }
                Console.WriteLine($"target domain is {targetDomain}");
                //search the targetDomain for all domain controllers
                //if username and password are not null then use them to authenticate
                DirectoryEntry domain;
                if (username != null && password != null)
                {
                    Console.WriteLine($"username is {username}");
                    Console.WriteLine($"password is {password}");
                    domain = new DirectoryEntry("LDAP://" + targetDomain, username, password);
                }
                else
                {
                    domain = new DirectoryEntry("LDAP://" + targetDomain);
                }
                //get the machine account quota value
            
                //search the domain for the ms-DS-MachineAccountQuota object and return its value
                var searcher = new DirectorySearcher(domain);
                searcher.Filter = "(&(objectClass=domain))";
                searcher.PropertiesToLoad.Add("ms-DS-MachineAccountQuota");
                var result = searcher.FindOne();
                var machineAccountQuota = result.Properties["ms-DS-MachineAccountQuota"][0].ToString();
                //return the machine account quota value
                Tasking.FillTaskResults("[+] Machine Account Quota: " + machineAccountQuota,task,EngTaskStatus.Complete);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Tasking.FillTaskResults("[-] Error Getting Machine Account Quota: " + e.Message,task,EngTaskStatus.Failed);
            }


        }
    }
}
