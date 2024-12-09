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
    public EnrollmentController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEnrollmentsAsync()
    {
        var result = await _enrollmentService.GetAllEnrollmentsAsync();
        return this.ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEnrollmentByIdAsync(int id)
    {
        var result = await _enrollmentService.GetEnrollmentByIdAsync(id);
        return this.ToActionResult(result);

    }

    [HttpGet]
    [Route("courses/{courseId}")]
    public async Task<IActionResult> GetEnrollmentsByCourseIdAsync(string courseId)
    {
        var result = await _enrollmentService.GetEnrollmentsByCourseIdAsync(courseId);
        return this.ToActionResult(result);
    }

    [HttpGet]
    [Route("students/{studentId}")]
    public async Task<IActionResult> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        var result = await _enrollmentService.GetEnrollmentsByStudentIdAsync(studentId);
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
