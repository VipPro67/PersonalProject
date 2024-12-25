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
    Task<ServiceResult> GetAllEnrollmentsAsync(EnrollmentQuery query);
    Task<ServiceResult> GetEnrollmentByIdAsync(int enrollmentId);

    Task<ServiceResult> GetEnrollmentsByCourseIdAsync(string courseId);
    Task<ServiceResult> GetEnrollmentsByStudentIdAsync(int studentId);
    Task<ServiceResult> EnrollStudentInCourseAsync(CreateEnrollmentDto createEnrollmentDto);
    Task<ServiceResult> DeleteEnrollmentAsync(int enrollmentId);
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
    public virtual HttpClient CreateHttpClient()
    {
        var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
        return new HttpClient { BaseAddress = new Uri(studentApiUrl) };
    }
    public async Task<ServiceResult> GetAllEnrollmentsAsync(EnrollmentQuery query)
    {
        var enrollments = await _enrollmentRepository.GetAllEnrollmentsAsync(query);
        if (enrollments == null || enrollments.Count == 0)
        {
            Log.Error("No enrollments found");
            return new ServiceResult(ResultType.NotFound, "No enrollments found");
        }
        var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
        var studentApiClient = CreateHttpClient();
        var ids = enrollments.Select(e => e.StudentId).Distinct().ToList();
        try
        {
            var response = await studentApiClient.GetAsync($"api/students/ids?ids={string.Join("&ids=", ids)}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new DateOnlyJsonConverter());
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Student>>>(responseContent, settings);
                if (apiResponse?.Data != null)
                {
                    enrollments.ForEach(e => e.Student = apiResponse.Data.FirstOrDefault(s => s.StudentId == e.StudentId));
                }
                int totalItems = await _enrollmentRepository.GetTotalEnrollmentsAsync(query);
                var pagination = new
                {
                    TotalItems = totalItems,
                    CurrentPage = query.Page,
                    TotalPage = (int)Math.Ceiling(totalItems / (double)query.ItemsPerPage),
                    ItemsPerPage = query.ItemsPerPage
                };

                return new ServiceResult(_mapper.Map<List<EnrollmentDto>>(enrollments), "Get all enrollments successfully", pagination);
            }
            else
            {
                Log.Error($"Failed to retrieve students from StudentApi: {response.StatusCode}");
                return new ServiceResult(ResultType.InternalServerError, "Error retrieving students from StudentApi");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return new ServiceResult(ResultType.InternalServerError, "Error retrieving students from StudentApi");
        }
    }
    public async Task<ServiceResult> GetEnrollmentByIdAsync(int enrollmentId)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(enrollmentId);
        if (enrollment == null)
        {
            Log.Error("No enrollments found");
            return new ServiceResult(ResultType.NotFound, "No enrollments found");
        }
        try
        {
            var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
            var studentApiClient = CreateHttpClient();
            var response = await studentApiClient.GetAsync($"api/students/{enrollment.StudentId}");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new DateOnlyJsonConverter());
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Student>>(responseContent, settings);
                if (apiResponse?.Data != null)
                {
                    enrollment.Student = apiResponse.Data;
                }
                return new ServiceResult(_mapper.Map<EnrollmentDto>(enrollment), "Get enrollment by id successfully");
            }
            else
            {
                Log.Error($"Failed to retrieve student from StudentApi: {response.StatusCode}");
                return new ServiceResult(ResultType.InternalServerError, "Error retrieving student from StudentApi");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving student from StudentApi: {e.Message}");
            return new ServiceResult(ResultType.InternalServerError, "Error retrieving student from StudentApi");
        }
    }

    public async Task<ServiceResult> GetEnrollmentsByCourseIdAsync(string courseId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(courseId);
        if (enrollments == null || enrollments.Count == 0)
        {
            Log.Error("No enrollments found");
            return new ServiceResult(ResultType.NotFound, "No enrollments found");
        }
        var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
        var studentApiClient = CreateHttpClient();
        var ids = enrollments.Select(e => e.StudentId).Distinct().ToList();
        try
        {
            var response = await studentApiClient.GetAsync($"api/students/ids?ids={string.Join("&ids=", ids)}");
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new DateOnlyJsonConverter());
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Student>>>(responseContent, settings);
                if (apiResponse?.Data != null)
                {
                    enrollments.ForEach(e => e.Student = apiResponse.Data.FirstOrDefault(s => s.StudentId == e.StudentId));
                }
                return new ServiceResult(_mapper.Map<List<EnrollmentDto>>(enrollments), "Get enrollments by course id successfully");
            }
            else
            {
                Log.Error($"Failed to retrieve students from StudentApi: {response.StatusCode}");
                return new ServiceResult(ResultType.InternalServerError, "Error retrieving students from StudentApi");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return new ServiceResult(ResultType.InternalServerError, "Error retrieving students from StudentApi");
        }
    }
    public async Task<ServiceResult> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(studentId);
        if (enrollments == null || enrollments.Count == 0)
        {
            Log.Error("No enrollments found");
            return new ServiceResult(ResultType.NotFound, "No enrollments found");
        }
        var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
        var studentApiClient = CreateHttpClient();
        try
        {
            var response = await studentApiClient.GetAsync($"api/students/{studentId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"Failed to retrieve student with id {studentId} from StudentApi: {response.StatusCode}");
                return new ServiceResult(ResultType.InternalServerError, "Error retrieving student from StudentApi");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new DateOnlyJsonConverter());
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Student>>(responseContent, settings);
            if (apiResponse?.Data == null || apiResponse.Data.StudentId != studentId)
            {
                Log.Error("Some thing wrong with student info");
                return new ServiceResult(ResultType.InternalServerError, "Some thing wrong with student info");
            }
            var enrollmentsWithStudent = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(studentId);
            enrollmentsWithStudent.ForEach(e => e.Student = apiResponse.Data);
            return new ServiceResult(_mapper.Map<List<EnrollmentDto>>(enrollmentsWithStudent), "Get enrollments by student id successfully");
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving student from StudentApi: {e.Message}");
            return new ServiceResult(ResultType.InternalServerError, "Error retrieving student from StudentApi");
        }
    }

    public async Task<ServiceResult> EnrollStudentInCourseAsync(CreateEnrollmentDto createEnrollmentDto)
    {
        var course = await _courseRepository.GetCourseByCourseIdAsync(createEnrollmentDto.CourseId);
        if (course == null)
        {
            Log.Error($"Course with id {createEnrollmentDto.CourseId} not found");
            return new ServiceResult(ResultType.NotFound, "Course not found");
        }
        var isEnroll = await _enrollmentRepository.IsStudentEnrolledInCourseAsync(createEnrollmentDto.StudentId, createEnrollmentDto.CourseId);
        if (isEnroll)
        {
            Log.Error($"Student with id {createEnrollmentDto.StudentId} is already enrolled in course with id {createEnrollmentDto.CourseId}");
            return new ServiceResult(ResultType.BadRequest, "Student is already enrolled in the course");
        }
        try
        {
            var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
            var studentApiClient = CreateHttpClient();
            var response = await studentApiClient.GetAsync($"api/students/{createEnrollmentDto.StudentId}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Log.Error($"Student not found");
                    return new ServiceResult(ResultType.InternalServerError, $"StudentId {createEnrollmentDto.StudentId} not found");

                }
                Log.Error($"Failed to retrieve student with id {createEnrollmentDto.StudentId} from StudentApi: {response.StatusCode}");
                return new ServiceResult(ResultType.InternalServerError, "Error retrieving student from StudentApi");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new DateOnlyJsonConverter());
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Student>>(responseContent, settings);
            if (apiResponse?.Data == null || apiResponse.Data.StudentId != createEnrollmentDto.StudentId)
            {
                Log.Error("Some thing wrong with student info");
                return new ServiceResult(ResultType.BadRequest, "Some thing wrong with student info");
            }
            var enrollment = _mapper.Map<Enrollment>(createEnrollmentDto);
            enrollment.Student = apiResponse.Data;
            var result = await _enrollmentRepository.CreateEnrollmentAsync(enrollment);
            if (result == null)
            {
                Log.Error("Failed to create enrollment");
                return new ServiceResult(ResultType.InternalServerError, "Failed to create enrollment");
            }
            return new ServiceResult(_mapper.Map<EnrollmentDto>(enrollment), "Enroll student in course successfully");
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return new ServiceResult(ResultType.InternalServerError, "Failed to create enrollment");
        }
    }
    public async Task<ServiceResult> DeleteEnrollmentAsync(int enrollmentId)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(enrollmentId);
        if (enrollment == null)
        {
            Log.Error($"Enrollment with id {enrollmentId} not found");
            return new ServiceResult(ResultType.NotFound, "Enrollment not found");
        }
        var result = await _enrollmentRepository.DeleteEnrollmentAsync(enrollment);
        if (!result)
        {
            Log.Error("Failed to delete enrollment");
            return new ServiceResult(ResultType.InternalServerError, "Failed to delete enrollment");
        }
        return new ServiceResult(true, "Delete enrollment successfully");
    }
}
