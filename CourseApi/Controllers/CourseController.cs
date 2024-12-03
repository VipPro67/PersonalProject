using System.Security.Claims;
using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Models;
using CourseApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CourseApi.Controllers
{
    [Route("api/courses")]
    [ApiController]
    [EnableCors]

    //[Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IMapper _mapper;

        public CourseController(ICourseService courseService, IMapper mapper)
        {
            _courseService = courseService;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCoursesAsync()
        {
            var courses = await _courseService.GetCoursesAsync();

            return Ok(new SuccessResponse(200, "Get list courses successfully", _mapper.Map<List<CourseDto>>(courses)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseByIdAsync(string id)
        {
            var course = await _courseService.GetCourseByCourseIdAsync(id);
            if (course == null)
            {
                return NotFound(
                    new ErrorResponse(404, "Course not found", null)
                );
            }
            return Ok(new SuccessResponse(200, "Get course successfully", _mapper.Map<CourseDto>(course)));
        }

        [HttpPost]
        public async Task<IActionResult> AddCourseAsync([FromBody] CreateCourseDto course)
        {
            var result = await _courseService.CreateCourseAsync(course);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse(500, "Failed to create course", null)
                );
            }
            return Ok(new SuccessResponse(200, "Get course successfully", _mapper.Map<CourseDto>(result)));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourseAsync(string id, [FromBody] UpdateCourseDto course)
        {
            Log.Information("Updating course with id: {Id}", id);
            var updatedCourse = await _courseService.UpdateCourseAsync(id, course);
            if (updatedCourse == null)
            {
                return NotFound(
                    new ErrorResponse(404, "Course not found", null)
                );
            }
            return Ok(new SuccessResponse(200, "Update course successfully", _mapper.Map<CourseDto>(updatedCourse)));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseAsync(string id)
        {
            var deletedCourse = await _courseService.DeleteCourseAsync(id);
            if (deletedCourse == false)
            {
                return NotFound(
                    new ErrorResponse(404, "Course not found", null)
                );
            }

            return Ok(new SuccessResponse(200, "Course deleted successfully", null));
        }
    }
}
