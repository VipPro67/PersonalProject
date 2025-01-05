using System.Text.Json;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace CourseApi.Controllers;
[Route("api/enrollments")]
[ApiController]
[EnableCors("AllowCors")]

public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly HybridCache _cache;
    public EnrollmentController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEnrollmentsAsync([FromQuery] EnrollmentQuery query, CancellationToken token = default)
    {
        var cacheKey = $"enrollments_{query.GetHashCode()}";
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async (cancellationToken) =>
            {
                var result = await _enrollmentService.GetAllEnrollmentsAsync(query);
                return JsonSerializer.Serialize(result);
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
            null,
            token);
        var result = JsonSerializer.Deserialize<ServiceResult<List<EnrollmentDto>>>(cachedResult);
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEnrollmentByIdAsync(int id, CancellationToken token = default)
    {
        var cacheKey = $"enrollment_{id}";
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async (cancellationToken) =>
            {
                var result = await _enrollmentService.GetEnrollmentByIdAsync(id);
                return JsonSerializer.Serialize(result);
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
            null,
            token);
        var result = JsonSerializer.Deserialize<ServiceResult<EnrollmentDto>>(cachedResult);
        return this.ToActionResult(result);
    }

    [HttpGet]
    [Route("courses/{courseId}")]
    public async Task<IActionResult> GetEnrollmentsByCourseIdAsync(string courseId, CancellationToken token = default)
    {
        var cacheKey = $"enrollments_course_{courseId}";
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async (cancellationToken) =>
            {
                var result = await _enrollmentService.GetEnrollmentsByCourseIdAsync(courseId);
                return JsonSerializer.Serialize(result);
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
            null,
            token);
        var result = JsonSerializer.Deserialize<ServiceResult<List<EnrollmentDto>>>(cachedResult);
        return this.ToActionResult(result);
    }

    [HttpGet]
    [Route("students/{studentId}")]
    public async Task<IActionResult> GetEnrollmentsByStudentIdAsync(int studentId, CancellationToken token = default)
    {
        var cacheKey = $"enrollments_student_{studentId}";
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async (cancellationToken) =>
            {
                var result = await _enrollmentService.GetEnrollmentsByStudentIdAsync(studentId);
                return JsonSerializer.Serialize(result);
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
            null,
            token);
        var result = JsonSerializer.Deserialize<ServiceResult<List<EnrollmentDto>>>(cachedResult);
        return this.ToActionResult(result);
    }


    [HttpPost]
    public async Task<IActionResult> CreateEnrollmentAsync([FromBody] CreateEnrollmentDto cretateEnrollmentDto)
    {
        var result = await _enrollmentService.EnrollStudentInCourseAsync(cretateEnrollmentDto);
        return this.ToActionResult(result);

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnrollmentAsync(int id)
    {
        var result = await _enrollmentService.DeleteEnrollmentAsync(id);
        return this.ToActionResult(result);
    }

}
