using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Models;
using CourseApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseApi.Controllers;
[Route("api/enrollments")]
[ApiController]
//[Authorize]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IMapper _mapper;
    public EnrollmentController(IEnrollmentService enrollmentService, IMapper mapper)
    {
        _enrollmentService = enrollmentService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEnrollmentsAsync()
    {
        var enrollments = await _enrollmentService.GetAllEnrollmentsAsync();
        return Ok(new SuccessResponse(200, "Get list enrollments successfully", _mapper.Map<List<EnrollmentDto>>(enrollments)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEnrollmentByIdAsync(int id)
    {
        var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
        if (enrollment == null)
        {
            return NotFound(new ErrorResponse(404, "Enrollment not found", null));
        }
        return Ok(new SuccessResponse(200, "Get enrollment successfully", _mapper.Map<EnrollmentDto>(enrollment)));
    }

    [HttpGet]
    [Route("courses/{courseId}")]
    public async Task<IActionResult> GetEnrollmentsByCourseIdAsync(string courseId)
    {
        var enrollments = await _enrollmentService.GetEnrollmentsByCourseIdAsync(courseId);
        return Ok(new SuccessResponse(200, "Get enrollments successfully", _mapper.Map<List<EnrollmentDto>>(enrollments)));
    }

    [HttpGet]
    [Route("students/{studentId}")]
    public async Task<IActionResult> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        var enrollments = await _enrollmentService.GetEnrollmentsByStudentIdAsync(studentId);
        return Ok(new SuccessResponse(200, "Get enrollments successfully", _mapper.Map<List<EnrollmentDto>>(enrollments)));
    }


    [HttpPost]
    public async Task<IActionResult> CreateEnrollmentAsync([FromBody] CreateEnrollmentDto cretateEnrollmentDto)
    {
        var enrollment = await _enrollmentService.EnrollStudentInCourseAsync(cretateEnrollmentDto);
        if (enrollment == null)
        {
            return BadRequest(new ErrorResponse(400, "Enrollment failed", null));
        }
        return Ok(new SuccessResponse(201, "Create enrollment successfully", _mapper.Map<EnrollmentDto>(enrollment)));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnrollmentAsync(int id)
    {
        var deletedEnrollment = await _enrollmentService.DeleteEnrollmentAsync(id);
        if (deletedEnrollment == false)
        {
            return NotFound(new ErrorResponse(404, "Enrollment not found", null));
        }
        return Ok(new SuccessResponse(200, "Delete enrollment successfully", null));
    }

}
