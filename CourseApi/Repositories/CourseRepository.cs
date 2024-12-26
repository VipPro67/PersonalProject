using CourseApi.Data;
using CourseApi.Models;
using CourseApi.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace CourseApi.Repositories;
public interface ICourseRepository
{
    Task<List<Course>?> GetAllCoursesAsync(CourseQuery query);
    Task<List<Course>?> GetCoursesByIdsAsync(List<string> courseIds);
    Task<Course?> GetCourseByCourseIdAsync(string courseId);
    Task<Course?> EditCourseAsync(Course course);
    Task<Course?> CreateCourseAsync(Course course);
    Task<bool> DeleteCourseAsync(Course course);

    Task<int> GetTotalCoursesAsync(CourseQuery query);
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

    public async Task<List<Course>> GetAllCoursesAsync(CourseQuery query)
    {
        var courses = _context.Courses.AsQueryable();
        if (!string.IsNullOrEmpty(query.CourseId))
        {
            courses = courses.Where(c => c.CourseId.Contains(query.CourseId.ToUpper()));
        }
        if (!string.IsNullOrEmpty(query.CourseName))
        {
            courses = courses.Where(c => c.CourseName.ToUpper().Contains(query.CourseName.ToUpper()));
        }
        if (!string.IsNullOrEmpty(query.Instructor))
        {
            courses = courses.Where(c => c.Instructor.ToUpper().Contains(query.Instructor.ToUpper()));
        }
        if (!string.IsNullOrEmpty(query.Department))
        {
            courses = courses.Where(c => c.Department.ToUpper().Contains(query.Department.ToUpper()));
        }

        if (query.CreditMin.HasValue && query.CreditMin.Value >= 0 && query.CreditMin.Value <= 10)
        {
            courses = courses.Where(c => c.Credit >= query.CreditMin);
        }
        if (query.CreditMax.HasValue && query.CreditMax.Value >= 0)
        {
            courses = courses.Where(c => c.Credit <= query.CreditMax);
        }
        // if (query.StartDateMin.HasValue)
        // {
        //     courses = courses.Where(c => c.StartDate >= query.StartDateMin);
        // }
        // if (query.StartDateMax.HasValue)
        // {
        //     courses = courses.Where(c => c.StartDate <= query.StartDateMax);
        // }
        // if (query.EndDateMin.HasValue)
        // {
        //     courses = courses.Where(c => c.EndDate >= query.EndDateMin);
        // }
        // if (query.EndDateMax.HasValue)
        // {
        //     courses = courses.Where(c => c.EndDate >= query.EndDateMax);
        // }
        if (!string.IsNullOrEmpty(query.Schedule))
        {
            courses = courses.Where(c => c.Schedule.ToUpper().Contains(query.Schedule.ToUpper()));
        }

        if (!string.IsNullOrEmpty(query.SortBy) && !string.IsNullOrEmpty(query.SortByDirection))
        {
            courses = ApplySorting(courses, query.SortBy, query.SortByDirection);
        }

        if (query.Page.HasValue && query.ItemsPerPage.HasValue && query.ItemsPerPage.Value > 0 & query.Page.Value > 0 && query.ItemsPerPage.Value <= 1000)
        {
            courses = courses.Skip((query.Page.Value - 1) * query.ItemsPerPage.Value)
            .Take(query.ItemsPerPage.Value);
        }
        else
        {
            courses = courses.Take(10);
        }
        return await courses.ToListAsync();
    }

    private IQueryable<Course> ApplySorting(IQueryable<Course> courses, string sortBy, string? sortDirection)
    {
        bool isAscending = string.IsNullOrEmpty(sortDirection) || sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase);

        switch (sortBy.ToLower())
        {
            case "courseid":
                return isAscending ? courses.OrderBy(c => c.CourseId) : courses.OrderByDescending(c => c.CourseId);
            case "coursename":
                return isAscending ? courses.OrderBy(c => c.CourseName) : courses.OrderByDescending(c => c.CourseName);
            case "instructor":
                return isAscending ? courses.OrderBy(c => c.Instructor) : courses.OrderByDescending(c => c.Instructor);
            case "department":
                return isAscending ? courses.OrderBy(c => c.Department) : courses.OrderByDescending(c => c.Department);
            case "credit":
                return isAscending ? courses.OrderBy(c => c.Credit) : courses.OrderByDescending(c => c.Credit);
            case "startdate":
                return isAscending ? courses.OrderBy(c => c.StartDate) : courses.OrderByDescending(c => c.StartDate);
            case "enddate":
                return isAscending ? courses.OrderBy(c => c.EndDate) : courses.OrderByDescending(c => c.EndDate);
            case "schedule":
                return isAscending ? courses.OrderBy(c => c.Schedule) : courses.OrderByDescending(c => c.Schedule);
            default:
                return courses.OrderBy(c => c.CourseId);
        }
    }

    public async Task<List<Course>?> GetCoursesByIdsAsync(List<string> courseIds)
    {
        return await _context.Courses.Where(x => courseIds.Contains(x.CourseId)).ToListAsync();
    }

    public async Task<Course?> GetCourseByCourseIdAsync(string courseId)
    {
        return await _context.Courses.Include(c => c.Enrollments).FirstOrDefaultAsync(c => c.CourseId == courseId.ToUpper());
    }

    public async Task<int> GetTotalCoursesAsync(CourseQuery query)
    {
        var courses = _context.Courses.AsQueryable();
        if (!string.IsNullOrEmpty(query.CourseId))
        {
            courses = courses.Where(c => c.CourseId.Contains(query.CourseId.ToUpper()));
        }
        if (!string.IsNullOrEmpty(query.CourseName))
        {
            courses = courses.Where(c => c.CourseName.ToUpper().Contains(query.CourseName.ToUpper()));
        }
        if (!string.IsNullOrEmpty(query.Instructor))
        {
            courses = courses.Where(c => c.Instructor.ToUpper().Contains(query.Instructor.ToUpper()));
        }
        if (!string.IsNullOrEmpty(query.Department))
        {
            courses = courses.Where(c => c.Department.ToUpper().Contains(query.Department.ToUpper()));
        }
        if (query.CreditMin.HasValue && query.CreditMin.Value >= 0 && query.CreditMin.Value <= 10)
        {
            courses = courses.Where(c => c.Credit >= query.CreditMin);
        }
        if (query.CreditMax.HasValue && query.CreditMax.Value >= 0)
        {
            courses = courses.Where(c => c.Credit <= query.CreditMax);
        }

        if (!string.IsNullOrEmpty(query.Schedule))
        {
            courses = courses.Where(c => c.Schedule.ToUpper().Contains(query.Schedule.ToUpper()));
        }
        return await courses.CountAsync();
    }
}
