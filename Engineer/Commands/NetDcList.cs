using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using System.DirectoryServices.AccountManagement;
using Engineer.Functions;

namespace Engineer.Commands
{
    internal class NetDcList : EngineerCommand
    {
        public override string Name => "netdclist";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //enumeratin of the supplied domain or current domain for all domain contrrollers name and ip addresses
                var targetDomain = task.Arguments.TryGetValue("/domain", out string domainName) ? domainName : null;

                var username = task.Arguments.TryGetValue("/username", out string usernameName) ? usernameName : null;
                var password = task.Arguments.TryGetValue("/password", out string passwordName) ? passwordName : null;

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

                var searcher = new DirectorySearcher(domain);
                //filter on objectClass=computer and userAccountControl:1.2.840.113556.1.4.803:=8192 to make sure we are returning only domain controllers
                searcher.Filter = "(&(objectClass=computer)(userAccountControl:1.2.840.113556.1.4.803:=8192))";
                //searcher.PropertiesToLoad.Add("name");
                searcher.PropertiesToLoad.Add("dNSHostName");
                //return the ip address property
                //searcher.PropertiesToLoad.Add("ipAddress");
                //search the domain for all domain controllers
                var results = searcher.FindAll();
                //create a string builder to hold the results
                var sb = new StringBuilder();
                //loop through the results and add them to the string builder
                foreach (SearchResult result in results)
                {
                    //get the name of the domain controller
                    //var name = result.Properties["name"][0].ToString();
                    //get the ip address of the domain controller
                    //var ip = result.Properties["ipAddress"][0].ToString();
                    //get the dns name of the domain controller
                    var dns = result.Properties["dNSHostName"][0].ToString();
                    //add the results to the string builder
                    sb.AppendLine($"DNS: {dns}");
                }
                //return the results
                Tasking.FillTaskResults(sb.ToString(),task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Tasking.FillTaskResults(ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
