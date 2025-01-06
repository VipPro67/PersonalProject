using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Models;
using CourseApi.Repositories;
using Newtonsoft.Json;
using Grpc.Net.Client;
using Serilog;
using CourseApi.Protos;
using Grpc.Core;

namespace CourseApi.Services;

public interface IEnrollmentService
{
    Task<ServiceResult<List<EnrollmentDto>>> GetAllEnrollmentsAsync(EnrollmentQuery query);
    Task<ServiceResult<EnrollmentDto>> GetEnrollmentByIdAsync(int enrollmentId);
    Task<ServiceResult<List<EnrollmentDto>>> GetEnrollmentsByCourseIdAsync(string courseId);
    Task<ServiceResult<List<EnrollmentDto>>> GetEnrollmentsByStudentIdAsync(int studentId);
    Task<ServiceResult<EnrollmentDto>> EnrollStudentInCourseAsync(CreateEnrollmentDto createEnrollmentDto);
    Task<ServiceResult<bool>> DeleteEnrollmentAsync(int enrollmentId);
}
public class EnrollmentService : IEnrollmentService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;
    private readonly StudentService.StudentServiceClient _studentServiceClient;

    public EnrollmentService(
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository,
        IMapper mapper,
        StudentService.StudentServiceClient studentServiceClient)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _mapper = mapper;
        _studentServiceClient = studentServiceClient;
    }
    public async Task<ServiceResult<EnrollmentDto>> GetEnrollmentByIdAsync(int enrollmentId)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(enrollmentId);
        if (enrollment == null)
        {
            Log.Error("No enrollments found");
            return new ServiceResult<EnrollmentDto>(ResultType.NotFound, "No enrollments found");
        }
        try
        {
            if (enrollment.StudentId.HasValue)
            {
                var request = new GetStudentByIdRequest { StudentId = enrollment.StudentId.Value };
                var response = await _studentServiceClient.GetStudentByIdAsync(request);

                if (response != null)
                {
                    enrollment.Student = new Student
                    {
                        StudentId = response.StudentId,
                        FullName = response.Name,
                        Email = response.Email,
                        PhoneNumber = response.PhoneNumber
                    };
                }
            }
            return new ServiceResult<EnrollmentDto>(_mapper.Map<EnrollmentDto>(enrollment), "Get enrollment by id successfully");
        }
        catch (RpcException e)
        {
            Log.Error(e, "gRPC error retrieving students from StudentApi");
            return new ServiceResult<EnrollmentDto>(ResultType.InternalServerError, $"gRPC error retrieving students from StudentApi: {e.Status.Detail}");
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving student from StudentApi: {e.Message}");
            return new ServiceResult<EnrollmentDto>(ResultType.InternalServerError, "Error retrieving student from StudentApi");
        }
    }


    public async Task<ServiceResult<List<EnrollmentDto>>> GetEnrollmentsByCourseIdAsync(string courseId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(courseId);
        if (enrollments == null || enrollments.Count == 0)
        {
            Log.Error("No enrollments found");
            return new ServiceResult<List<EnrollmentDto>>(ResultType.NotFound, "No enrollments found");
        }
        var ids = enrollments.Select(e => e.StudentId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();
        try
        {
            var request = new GetStudentsByIdsRequest();
            request.StudentIds.AddRange(ids);
            var response = await _studentServiceClient.GetStudentsByIdsAsync(request);

            if (response != null && response.Students.Any())
            {
                foreach (var enrollment in enrollments)
                {
                    var student = response.Students.FirstOrDefault(s => s.StudentId == enrollment.StudentId);
                    if (student != null)
                    {
                        enrollment.Student = new Student
                        {
                            StudentId = student.StudentId,
                            FullName = student.Name,
                            Email = student.Email,
                            PhoneNumber = student.PhoneNumber
                        };
                    }
                }
            }
            return new ServiceResult<List<EnrollmentDto>>(_mapper.Map<List<EnrollmentDto>>(enrollments), "Get all enrollments successfully");
        }

        catch (Exception e)
        {
            Log.Error(e, "Error retrieving students from StudentApi");
            return new ServiceResult<List<EnrollmentDto>>(ResultType.InternalServerError, $"Error retrieving students from StudentApi: {e.Message}");
        }
    }
    public async Task<ServiceResult<List<EnrollmentDto>>> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(studentId);
        if (enrollments == null || enrollments.Count == 0)
        {
            Log.Error("No enrollments found");
            return new ServiceResult<List<EnrollmentDto>>(ResultType.NotFound, "No enrollments found");
        }
        try
        {
            var request = new GetStudentByIdRequest { StudentId = studentId };
            var response = await _studentServiceClient.GetStudentByIdAsync(request);
            if (response != null)
            {
                foreach (var enrollment in enrollments)
                {
                    enrollment.Student = new Student
                    {
                        StudentId = response.StudentId,
                        FullName = response.Name,
                        Email = response.Email,
                        PhoneNumber = response.PhoneNumber
                    };
                }
            }
            return new ServiceResult<List<EnrollmentDto>>(_mapper.Map<List<EnrollmentDto>>(enrollments), "Get all enrollments successfully");
        }

        catch (Exception e)
        {
            Log.Error(e, "Error retrieving students from StudentApi");
            return new ServiceResult<List<EnrollmentDto>>(ResultType.InternalServerError, $"Error retrieving students from StudentApi: {e.Message}");
        }
    }

    public async Task<ServiceResult<EnrollmentDto>> EnrollStudentInCourseAsync(CreateEnrollmentDto createEnrollmentDto)
    {
        var course = await _courseRepository.GetCourseByCourseIdAsync(createEnrollmentDto.CourseId);
        if (course == null)
        {
            Log.Error($"Course with id {createEnrollmentDto.CourseId} not found");
            return new ServiceResult<EnrollmentDto>(ResultType.NotFound, "Course not found");
        }
        if (_enrollmentRepository.IsStudentEnrolledInCourseAsync(createEnrollmentDto.StudentId, createEnrollmentDto.CourseId).Result)
        {
            Log.Error("Student already enrolled in course");
            return new ServiceResult<EnrollmentDto>(ResultType.BadRequest, "Student already enrolled in course");
        }
        try
        {
            var request = new GetStudentByIdRequest { StudentId = createEnrollmentDto.StudentId };
            var response = await _studentServiceClient.GetStudentByIdAsync(request);
            if (response == null)
            {
                Log.Error($"Student with id {createEnrollmentDto.StudentId} not found");
                return new ServiceResult<EnrollmentDto>(ResultType.NotFound, "Student not found");
            }
            var enrollment = _mapper.Map<Enrollment>(createEnrollmentDto);
            var result = await _enrollmentRepository.CreateEnrollmentAsync(enrollment);
            if (result == null)
            {
                Log.Error("Failed to enroll student in course");
                return new ServiceResult<EnrollmentDto>(ResultType.InternalServerError, "Failed to enroll student in course");
            }
            return new ServiceResult<EnrollmentDto>(_mapper.Map<EnrollmentDto>(result), "Enroll student in course successfully");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error retrieving students from StudentApi");
            return new ServiceResult<EnrollmentDto>(ResultType.InternalServerError, $"Error retrieving students from StudentApi");
        }

    }
    public async Task<ServiceResult<bool>> DeleteEnrollmentAsync(int enrollmentId)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(enrollmentId);
        if (enrollment == null)
        {
            Log.Error($"Enrollment with id {enrollmentId} not found");
            return new ServiceResult<bool>(ResultType.NotFound, "Enrollment not found");
        }
        var result = await _enrollmentRepository.DeleteEnrollmentAsync(enrollment);
        if (!result)
        {
            Log.Error("Failed to delete enrollment");
            return new ServiceResult<bool>(ResultType.InternalServerError, "Failed to delete enrollment");
        }
        return new ServiceResult<bool>(true, "Delete enrollment successfully");
    }

    public async Task<ServiceResult<List<EnrollmentDto>>> GetAllEnrollmentsAsync(EnrollmentQuery query)
    {
        var enrollments = await _enrollmentRepository.GetAllEnrollmentsAsync(query);
        if (enrollments == null || enrollments.Count == 0)
        {
            Log.Warning("No enrollments found");
            return new ServiceResult<List<EnrollmentDto>>(ResultType.NotFound, "No enrollments found");
        }

        var ids = enrollments.Select(e => e.StudentId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();
        try
        {
            var request = new GetStudentsByIdsRequest();
            request.StudentIds.AddRange(ids);
            var response = await _studentServiceClient.GetStudentsByIdsAsync(request);
            {
                foreach (var enrollment in enrollments)
                {
                    var student = response.Students.FirstOrDefault(s => s.StudentId == enrollment.StudentId);
                    if (student != null)
                    {
                        enrollment.Student = new Student
                        {
                            StudentId = student.StudentId,
                            FullName = student.Name,
                            Email = student.Email,
                            PhoneNumber = student.PhoneNumber
                        };
                    }
                }
            }

            int totalItems = await _enrollmentRepository.GetTotalEnrollmentsAsync(query);
            var pagination = new Pagination
            {
                TotalItems = totalItems,
                CurrentPage = query.Page.Value,
                TotalPage = (int)Math.Ceiling(totalItems / (double)query.ItemsPerPage),
                ItemsPerPage = query.ItemsPerPage.Value
            };

            return new ServiceResult<List<EnrollmentDto>>(_mapper.Map<List<EnrollmentDto>>(enrollments), "Get all enrollments successfully", pagination);
        }

        catch (Exception e)
        {
            Log.Error(e, "Error retrieving students from StudentApi");
            return new ServiceResult<List<EnrollmentDto>>(ResultType.InternalServerError, $"Error retrieving students from StudentApi: {e.Message}");
        }


    }
}