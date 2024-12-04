using CourseApi.Data;
using CourseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseApi.Repositories;

public interface IEnrollmentRepository
{
    Task<List<Enrollment>?> GetAllEnrollmentsAsync();
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

    public async Task<List<Enrollment>?> GetAllEnrollmentsAsync()
    {
        return await _context.Enrollments.Include(e => e.Course).ToListAsync();
    }

    public async Task<List<Enrollment>?> GetEnrollmentsByCourseIdAsync(string courseId)
    {
        return await _context.Enrollments.Where(e => e.CourseId == courseId).Include(e => e.Course).ToListAsync();
    }

    public async Task<List<Enrollment>?> GetEnrollmentsByStudentIdAsync(int courseId)
    {
        return await _context.Enrollments.Where(e => e.StudentId == courseId).Include(e => e.Course).ToListAsync();
    }

    public async Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId)
    {
        return await _context.Enrollments.Include(e => e.Course).FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
    }

    public Task<bool> IsStudentEnrolledInCourseAsync(int enrollmentId, string courseId)
    {
        return _context.Enrollments.AnyAsync(e => e.EnrollmentId == enrollmentId && e.CourseId == courseId);
    }
}