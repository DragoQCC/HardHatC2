using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HardHatCore.TeamServer.Controllers
{
    [Authorize(Roles = "Operator,TeamLead,Administrator")]
    [ApiController]
    [Route("[controller]")]
    public class ContractorSystemController : ControllerBase
    {
       
        
    }
}
