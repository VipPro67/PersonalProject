﻿using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Models;
using CourseApi.Protos;
using CourseApi.Repositories;
using Ganss.Xss;
using Grpc.Core;
using Grpc.Net.Client;
using Newtonsoft.Json;
using Serilog;

namespace CourseApi.Services;
public interface ICourseService
{
    Task<ServiceResult<List<CourseDto>>> GetCoursesAsync(CourseQuery query);
    Task<ServiceResult<CourseDto>> GetCourseByCourseIdAsync(string courseId);
    Task<ServiceResult<CourseDto>> CreateCourseAsync(CreateCourseDto createCourseDto);
    Task<ServiceResult<CourseDto>> UpdateCourseAsync(string courseId, UpdateCourseDto updateCourseDto);
    Task<ServiceResult<List<StudentDto>>> GetStudentsByCourseIdAsync(string courseId);
    Task<ServiceResult<bool>> DeleteCourseAsync(string courseId);
}
public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;
    private readonly StudentService.StudentServiceClient _studentServiceClient;

    public CourseService(ICourseRepository courseRepository, IEnrollmentRepository enrollmentRepository, IMapper mapper, StudentService.StudentServiceClient studentServiceClient)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _studentServiceClient = studentServiceClient;
        _mapper = mapper;
    }

    public async Task<ServiceResult<List<CourseDto>>> GetCoursesAsync(CourseQuery query)
    {
        var courses = await _courseRepository.GetAllCoursesAsync(query);
        if (courses == null || courses.Count == 0)
        {
            Log.Error("No courses found");
            return new ServiceResult<List<CourseDto>>(ResultType.NotFound, "No courses found");
        }
        int totalItems = await _courseRepository.GetTotalCoursesAsync(query);
        var pagination = new Pagination
        {
            TotalItems = totalItems,
            CurrentPage = query.Page.Value,
            TotalPage = (int)Math.Ceiling(totalItems / (double)query.ItemsPerPage),
            ItemsPerPage = query.ItemsPerPage.Value
        };

        return new ServiceResult<List<CourseDto>>(_mapper.Map<List<CourseDto>>(courses), "Get list course successfully", pagination);
    }

    public async Task<ServiceResult<CourseDto>> GetCourseByCourseIdAsync(string courseId)
    {
        var course = await _courseRepository.GetCourseByCourseIdAsync(courseId);
        if (course == null)
        {
            Log.Error("Course not found for id: {Id}", courseId);
            return new ServiceResult<CourseDto>(ResultType.NotFound, "Course not found");
        }
        return new ServiceResult<CourseDto>(_mapper.Map<CourseDto>(course), "Get course by id successfully");
    }

    public async Task<ServiceResult<CourseDto>> CreateCourseAsync(CreateCourseDto createCourseDto)
    {
        var course = _mapper.Map<Course>(createCourseDto);
        var existingCourse = await _courseRepository.GetCourseByCourseIdAsync(course.CourseId);
        if (existingCourse != null)
        {
            Log.Error("Course with id: {Id} already exists", course.CourseId);
            return new ServiceResult<CourseDto>(ResultType.BadRequest, "Course with id already exists");
        }
        var createdCourse = await _courseRepository.CreateCourseAsync(course);
        return new ServiceResult<CourseDto>(_mapper.Map<CourseDto>(createdCourse), "Create course successfully");
    }

    public async Task<ServiceResult<CourseDto>> UpdateCourseAsync(string courseId, UpdateCourseDto updateCourseDto)
    {
        var existingCourse = await _courseRepository.GetCourseByCourseIdAsync(courseId);
        if (existingCourse == null)
        {
            Log.Error("Course not found for id: {Id}", courseId);
            return new ServiceResult<CourseDto>(ResultType.NotFound, "Course not found");
        }
        _mapper.Map(updateCourseDto, existingCourse);
        var updatedCourse = await _courseRepository.EditCourseAsync(existingCourse);
        return new ServiceResult<CourseDto>(_mapper.Map<CourseDto>(updatedCourse), "Update course successfully");
    }

    public async Task<ServiceResult<bool>> DeleteCourseAsync(string courseId)
    {
        var course = await _courseRepository.GetCourseByCourseIdAsync(courseId);
        if (course == null)
        {
            Log.Error("Course not found for id: {Id}", courseId);
            return new ServiceResult<bool>(ResultType.NotFound, "Course not found");
        }
        var enrollments = _enrollmentRepository.GetEnrollmentsByCourseIdAsync(course.CourseId);
        if (enrollments != null && enrollments.Result.Count > 0)
        {
            Log.Error("Delete course failed!. Exist enrollment in this course");
            return new ServiceResult<bool>(ResultType.BadRequest, "Exist enrollment in this course");
        }
        var result = await _courseRepository.DeleteCourseAsync(course);
        return new ServiceResult<bool>(result, "Delete course successfully");
    }

    public async Task<ServiceResult<List<StudentDto>>> GetStudentsByCourseIdAsync(string courseId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(courseId);
        if (enrollments == null || enrollments.Count == 0)
        {
            Log.Error("No enrollment found for course id: {Id}", courseId);
            return new ServiceResult<List<StudentDto>>(ResultType.NotFound, "No enrollment found");
        }
        var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
        var ids = enrollments.Select(e => e.StudentId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();
        try
        {
            var request = new GetStudentsByIdsRequest();
            request.StudentIds.AddRange(ids);
            var response = await _studentServiceClient.GetStudentsByIdsAsync(request);
            if (response != null && response.Students.Any())
            {
                var students = response.Students.Select(s => new StudentDto
                {
                    StudentId = s.StudentId,
                    FullName = s.Name,
                    Email = s.Email,
                    PhoneNumber = s.PhoneNumber
                }).ToList();
                return new ServiceResult<List<StudentDto>>(students, "Get students by course id successfully");
            }
            return new ServiceResult<List<StudentDto>>(ResultType.NotFound, "No students found");
        }
        catch (RpcException e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return new ServiceResult<List<StudentDto>>(ResultType.InternalServerError, "Error retrieving students from StudentApi");
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return new ServiceResult<List<StudentDto>>(ResultType.InternalServerError, "Error retrieving students from StudentApi");
        }
    }
}
