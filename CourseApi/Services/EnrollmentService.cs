using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Models;
using CourseApi.Repositories;

namespace CourseApi.Services;

public interface IEnrollmentService
{
    Task<List<Enrollment>> GetAllEnrollmentsAsync();
    Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId);

    Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(string courseId);
    Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId);
    Task<Enrollment?> EnrollStudentInCourseAsync(CreateEnrollmentDto createEnrollmentDto);
    Task<bool> DropStudentFromCourseAsync(int enrollmentId);
}
public class EnrollmentService : IEnrollmentService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    private readonly IMapper _mapper;
    public EnrollmentService(ICourseRepository courseRepository, IEnrollmentRepository enrollmentRepository, IMapper mapper)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _mapper = mapper;
    }

    public async Task<List<Enrollment>> GetAllEnrollmentsAsync()
    {
        var enrollments = await _enrollmentRepository.GetAllEnrollmentsAsync();
        return enrollments;
    }
    public async Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(enrollmentId);
        return enrollment;
    }

    public async Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(string courseId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(courseId);
        return enrollments;
    }
    public async Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(studentId);
        return enrollments;
    }

    public async Task<Enrollment?> EnrollStudentInCourseAsync(CreateEnrollmentDto createEnrollmentDto)
    {
        var course = await _courseRepository.GetCourseByCourseIdAsync(createEnrollmentDto.CourseId);
        if (course == null)
        {
            return null;
        }
        var enrollment = _mapper.Map<Enrollment>(createEnrollmentDto);
        await _enrollmentRepository.CreateEnrollmentAsync(enrollment);
        return enrollment;
    }
    public async Task<bool> DropStudentFromCourseAsync(int enrollmentId)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(enrollmentId);
        if (enrollment == null)
        {
            return false;
        }
        await _enrollmentRepository.DeleteEnrollmentAsync(enrollment);
        return true;
    }



}
