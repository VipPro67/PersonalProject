using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CourseApi.Controllers
{
    [Route("api/courses")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello Course There!");
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
