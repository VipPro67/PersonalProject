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

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCoursesAsync([FromQuery] CourseQuery query)
        {
            var result = await _courseService.GetCoursesAsync(query);
            return this.ToActionResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseByIdAsync(string id)
        {
            var result = await _courseService.GetCourseByCourseIdAsync(id);
            return this.ToActionResult(result);

        }

        [HttpGet("{id}/students")]
        public async Task<IActionResult> GetStudentsInCourseAsync(string id)
        {
            var result = await _courseService.GetStudentsByCourseIdAsync(id);
            return this.ToActionResult(result);

        }

        [HttpPost]
        public async Task<IActionResult> AddCourseAsync([FromBody] CreateCourseDto course)
        {
            var result = await _courseService.CreateCourseAsync(course);
            return this.ToActionResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourseAsync(string id, [FromBody] UpdateCourseDto course)
        {
            var result = await _courseService.UpdateCourseAsync(id, course);
            return this.ToActionResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseAsync(string id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            return this.ToActionResult(result);
        }
    }
}
