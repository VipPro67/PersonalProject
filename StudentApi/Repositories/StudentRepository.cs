using StudentApi.Data;
using StudentApi.Models;
using Microsoft.EntityFrameworkCore;

namespace StudentApi.Repositories;
public interface IStudentRepository
{
    Task<List<Student>> GetAllStudentsAsync();


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

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        return await _context.Students.ToListAsync();
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
