using StudentApi.Data;
using StudentApi.Models;
using Microsoft.EntityFrameworkCore;
using StudentApi.Helpers;

namespace StudentApi.Repositories;
public interface IStudentRepository
{
    Task<List<Student>> GetAllStudentsAsync(StudentQuery query);


    Task<Student?> GetStudentByIdAsync(int studentId);

    Task<Student?> GetStudentByEmailAsync(string email);

    Task<Student?> CreateStudentAsync(Student student);

    Task<Student?> UpdateStudentAsync(Student student);

    Task<bool> DeleteStudentAsync(Student student);

    Task<List<Student>> GetStudentsByIdsAsync(List<int> ids);


}
public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;
    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> CreateStudentAsync(Student student)
    {
        await _context.Students.AddAsync(student);
        var result = await _context.SaveChangesAsync();
        if (result > 0)
        {
            return student;
        }
        return null;
    }

    public async Task<bool> DeleteStudentAsync(Student student)
    {
        _context.Students.Remove(student);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<Student>> GetAllStudentsAsync(StudentQuery query)
    {
        var students = _context.Students.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.StudentName))
        {
            students = students.Where(s => s.FullName.ToUpper().Contains(query.StudentName.ToUpper()));
        }
        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            students = students.Where(s => s.Email.Contains(query.Email));
        }
        if (!string.IsNullOrWhiteSpace(query.PhoneNumber))
        {
            students = students.Where(s => s.PhoneNumber.Contains(query.PhoneNumber));
        }
        if (!string.IsNullOrEmpty(query.Address))
        {
            students = students.Where(c => c.Address.ToUpper().Contains(query.Address.ToUpper()));
        }

        if (query.GradeMin.HasValue)
        {
            students = students.Where(c => c.Grade >= query.GradeMin);
        }
        if (query.GradeMax.HasValue)
        {
            students = students.Where(c => c.Grade <= query.GradeMax);
        }
        if (query.Page.HasValue && query.ItemsPerPage.HasValue)
        {
            students = students.Skip((query.Page.Value - 1) * query.ItemsPerPage.Value)
                          .Take(query.ItemsPerPage.Value);
        }
        students = students.OrderBy(s => s.StudentId);
        return await students.ToListAsync();
    }

    public async Task<Student?> GetStudentByEmailAsync(string email)
    {
        return await _context.Students.Where(x => x.Email == email).FirstOrDefaultAsync();
    }

    public async Task<Student?> GetStudentByIdAsync(int studentId)
    {
        return await _context.Students.FirstOrDefaultAsync(x => x.StudentId == studentId);
    }

    public async Task<List<Student>> GetStudentsByIdsAsync(List<int> ids)
    {
        return await _context.Students.Where(x => ids.Contains(x.StudentId)).ToListAsync();
    }

    public async Task<Student?> UpdateStudentAsync(Student student)
    {
        _context.Entry(student).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return student;
    }
}
