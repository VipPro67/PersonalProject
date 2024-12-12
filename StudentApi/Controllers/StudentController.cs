using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentApi.DTOs;
using StudentApi.Helpers;
using StudentApi.Services;

namespace StudentApi.Controllers
{
    [Route("api/students")]
    [ApiController]
    //[Authorize]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudentsAsync([FromQuery] StudentQuery query)
        {
            var result = await _studentService.GetStudentsAsync(query);
            return this.ToActionResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentByIdAsync(int id)
        {
            var result = await _studentService.GetStudentByIdAsync(id);
            return this.ToActionResult(result);
        }

        [HttpGet("ids")]
        public async Task<IActionResult> GetStudentsByIdsAsync([FromQuery] List<int> ids)
        {
            var result = await _studentService.GetStudentsByIdsAsync(ids);
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
