using System.Text.Json;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using Serilog;

namespace CourseApi.Controllers
{
    [Route("api/courses")]
    [ApiController]
    [EnableCors("AllowCors")]

    //[Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly HybridCache _cache;

        public CourseController(ICourseService courseService, HybridCache cache)
        {
            _courseService = courseService;
            _cache = cache;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCoursesAsync([FromQuery] CourseQuery query, CancellationToken token = default)
        {
            var cacheKey = $"courses_{query.GetHashCode()}";
            var cachedResult = await _cache.GetOrCreateAsync(
                cacheKey,
                async (cancellationToken) =>
                {
                    var result = await _courseService.GetCoursesAsync(query);
                    return JsonSerializer.Serialize(result);
                },
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
                null,
                token);
            var result = JsonSerializer.Deserialize<ServiceResult<List<CourseDto>>>(cachedResult);
            return this.ToActionResult(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseByIdAsync(string id, CancellationToken token = default)
        {
            var cacheKey = $"course_{id}";
            var cachedResult = await _cache.GetOrCreateAsync(
                cacheKey,
                async (cancellationToken) =>
                {
                    var result = await _courseService.GetCourseByCourseIdAsync(id);
                    return JsonSerializer.Serialize(result);
                },
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
                null,
                token);
            var result = JsonSerializer.Deserialize<ServiceResult<CourseDto>>(cachedResult);
            return this.ToActionResult(result);
        }

        [HttpGet("{id}/students")]
        public async Task<IActionResult> GetStudentsInCourseAsync(string id, CancellationToken token = default)
        {
            var cacheKey = $"students_{id}";
            var cachedResult = await _cache.GetOrCreateAsync(
                cacheKey,
                async (cancellationToken) =>
                {
                    var result = await _courseService.GetStudentsByCourseIdAsync(id);
                    return JsonSerializer.Serialize(result);
                },
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
                null,
                token);
            var result = JsonSerializer.Deserialize<ServiceResult<List<StudentDto>>>(cachedResult);
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
