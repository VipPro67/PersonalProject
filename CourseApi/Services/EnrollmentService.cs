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
    Task<List<Enrollment>?> GetAllEnrollmentsAsync();
    Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId);

    Task<List<Enrollment>?> GetEnrollmentsByCourseIdAsync(string courseId);
    Task<List<Enrollment>?> GetEnrollmentsByStudentIdAsync(int studentId);
    Task<Enrollment?> EnrollStudentInCourseAsync(CreateEnrollmentDto createEnrollmentDto);
    Task<bool> DeleteEnrollmentAsync(int enrollmentId);
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
    public async Task<List<Enrollment>?> GetAllEnrollmentsAsync()
    {
        var enrollments = await _enrollmentRepository.GetAllEnrollmentsAsync();
        if (enrollments == null || enrollments.Count == 0)
        {
            return null;
        }
        var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
        var studentApiClient = new HttpClient { BaseAddress = new Uri(studentApiUrl) };
        var ids = enrollments.Select(x => x.StudentId).ToList();
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
            }
            else
            {
                Log.Error($"Failed to retrieve students from StudentApi: {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
        }

        return enrollments;
    }
    public async Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId)
    {
        var enrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(enrollmentId);
        if (enrollment == null)
        {
            return null;
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
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving student from StudentApi: {e.Message}");
        }

        return enrollment;
    }

    public async Task<List<Enrollment>?> GetEnrollmentsByCourseIdAsync(string courseId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(courseId);
        if (enrollments == null || enrollments.Count == 0)
        {
            return null;
        }
        var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
        var studentApiClient = new HttpClient { BaseAddress = new Uri(studentApiUrl) };
        var ids = enrollments.Select(x => x.StudentId).ToList();

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
            }
            else
            {
                Log.Error($"Failed to retrieve students from StudentApi: {response.StatusCode}");
            }

        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
        }
        return enrollments;
    }
    public async Task<List<Enrollment>?> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(studentId);
        if (enrollments == null || enrollments.Count == 0)
        {
            return null;
        }
        return enrollments;
    }

    public async Task<Enrollment?> EnrollStudentInCourseAsync(CreateEnrollmentDto createEnrollmentDto)
    {
        var course = await _courseRepository.GetCourseByCourseIdAsync(createEnrollmentDto.CourseId);
        if (course == null)
        {
            Log.Error($"Course with id {createEnrollmentDto.CourseId} not found");
            return null;
        }
        if (await _enrollmentRepository.IsStudentEnrolledInCourseAsync(createEnrollmentDto.StudentId, createEnrollmentDto.CourseId))
        {
            Log.Error($"Student with id {createEnrollmentDto.StudentId} is already enrolled in course with id {createEnrollmentDto.CourseId}");
            return null;
        }
        try
        {
            var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
            var studentApiClient = new HttpClient { BaseAddress = new Uri(studentApiUrl) };
            var response = await studentApiClient.GetAsync($"api/students/{createEnrollmentDto.StudentId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"Failed to retrieve student with id {createEnrollmentDto.StudentId} from StudentApi: {response.StatusCode}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Student>>(responseContent);
            if (apiResponse?.Data != null && apiResponse.Data.StudentId == createEnrollmentDto.StudentId)
            {
                var enrollment = _mapper.Map<Enrollment>(createEnrollmentDto);
                await _enrollmentRepository.CreateEnrollmentAsync(enrollment);
                return enrollment;
            }
            return null;

        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return null;
        }
    }
    public async Task<bool> DeleteEnrollmentAsync(int enrollmentId)
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
