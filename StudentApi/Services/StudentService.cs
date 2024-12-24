using System.Net;
using AutoMapper;
using Serilog;
using StudentApi.DTOs;
using StudentApi.Helpers;
using StudentApi.Models;
using StudentApi.Repositories;
namespace StudentApi.Services;
public interface IStudentService
{
    Task<ServiceResult> GetStudentByIdAsync(int id);
    Task<ServiceResult> GetStudentsByIdsAsync(List<int> ids);
    Task<ServiceResult> GetStudentByEmailAsync(string email);
    Task<ServiceResult> CreateStudentAsync(CreateStudentDto student);
    Task<ServiceResult> GetStudentsAsync(StudentQuery query);
    Task<ServiceResult> UpdateStudentAsync(int id, UpdateStudentDto updatedStudent);
    Task<ServiceResult> DeleteStudentAsync(int studentId);
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
    public virtual HttpClient CreateHttpClient()
    {
        var courseApiUrl = Environment.GetEnvironmentVariable("CourseApiUrl");
        return new HttpClient { BaseAddress = new Uri(courseApiUrl) };
    }
    public async Task<ServiceResult> CreateStudentAsync(CreateStudentDto createStudentDto)
    {
        if (await _studentRepository.GetStudentByEmailAsync(createStudentDto.Email) != null)
        {
            Log.Error($"Create student failed. Student with email {createStudentDto.Email} already exists");
            return new ServiceResult(ResultType.BadRequest, $"Student with email {createStudentDto.Email} already exists");
        }
        if (await _studentRepository.GetStudentByPhoneNumberAsync(createStudentDto.PhoneNumber) != null)
        {
            Log.Error($"Create student failed. Student with phoneNumber {createStudentDto.PhoneNumber} already exists");
            return new ServiceResult(ResultType.BadRequest, $"Student with phoneNumber {createStudentDto.PhoneNumber} already exists");
        }
        var result = await _studentRepository.CreateStudentAsync(_mapper.Map<Student>(createStudentDto));
        return new ServiceResult(_mapper.Map<StudentDto>(result), "Create student successfully");
    }
    public async Task<ServiceResult> DeleteStudentAsync(int studentId)
    {
        var student = await _studentRepository.GetStudentByIdAsync(studentId);
        if (student == null)
        {
            Log.Error($"Delete student with id {studentId} failed. Student not found");
            return new ServiceResult(ResultType.NotFound, "Student not found");
        }
        try
        {
            var courseApiUrl = Environment.GetEnvironmentVariable("CourseApiUrl");
            var courseApiClient = CreateHttpClient();
            var response = await courseApiClient.GetAsync($"api/enrollments/students/{studentId}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Log.Information($"No enrollments found for student id {studentId}");
                await _studentRepository.DeleteStudentAsync(student);
                return new ServiceResult(ResultType.Ok, "Delete student successfully");
            }
            else if (response.IsSuccessStatusCode)
            {
                Log.Error($"Delete student with id {studentId} failed. Student has enrollments");
                return new ServiceResult(ResultType.BadRequest, "Student has enrollments");
            }
            else
            {
                Log.Error($"Failed to delete student with id {studentId} from CourseApi: {response.StatusCode}");
                return new ServiceResult(ResultType.InternalServerError, "Error deleting student from CourseApi");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from CourseApi: {e.Message}");
            return new ServiceResult(ResultType.InternalServerError, "Error retrieving students from CourseApi");
        }
    }
    public async Task<ServiceResult> GetStudentByEmailAsync(string email)
    {
        var student = await _studentRepository.GetStudentByEmailAsync(email);
        if (student == null)
        {
            Log.Error($"Get student by email {email} failed. Student not found");
            return new ServiceResult(ResultType.NotFound, "Student not found");
        }
        return new ServiceResult(_mapper.Map<StudentDto>(student), "Get student by email successfully");
    }
    public async Task<ServiceResult> GetStudentByIdAsync(int studentId)
    {
        var student = await _studentRepository.GetStudentByIdAsync(studentId);
        if (student == null)
        {
            Log.Error($"Get student by id {studentId} failed. Student not found");
            return new ServiceResult(ResultType.NotFound, "Student not found");
        }
        return new ServiceResult(_mapper.Map<StudentDto>(student), "Get student by id successfully");
    }
    public async Task<ServiceResult> GetStudentsAsync(StudentQuery query)
    {
        var students = await _studentRepository.GetAllStudentsAsync(query);
        if (students == null || students.Count == 0)
        {
            Log.Error("Get list students failed. Students not found");
            return new ServiceResult(ResultType.NotFound, "Students not found");
        }
        return new ServiceResult(_mapper.Map<List<StudentDto>>(students), "Get list students successfully");
    }
    public async Task<ServiceResult> GetStudentsByIdsAsync(List<int> ids)
    {
        var students = await _studentRepository.GetStudentsByIdsAsync(ids);
        if (students == null || students.Count == 0)
        {
            Log.Error("Get list students by ids failed. Students not found");
            return new ServiceResult(ResultType.NotFound, "Students not found");
        }
        return new ServiceResult(_mapper.Map<List<StudentDto>>(students), "Get list students by ids successfully");
    }
    public async Task<ServiceResult> UpdateStudentAsync(int id, UpdateStudentDto updatedStudentDto)
    {
        if (id != updatedStudentDto.StudentId)
        {
            Log.Error("Id in UpdateStudentDto does not match the id in the URL");
            return new ServiceResult(ResultType.BadRequest, "Id in UpdateStudentDto does not match the id in the URL");
        }

        var existingStudent = await _studentRepository.GetStudentByIdAsync(id);
        if (existingStudent == null)
        {
            Log.Error($"Update student with id {id} failed. Student not found");
            return new ServiceResult(ResultType.NotFound, "Student not found");
        }
        if (await _studentRepository.GetStudentByEmailAsync(updatedStudentDto.Email) != existingStudent)
        {
            Log.Error($"Update student failed. Student with email {updatedStudentDto.Email} already exists");
            return new ServiceResult(ResultType.BadRequest, $"Student with email {updatedStudentDto.Email} already exists");
        }
        if (await _studentRepository.GetStudentByPhoneNumberAsync(updatedStudentDto.PhoneNumber) != existingStudent)
        {
            Log.Error($"Update student failed. Student with phoneNumber {updatedStudentDto.PhoneNumber} already exists");
            return new ServiceResult(ResultType.BadRequest, $"Student with phoneNumber {updatedStudentDto.PhoneNumber} already exists");
        }
        _mapper.Map(updatedStudentDto, existingStudent);

        var result = await _studentRepository.UpdateStudentAsync(existingStudent);
        return new ServiceResult(_mapper.Map<StudentDto>(result), "Update student successfully");
    }

}