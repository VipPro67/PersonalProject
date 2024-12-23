using CourseApi.Data;
using CourseApi.Helpers;
using CourseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseApi.Repositories;

public interface IEnrollmentRepository
{
    Task<List<Enrollment>?> GetAllEnrollmentsAsync(EnrollmentQuery query);
    Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId);

    Task<bool> IsStudentEnrolledInCourseAsync(int enrollmentId, string courseId);

    Task<List<Enrollment>?> GetEnrollmentsByCourseIdAsync(string courseId);

    Task<List<Enrollment>?> GetEnrollmentsByStudentIdAsync(int studentId);

    Task<Enrollment?> CreateEnrollmentAsync(Enrollment enrollment);

    Task<bool> DeleteEnrollmentAsync(Enrollment enrollment);
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
        enrollments = enrollments.Skip((query.Page - 1) * query.Page).Take(query.ItemsPerPage);
        enrollments = enrollments.OrderBy(e => e.EnrollmentId);
        return await enrollments.ToListAsync();
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

    public Task<bool> IsStudentEnrolledInCourseAsync(int studentId, string courseId)
    {
        return _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId.ToUpper());
    }
}