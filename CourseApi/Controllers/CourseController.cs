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
        [HttpGet]
        public IActionResult Get()
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var UserName = User.FindFirstValue(ClaimTypes.Name);
            var Email = User.FindFirstValue(ClaimTypes.Email);
            return Ok($"Hello User {UserId}! {UserName} {Email} You are authorized to access this endpoint.COURSE");
        }

        [HttpGet]
        [Route("ecec")]
        public IActionResult GetCourse()
        {
            return Ok(new
            {
                Status = 200,
                Message = "Course retrieved successfully",
                Data = new
                {
                    CourseId = 1,
                    Name = "ECEC"
                }
            });

        }

    }

}
