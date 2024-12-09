using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Models;
using CourseApi.Repositories;
using Newtonsoft.Json;
using Serilog;

namespace CourseApi.Services;
public interface ICourseService
{
    Task<List<Course>> GetCoursesAsync(CourseQuery query  );
    Task<Course?> GetCourseByCourseIdAsync(string courseId);
    Task<Course?> CreateCourseAsync(CreateCourseDto createCourseDto);
    Task<Course?> UpdateCourseAsync(string courseId, UpdateCourseDto updateCourseDto);

    Task<List<Student>?> GetStudentsByCourseIdAsync(string courseId);

    Task<bool> DeleteCourseAsync(string courseId);
}
public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;

    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;

    public CourseService(ICourseRepository courseRepository, IEnrollmentRepository enrollmentRepository, IMapper mapper)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _mapper = mapper;
    }

    public async Task<List<Course>> GetCoursesAsync(CourseQuery query)
    {
        var courses = await _courseRepository.GetAllCoursesAsync(query);
        return courses;
    }

    public async Task<Course?> GetCourseByCourseIdAsync(string courseId)
    {
        var course = await _courseRepository.GetCourseByCourseIdAsync(courseId);
        return course;
    }

    public async Task<Course?> CreateCourseAsync(CreateCourseDto createCourseDto)
    {
        var course = _mapper.Map<Course>(createCourseDto);
        var existingCourse = await _courseRepository.GetCourseByCourseIdAsync(course.CourseId);
        if (existingCourse != null)
        {
            Log.Error("Course with id: {Id} already exists", course.CourseId);
            return null;
        }
        var createdCourse = await _courseRepository.CreateCourseAsync(course);
        return createdCourse;
    }

    public async Task<Course?> UpdateCourseAsync(string courseId, UpdateCourseDto updateCourseDto)
    {
        var existingCourse = await _courseRepository.GetCourseByCourseIdAsync(courseId);
        Log.Information("Updating course with id: {Id}", courseId);
        Log.Information("course", existingCourse);
        if (existingCourse == null)
        {
            Log.Error("Course not found for id: {Id}", courseId);
            return null;
        }
        _mapper.Map(updateCourseDto, existingCourse);
        var updatedCourse = await _courseRepository.EditCourseAsync(existingCourse);
        return updatedCourse;
    }

    public async Task<bool> DeleteCourseAsync(string courseId)
    {
        var course = await _courseRepository.GetCourseByCourseIdAsync(courseId);
        if (course == null)
        {
            Log.Error("Course not found for id: {Id}", courseId);
            return false;
        }
        //check course enrollment 
        var enrollments = _enrollmentRepository.GetEnrollmentsByCourseIdAsync(course.CourseId);
        if (enrollments != null)
        {
            Log.Error("Delete course failed!. Exist enrollment in this course");
            return false;
        }
        return await _courseRepository.DeleteCourseAsync(course);
    }

    public async Task<List<Student>?> GetStudentsByCourseIdAsync(string courseId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(courseId);
        if (enrollments == null || enrollments.Count == 0)
            return null;
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
                return apiResponse.Data;
            }
            else
            {
                Log.Error($"Failed to retrieve students from StudentApi: {response.StatusCode}");
                return null;
            }

        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return null;
        }
    }
}
