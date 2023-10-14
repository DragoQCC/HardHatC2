using DynamicEngLoading;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class LdapWhoami : EngineerCommand
    {
        public override string Name => "ldapwhoami";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //check for arguments for /domain /username and /password
                string targetDomain = task.Arguments.TryGetValue("/domain", out string domainName) ? domainName : null;

                // if domain is null geet the current domain
                if (targetDomain == null)
                {
                    targetDomain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                }
                //create a directory entry for the target domain, if username and password are not null then use those as well
                DirectoryEntry domain;
                domain = new DirectoryEntry("LDAP://" + targetDomain);
                Console.WriteLine($"target domain is {targetDomain}");
                //send an extended request of LDAP_SERVER_WHO_AM_I_OID
                var whoami = new ExtendedRequest("1.3.6.1.4.1.4203.1.11.3");
                //create a directory connection for the domain
                Console.WriteLine($"domain path is {domain.Path}");
                Console.WriteLine($"trying to connect to {targetDomain}");
                var connection = new LdapConnection(targetDomain);
                //send the request to the domain
                var whoamiResponse = (ExtendedResponse)connection.SendRequest(whoami);
                //create a string builder to hold the results
                StringBuilder sb = new StringBuilder();
                //get the result and add it to the string builder
                sb.AppendLine(Encoding.UTF8.GetString(whoamiResponse.ResponseValue));
                //return the string builder
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(sb.ToString(), task, EngTaskStatus.Complete, TaskResponseType.String);

            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                // Console.WriteLine(ex.StackTrace);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message, task, EngTaskStatus.Failed, TaskResponseType.String);
            }

        }
    }
}
