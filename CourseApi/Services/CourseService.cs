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
    Task<ServiceResult<List<CourseDto>>> GetCoursesAsync(CourseQuery query);
    Task<ServiceResult<CourseDto?>> GetCourseByCourseIdAsync(string courseId);
    Task<ServiceResult<CourseDto?>> CreateCourseAsync(CreateCourseDto createCourseDto);
    Task<ServiceResult<CourseDto?>> UpdateCourseAsync(string courseId, UpdateCourseDto updateCourseDto);
    Task<ServiceResult<List<StudentDto>?>> GetStudentsByCourseIdAsync(string courseId);
    Task<ServiceResult<bool>> DeleteCourseAsync(string courseId);
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

    public async Task<ServiceResult<List<CourseDto>>> GetCoursesAsync(CourseQuery query)
    {
        var courses = await _courseRepository.GetAllCoursesAsync(query);
        return new ServiceResult<List<CourseDto>>(_mapper.Map<List<CourseDto>>(courses), "Get list course successfully");
    }

    public async Task<ServiceResult<CourseDto?>> GetCourseByCourseIdAsync(string courseId)
    {
        var course = await _courseRepository.GetCourseByCourseIdAsync(courseId);
        return new ServiceResult<CourseDto?>(_mapper.Map<CourseDto?>(course), "Get course by id successfully");
    }

    public async Task<ServiceResult<CourseDto?>> CreateCourseAsync(CreateCourseDto createCourseDto)
    {
        var course = _mapper.Map<Course>(createCourseDto);
        var existingCourse = await _courseRepository.GetCourseByCourseIdAsync(course.CourseId);
        if (existingCourse != null)
        {
            Log.Error("Course with id: {Id} already exists", course.CourseId);
            return new ServiceResult<CourseDto?>(ResultType.BadRequest, "Course with id already exists");
        }
        var createdCourse = await _courseRepository.CreateCourseAsync(course);
        return new ServiceResult<CourseDto?>(_mapper.Map<CourseDto?>(createdCourse), "Create course successfully");
    }

    public async Task<ServiceResult<CourseDto?>> UpdateCourseAsync(string courseId, UpdateCourseDto updateCourseDto)
    {
        var existingCourse = await _courseRepository.GetCourseByCourseIdAsync(courseId);
        if (existingCourse == null)
        {
            Log.Error("Course not found for id: {Id}", courseId);
            return new ServiceResult<CourseDto?>(ResultType.NotFound, "Course not found");
        }
        _mapper.Map(updateCourseDto, existingCourse);
        var updatedCourse = await _courseRepository.EditCourseAsync(existingCourse);
        return new ServiceResult<CourseDto?>(_mapper.Map<CourseDto?>(updatedCourse), "Update course successfully");
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

    public async Task<ServiceResult<List<StudentDto>?>> GetStudentsByCourseIdAsync(string courseId)
    {
        var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(courseId);
        if (enrollments == null || enrollments.Count == 0)
        {
            Log.Error("No enrollment found for course id: {Id}", courseId);
            return new ServiceResult<List<StudentDto>?>(ResultType.NotFound, "No enrollment found");
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
                return new ServiceResult<List<StudentDto>?>(_mapper.Map<List<StudentDto>>(apiResponse.Data), "Get list student in course successfully");
            }
            else
            {
                Log.Error($"Failed to retrieve students from StudentApi: {response.StatusCode}");
                return new ServiceResult<List<StudentDto>?>(ResultType.InternalServerError, "Error retrieving students from StudentApi");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return new ServiceResult<List<StudentDto>?>(ResultType.InternalServerError, "Error retrieving students from StudentApi");
        }
    }
}
