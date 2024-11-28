using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CourseApi.Controllers
{
    [Route("api/courses")]
    [ApiController]
    [Authorize]
    public class CourseController : ControllerBase
    {

    }

}
