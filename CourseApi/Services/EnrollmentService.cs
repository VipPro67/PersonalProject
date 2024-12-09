using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Models;
using CourseApi.Repositories;
using Newtonsoft.Json;
using Serilog;

namespace CourseApi.Services;

public interface IEnrollmentService
{
    Task<ServiceResult<List<EnrollmentDto>?>> GetAllEnrollmentsAsync();
    Task<ServiceResult<EnrollmentDto?>> GetEnrollmentByIdAsync(int enrollmentId);

    Task<ServiceResult<List<EnrollmentDto>?>> GetEnrollmentsByCourseIdAsync(string courseId);
    Task<ServiceResult<List<EnrollmentDto>?>> GetEnrollmentsByStudentIdAsync(int studentId);
    Task<ServiceResult<EnrollmentDto?>> EnrollStudentInCourseAsync(CreateEnrollmentDto createEnrollmentDto);
    Task<ServiceResult<bool>> DeleteEnrollmentAsync(int enrollmentId);
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
    public async Task<ServiceResult<List<EnrollmentDto>?>> GetAllEnrollmentsAsync()
    {
        var enrollments = await _enrollmentRepository.GetAllEnrollmentsAsync();
        if (enrollments == null || enrollments.Count == 0)
        {
            Log.Error("No enrollments found");
            return new ServiceResult<List<EnrollmentDto>?>(ResultType.NotFound, "No enrollments found");
        }
        var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
        var studentApiClient = new HttpClient { BaseAddress = new Uri(studentApiUrl) };
        var ids = enrollments.Select(e => e.StudentId).Distinct().ToList();
        try
        {
            var response = await studentApiClient.GetAsync($"api/students/ids?ids={string.Join("&ids=", ids)}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Student>>>(responseContent);
                if (apiResponse?.Data != null)
                {
                    enrollments.ForEach(e => e.Student = apiResponse.Data.FirstOrDefault(s => s.StudentId == e.StudentId));
                }
                return new ServiceResult<List<EnrollmentDto>?>(_mapper.Map<List<EnrollmentDto>>(enrollments), "Get all enrollments successfully");
            }
            else
            {
                Log.Error($"Failed to retrieve students from StudentApi: {response.StatusCode}");
                return new ServiceResult<List<EnrollmentDto>?>(ResultType.InternalServerError, "Error retrieving students from StudentApi");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return new ServiceResult<List<EnrollmentDto>?>(ResultType.InternalServerError, "Error retrieving students from StudentApi");
        }
    }
    public async Task<ServiceResult<EnrollmentDto?>> GetEnrollmentByIdAsync(int enrollmentId)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(enrollmentId);
        if (enrollment == null)
        {
            Log.Error("No enrollments found");
            return new ServiceResult<EnrollmentDto?>(ResultType.NotFound, "No enrollments found");
        }
        try
        {
            var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
            var studentApiClient = new HttpClient { BaseAddress = new Uri(studentApiUrl) };
            var response = await studentApiClient.GetAsync($"api/students/{enrollment.StudentId}");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Student>>(responseContent);
                if (apiResponse?.Data != null)
                {
                    enrollment.Student = apiResponse.Data;
                }
                return new ServiceResult<EnrollmentDto?>(_mapper.Map<EnrollmentDto>(enrollment), "Get enrollment by id successfully");
            }
            else
            {
                Log.Error($"Failed to retrieve student from StudentApi: {response.StatusCode}");
                return new ServiceResult<EnrollmentDto?>(ResultType.InternalServerError, "Error retrieving student from StudentApi");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving student from StudentApi: {e.Message}");
            return new ServiceResult<EnrollmentDto?>(ResultType.InternalServerError, "Error retrieving student from StudentApi");
        }
    }

    public async Task<ServiceResult<List<EnrollmentDto>?>> GetEnrollmentsByCourseIdAsync(string courseId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(courseId);
        if (enrollments == null || enrollments.Count == 0)
        {
            Log.Error("No enrollments found");
            return new ServiceResult<List<EnrollmentDto>?>(ResultType.NotFound, "No enrollments found");
        }
        var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
        var studentApiClient = new HttpClient { BaseAddress = new Uri(studentApiUrl) };
        var ids = enrollments.Select(e => e.StudentId).Distinct().ToList();
        try
        {
            var response = await studentApiClient.GetAsync($"api/students/ids?ids={string.Join("&ids=", ids)}");
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Student>>>(responseContent);
                if (apiResponse?.Data != null)
                {
                    enrollments.ForEach(e => e.Student = apiResponse.Data.FirstOrDefault(s => s.StudentId == e.StudentId));
                }
                return new ServiceResult<List<EnrollmentDto>?>(_mapper.Map<List<EnrollmentDto>>(enrollments), "Get enrollments by course id successfully");
            }
            else
            {
                Log.Error($"Failed to retrieve students from StudentApi: {response.StatusCode}");
                return new ServiceResult<List<EnrollmentDto>?>(ResultType.InternalServerError, "Error retrieving students from StudentApi");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return new ServiceResult<List<EnrollmentDto>?>(ResultType.InternalServerError, "Error retrieving students from StudentApi");
        }
    }
    public async Task<ServiceResult<List<EnrollmentDto>?>> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(studentId);
        if (enrollments == null || enrollments.Count == 0)
        {
            Log.Error("No enrollments found");
            return new ServiceResult<List<EnrollmentDto>?>(ResultType.NotFound, "No enrollments found");
        }
        var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
        var studentApiClient = new HttpClient { BaseAddress = new Uri(studentApiUrl) };
        try
        {
            var response = await studentApiClient.GetAsync($"api/students/{studentId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"Failed to retrieve student with id {studentId} from StudentApi: {response.StatusCode}");
                return new ServiceResult<List<EnrollmentDto>?>(ResultType.InternalServerError, "Error retrieving student from StudentApi");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Student>>(responseContent);
            if (apiResponse?.Data == null || apiResponse.Data.StudentId != studentId)
            {
                Log.Error("Some thing wrong with student info");
                return new ServiceResult<List<EnrollmentDto>?>(ResultType.InternalServerError, "Some thing wrong with student info");
            }
            var enrollmentsWithStudent = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(studentId);
            enrollmentsWithStudent.ForEach(e => e.Student = apiResponse.Data);
            return new ServiceResult<List<EnrollmentDto>?>(_mapper.Map<List<EnrollmentDto>>(enrollmentsWithStudent), "Get enrollments by student id successfully");
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving student from StudentApi: {e.Message}");
            return new ServiceResult<List<EnrollmentDto>?>(ResultType.InternalServerError, "Error retrieving student from StudentApi");
        }
    }

    public async Task<ServiceResult<EnrollmentDto>?> EnrollStudentInCourseAsync(CreateEnrollmentDto createEnrollmentDto)
    {
        var course = await _courseRepository.GetCourseByCourseIdAsync(createEnrollmentDto.CourseId);
        if (course == null)
        {
            Log.Error($"Course with id {createEnrollmentDto.CourseId} not found");
            return new ServiceResult<EnrollmentDto>(ResultType.NotFound, "Course not found");
        }
        if (await _enrollmentRepository.IsStudentEnrolledInCourseAsync(createEnrollmentDto.StudentId, createEnrollmentDto.CourseId))
        {
            Log.Error($"Student with id {createEnrollmentDto.StudentId} is already enrolled in course with id {createEnrollmentDto.CourseId}");
            return new ServiceResult<EnrollmentDto>(ResultType.BadRequest, "Student is already enrolled in the course");
        }
        try
        {
            var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
            var studentApiClient = new HttpClient { BaseAddress = new Uri(studentApiUrl) };
            var response = await studentApiClient.GetAsync($"api/students/{createEnrollmentDto.StudentId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"Failed to retrieve student with id {createEnrollmentDto.StudentId} from StudentApi: {response.StatusCode}");
                return new ServiceResult<EnrollmentDto>(ResultType.InternalServerError, "Error retrieving student from StudentApi");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Student>>(responseContent);
            if (apiResponse?.Data == null || apiResponse.Data.StudentId != createEnrollmentDto.StudentId)
            {
                Log.Error("Some thing wrong with student info");
                return new ServiceResult<EnrollmentDto>(ResultType.BadRequest, "Some thing wrong with student info");
            }
            var enrollment = _mapper.Map<Enrollment>(createEnrollmentDto);
            var result = await _enrollmentRepository.CreateEnrollmentAsync(enrollment);
            if (result == null)
            {
                Log.Error("Failed to create enrollment");
                return new ServiceResult<EnrollmentDto>(ResultType.InternalServerError, "Failed to create enrollment");
            }
            return new ServiceResult<EnrollmentDto>(_mapper.Map<EnrollmentDto>(enrollment), "Enroll student in course successfully");
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return null;
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
}
