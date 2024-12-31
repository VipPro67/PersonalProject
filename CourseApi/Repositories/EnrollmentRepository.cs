using CourseApi.Data;
using CourseApi.Helpers;
using CourseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseApi.Repositories;

public interface IEnrollmentRepository
{
    Task<List<Enrollment>?> GetAllEnrollmentsAsync(EnrollmentQuery query);
    
    Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId);

    Task<bool> IsStudentHasEnrollmentAsync(int enrollmentId, string courseId);

    Task<List<Enrollment>?> GetEnrollmentsByCourseIdAsync(string courseId);

    Task<List<Enrollment>?> GetEnrollmentsByStudentIdAsync(int studentId);

    Task<Enrollment?> CreateEnrollmentAsync(Enrollment enrollment);

    Task<bool> DeleteEnrollmentAsync(Enrollment enrollment);

    Task<bool> IsStudentHasEnrollmentAsync(int studentId);

    Task<int> GetTotalEnrollmentsAsync(EnrollmentQuery query);
}

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;
    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Enrollment?> CreateEnrollmentAsync(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        _context.SaveChanges();
        return await _context.Enrollments.FindAsync(enrollment.EnrollmentId);
    }

    public async Task<bool> DeleteEnrollmentAsync(Enrollment enrollment)
    {
        _context.Enrollments.Remove(enrollment);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<Enrollment>?> GetAllEnrollmentsAsync(EnrollmentQuery query)
    {
        var enrollments = _context.Enrollments.AsQueryable();
        enrollments = enrollments.Include(e => e.Course);
        if (query.StudentId.HasValue)
        {
            enrollments = enrollments.Where(e => e.StudentId == query.StudentId);
        }
        if (!string.IsNullOrWhiteSpace(query.CourseId))
        {
            enrollments = enrollments.Where(e => e.CourseId == query.CourseId.ToUpper());
        }
        enrollments = ApplySorting(enrollments, query.SortBy, query.SortByDirection);
        if (query.Page.HasValue && query.ItemsPerPage.HasValue && query.ItemsPerPage.Value > 0 & query.Page.Value > 0 & query.ItemsPerPage.Value <= 1000)
        {
            enrollments = enrollments.Skip((query.Page.Value - 1) * query.ItemsPerPage.Value)
            .Take(query.ItemsPerPage.Value);
        }
        else
        {
            enrollments = enrollments.Take(10);
        }
        return await enrollments.ToListAsync();
    }

    private IQueryable<Enrollment> ApplySorting(IQueryable<Enrollment> enrollments, string sortBy, string? sortDirection)
    {
        bool isAscending = string.IsNullOrEmpty(sortDirection) || sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase);

        switch (sortBy.ToLower())
        {
            case "enrollmentid":
                return isAscending ? enrollments.OrderBy(e => e.EnrollmentId) : enrollments.OrderByDescending(e => e.EnrollmentId);
            case "studentid":
                return isAscending ? enrollments.OrderBy(e => e.StudentId) : enrollments.OrderByDescending(e => e.StudentId);
            case "courseid":
                return isAscending ? enrollments.OrderBy(e => e.CourseId) : enrollments.OrderByDescending(e => e.CourseId);
            default:
                return enrollments.OrderBy(e => e.EnrollmentId);
        }
    }
    public async Task<List<Enrollment>?> GetEnrollmentsByCourseIdAsync(string courseId)
    {
        return await _context.Enrollments.Include(e => e.Course).Where(e => e.CourseId == courseId.ToUpper()).ToListAsync();
    }

    public async Task<List<Enrollment>?> GetEnrollmentsByStudentIdAsync(int studentId)
    {
        return await _context.Enrollments.Include(e => e.Course).Where(e => e.StudentId == studentId).ToListAsync();
    }

    public async Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId)
    {
        return await _context.Enrollments.Include(e => e.Course).FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
    }

    public Task<bool> IsStudentHasEnrollmentAsync(int studentId, string courseId)
    {
        return _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId.ToUpper());
    }

    public async Task<int> GetTotalEnrollmentsAsync(EnrollmentQuery query)
    {
        var enrollments = _context.Enrollments.AsQueryable();
        if (query.StudentId.HasValue)
        {
            enrollments = enrollments.Where(e => e.StudentId == query.StudentId);
        }
        if (!string.IsNullOrWhiteSpace(query.CourseId))
        {
            enrollments = enrollments.Where(e => e.CourseId == query.CourseId.ToUpper());
        }
        return await enrollments.CountAsync();
    }
    public async Task<bool> IsStudentHasEnrollmentAsync(int studentId)
    {
        return await _context.Enrollments.AnyAsync(e => e.StudentId == studentId);
    }

}