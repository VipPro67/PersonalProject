using AutoMapper;
using Serilog;
using StudentApi.DTOs;
using StudentApi.Models;
using StudentApi.Repositories;

namespace StudentApi.Services;
public interface IStudentService
{
    Task<Student?> GetStudentByIdAsync(int id);
    Task<List<Student>?> GetStudentsByIdsAsync(List<int> ids);
    Task<Student?> GetStudentByEmailAsync(string email);
    Task<Student?> CreateStudentAsync(CreateStudentDto student);
    Task<List<Student>> GetStudentsAsync();
    Task<Student?> UpdateStudentAsync(int id, UpdateStudentDto updatedStudent);
    Task<bool> DeleteStudentAsync(int studentId);
}

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IMapper _mapper;

    public StudentService(IStudentRepository studentRepository, IMapper mapper)
    {
        _studentRepository = studentRepository;
        _mapper = mapper;
    }
    public async Task<Student?> CreateStudentAsync(CreateStudentDto createStudentDto)
    {
        if (await _studentRepository.GetStudentByEmailAsync(createStudentDto.Email) != null)
        {
            Log.Information($"Student with email {createStudentDto.Email} already exists");
            return null;
        }
        var student = _mapper.Map<Student>(createStudentDto);
        return await _studentRepository.CreateStudentAsync(student);
    }

    public async Task<bool> DeleteStudentAsync(int studentId)
    {
        var student = await _studentRepository.GetStudentByIdAsync(studentId);
        if (student == null)
        {
            return false;
        }
        return await _studentRepository.DeleteStudentAsync(student);
    }

    public async Task<Student?> GetStudentByEmailAsync(string email)
    {
        return await _studentRepository.GetStudentByEmailAsync(email);
    }

    public async Task<Student?> GetStudentByIdAsync(int studentId)
    {
        return await _studentRepository.GetStudentByIdAsync(studentId);
    }

    public Task<List<Student>> GetStudentsAsync()
    {
        return _studentRepository.GetAllStudentsAsync();
    }

    public async Task<List<Student>?> GetStudentsByIdsAsync(List<int> ids)
    {
        return await _studentRepository.GetStudentsByIdsAsync(ids);
    }

    public async Task<Student?> UpdateStudentAsync(int id, UpdateStudentDto updatedStudentDto)
    {
        if (id != updatedStudentDto.StudentId)
        {
            Log.Information("Id in UpdateStudentDto does not match the id in the URL");
            return null;
        }
        var existingStudent = await _studentRepository.GetStudentByIdAsync(id);
        if (existingStudent == null)
        {
            return null;
        }
        _mapper.Map(updatedStudentDto, existingStudent);
        return await _studentRepository.UpdateStudentAsync(existingStudent);
    }
}