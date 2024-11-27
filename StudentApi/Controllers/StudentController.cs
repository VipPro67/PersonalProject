using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StudentApi.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var context = HttpContext;
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var UserName = User.FindFirstValue(ClaimTypes.Name);
            var Email = User.FindFirstValue(ClaimTypes.Email);
            return Ok($"Hello User {UserId}! {UserName} {Email} You are authorized to access this endpoint.STUDENT");
        }
    }
}
