using CourseApi.Data;
using CourseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseApi.Repositories;
public interface ICourseRepository
{
    Task<List<Course>> GetAllCoursesAsync();
    Task<Course?> GetCourseByCourseIdAsync(string courseId);
    Task<Course?> EditCourseAsync(Course course);

    Task<Course?> CreateCourseAsync(Course course);
    Task<bool> DeleteCourseAsync(Course course);


}
public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;
    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }   

    public async Task<Course?> CreateCourseAsync(Course course)
    {
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();
        return await _context.Courses.FindAsync(course.CourseId);

    }

    public async Task<bool> DeleteCourseAsync(Course course)
    {
        _context.Courses.Remove(course);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Course?> EditCourseAsync(Course course)
    {
        _context.Entry(course).State = EntityState.Modified;
        //_context.Courses.Update(course);
        _context.SaveChanges();
        return await _context.Courses.FindAsync(course.CourseId);
    }

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        return await _context.Courses.ToListAsync();
    }

    public async Task<Course?> GetCourseByCourseIdAsync(string courseId)
    {
        return await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
    }
}
