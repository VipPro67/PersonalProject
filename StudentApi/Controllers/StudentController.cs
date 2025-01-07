using System.Text.Json;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using StudentApi.DTOs;
using StudentApi.Helpers;
using StudentApi.Services;

namespace StudentApi.Controllers
{
    [Route("api/students")]
    [ApiController]
    [EnableCors("AllowCors")]

    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IHybridCacheWrapper _cache;

        public StudentController(IStudentService studentService, IHybridCacheWrapper cache)
        {
            _studentService = studentService;
            _cache = cache;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllStudentsAsync([FromQuery] StudentQuery query, CancellationToken token = default)
        {
            var rawCacheKey = JsonSerializer.Serialize(query);
            var cacheKey = $"enrollments_{GenerateHash.Hash(rawCacheKey)}";

            var needCache = Request.Headers.TryGetValue("Cache-Control", out var cacheControl);
            if (needCache && cacheControl.Contains("no-cache"))
            {
                await _cache.RemoveAsync(cacheKey);
            }
            var cachedResult = await _cache.GetOrCreateAsync(
                cacheKey,
                async (cancellationToken) =>
                {
                    var result = await _studentService.GetStudentsAsync(query);
                    return JsonSerializer.Serialize(result);
                },
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(3) },
                null,
                token);
            var result = JsonSerializer.Deserialize<ServiceResult<List<StudentDto>>>(cachedResult);
            return this.ToActionResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentByIdAsync(int id, CancellationToken token = default)
        {
            var cacheKey = $"student_{id}";
            var needCache = Request.Headers.TryGetValue("Cache-Control", out var cacheControl);
            if (needCache && cacheControl.Contains("no-cache"))
            {
                await _cache.RemoveAsync(cacheKey);
            }
            var cachedResult = await _cache.GetOrCreateAsync(
                cacheKey,
                async (cancellationToken) =>
                {
                    var result = await _studentService.GetStudentByIdAsync(id);
                    return JsonSerializer.Serialize(result);
                },
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(3) },
                null,
                token);
            var result = JsonSerializer.Deserialize<ServiceResult<StudentDto>>(cachedResult);
            return this.ToActionResult(result);
        }

        [HttpGet("ids")]
        public async Task<IActionResult> GetStudentsByIdsAsync([FromQuery] List<int> ids, CancellationToken token = default)
        {
            var cacheKey = $"students_{GenerateHash.Hash(JsonSerializer.Serialize(ids))}";
            var needCache = Request.Headers.TryGetValue("Cache-Control", out var cacheControl);
            if (needCache && cacheControl.Contains("no-cache"))
            {
                await _cache.RemoveAsync(cacheKey);
            }
            var cachedResult = await _cache.GetOrCreateAsync(
                cacheKey,
                async (cancellationToken) =>
                {
                    var result = await _studentService.GetStudentsByIdsAsync(ids);
                    return JsonSerializer.Serialize(result);
                },
                new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(3) },
                null,
                token);
            var result = JsonSerializer.Deserialize<ServiceResult<List<StudentDto>>>(cachedResult);
            return this.ToActionResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentAsync([FromBody] CreateStudentDto createStudentDto)
        {
            var result = await _studentService.CreateStudentAsync(createStudentDto);
            return this.ToActionResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudentAsync(int id, [FromBody] UpdateStudentDto updateStudentDto)
        {
            var result = await _studentService.UpdateStudentAsync(id, updateStudentDto);
            return this.ToActionResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudentAsync(int id)
        {
            var result = await _studentService.DeleteStudentAsync(id);
            return this.ToActionResult(result);
        }
    }
}
