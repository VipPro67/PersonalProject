using StudentApi.Data;
using StudentApi.Models;
using Microsoft.EntityFrameworkCore;
using StudentApi.Helpers;
using Serilog;

namespace StudentApi.Repositories;
public interface IStudentRepository
{
    Task<List<Student>> GetAllStudentsAsync(StudentQuery query);


    Task<Student?> GetStudentByIdAsync(int studentId);

    Task<Student?> GetStudentByEmailAsync(string email);

    Task<Student?> GetStudentByPhoneNumberAsync(string phoneNumber);

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
            //students = students.Where(s => s.Email.ToUpper().Contains(query.Email.ToUpper()));
            /*'%lice%'*/
            students = students.Where(s => EF.Functions.Like(s.Email.ToUpper(), $"{query.Email.ToUpper()}%"));
            /*
            2024-12-24T08:23:08.4090062+00:00 [INF] (StudentApi//) GetStudentsAsync: -- @__Format_1='NON%'
            -- @__p_3='10'
            -- @__p_2='0'
            SELECT s."StudentId", s."Address", s."DateOfBirth", s."Email", s."FullName", s."Grade", s."PhoneNumber"
            FROM "Students" AS s
            WHERE upper(s."Email") LIKE @__Format_1 ESCAPE ''
            ORDER BY s."StudentId"
            LIMIT @__p_3 OFFSET @__p_2*/
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
        // Apply sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            students = ApplySorting(students, query.SortBy, query.SortByDirection);
        }
        if (query.Page.HasValue && query.ItemsPerPage.HasValue & query.ItemsPerPage.Value > 0 & query.Page.Value > 0 && query.ItemsPerPage.Value <= 1000)
        {
            students = students.Skip((query.Page.Value - 1) * query.ItemsPerPage.Value)
                          .Take(query.ItemsPerPage.Value);
        }
        else
        {
            students = students.Take(10);
        }
        // Log the query
        Log.Information($"GetStudentsAsync: {students.ToQueryString()}");
        return await students.ToListAsync();
    }

    private IQueryable<Student> ApplySorting(IQueryable<Student> students, string sortBy, string sortDirection)
    {
        bool isAscending = string.IsNullOrEmpty(sortDirection) || sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase);
        switch (sortBy.ToLower())
        {
            case "studentid":
                return isAscending ? students.OrderBy(s => s.StudentId) : students.OrderByDescending(s => s.StudentId);
            case "fullname":
                return isAscending ? students.OrderBy(s => s.FullName) : students.OrderByDescending(s => s.FullName);
            case "email":
                return isAscending ? students.OrderBy(s => s.Email) : students.OrderByDescending(s => s.Email);
            case "phonenumber":
                return isAscending ? students.OrderBy(s => s.PhoneNumber) : students.OrderByDescending(s => s.PhoneNumber);
            case "address":
                return isAscending ? students.OrderBy(s => s.Address) : students.OrderByDescending(s => s.Address);
            case "grade":
                return isAscending ? students.OrderBy(s => s.Grade) : students.OrderByDescending(s => s.Grade);
            default:
                return students.OrderBy(s => s.StudentId);
        }
    }
    public async Task<Student?> GetStudentByEmailAsync(string email)
    {
        return await _context.Students.Where(x => x.Email.ToUpper() == email.ToUpper()).FirstOrDefaultAsync();
    }

    public async Task<Student?> GetStudentByIdAsync(int studentId)
    {
        return await _context.Students.FirstOrDefaultAsync(x => x.StudentId == studentId);
    }

    public async Task<Student?> GetStudentByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Students.Where(x => x.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
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
