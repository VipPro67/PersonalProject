using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Models;
using CourseApi.Repositories;
using Serilog;

namespace CourseApi.Services;
public interface ICourseService
{
    Task<List<Course>> GetCoursesAsync();
    Task<Course?> GetCourseByCourseIdAsync(string courseId);
    Task<Course?> CreateCourseAsync(CreateCourseDto createCourseDto);
    Task<Course?> UpdateCourseAsync(string courseId, UpdateCourseDto updateCourseDto);

    Task<bool> DeleteCourseAsync(string courseId);
}
public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IMapper _mapper;

    public CourseService(ICourseRepository courseRepository, IMapper mapper)
    {
        _courseRepository = courseRepository;
        _mapper = mapper;
    }

    public async Task<List<Course>> GetCoursesAsync()
    {
        var courses = await _courseRepository.GetAllCoursesAsync();
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
        if (existingCourse!= null)
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
        Log.Information("course",existingCourse);
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
        return await _courseRepository.DeleteCourseAsync(course);
    }
}
