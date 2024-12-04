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
        private readonly IMapper _mapper;

        public StudentController(IStudentService studentService, IMapper mapper)
        {
            _studentService = studentService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudentsAsync()
        {
            var students = await _studentService.GetStudentsAsync();
            return Ok(new SuccessResponse(200, "Get list students successfully", _mapper.Map<List<StudentDto>>(students)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentByIdAsync(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound(
                    new ErrorResponse(404, "Student not found", null)
                );
            }
            return Ok(new SuccessResponse(200, "Get student successfully", _mapper.Map<StudentDto>(student)));
        }

        [HttpGet("ids")]
        public async Task<IActionResult> GetStudentsByIdsAsync([FromQuery] List<int> ids)
        {
            var students = await _studentService.GetStudentsByIdsAsync(ids);
            if (students == null)
            {
                return NotFound(
                    new ErrorResponse(404, "Students not found", null)
                );
            }
            return Ok(new SuccessResponse(200, "Get list students successfully", _mapper.Map<List<StudentDto>>(students)));
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentAsync([FromBody] CreateStudentDto createStudentDto)
        {
            var result = await _studentService.CreateStudentAsync(createStudentDto);
            if (result == null)
            {
                return BadRequest(
                    new ErrorResponse(400, "Failed to create student", null)
                );
            }
            return Ok(new SuccessResponse(200, "Create student successfully", _mapper.Map<StudentDto>(result)));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudentAsync(int id, [FromBody] UpdateStudentDto updateStudentDto)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound(
                    new ErrorResponse(404, "Student not found", null)
                );
            }
            var result = await _studentService.UpdateStudentAsync(id, updateStudentDto);
            if (result == null)
            {
                return BadRequest(
                    new ErrorResponse(400, "Failed to update student", null)
                );
            }
            return Ok(new SuccessResponse(200, "Update student successfully", _mapper.Map<StudentDto>(student)));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudentAsync(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound(
                    new ErrorResponse(404, "Student not found", null)
                );
            }
            var result = await _studentService.DeleteStudentAsync(id);
            if (result == false)
            {
                return BadRequest(
                    new ErrorResponse(400, "Failed to delete student", null)
                );
            }
            return Ok(new SuccessResponse(200, "Delete student successfully", null));
        }
    }
}
