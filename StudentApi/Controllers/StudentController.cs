using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StudentApi.Controllers
{
    [Route("api/students")]
    [ApiController]
    [Authorize]
    public class StudentController : ControllerBase
    {
        
    }
}
