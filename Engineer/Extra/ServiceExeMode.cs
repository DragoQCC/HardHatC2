using System.ServiceProcess;
using System.Threading.Tasks;

namespace Engineer.Extra;

public class ServiceExeMode :  ServiceBase  
{

    public static void Main() 
    {
        ServiceBase.Run(new ServiceExeMode());
    }
    
    public ServiceExeMode()
    {
        this.ServiceName = "";
        this.CanStop = true;
        this.CanPauseAndContinue = true;
        this.AutoLog = false;
    }

    protected override void OnStart(string[] args)
    {
        Task.Run(async () => await Program.Main(new string[]{}));
    }
    
    protected override void OnStop()
    {
        
    }
    
}