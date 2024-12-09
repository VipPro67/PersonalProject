using AutoMapper;
using Newtonsoft.Json;
using Serilog;
using StudentApi.DTOs;
using StudentApi.Helpers;
using StudentApi.Models;
using StudentApi.Repositories;

namespace StudentApi.Services;
public interface IStudentService
{
    Task<Student?> GetStudentByIdAsync(int id);
    Task<List<Student>?> GetStudentsByIdsAsync(List<int> ids);
    Task<Student?> GetStudentByEmailAsync(string email);
    Task<Student?> CreateStudentAsync(CreateStudentDto student);
    Task<List<Student>> GetStudentsAsync(StudentQuery query);
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
            Log.Error($"Create student failed. Student with email {createStudentDto.Email} already exists");
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
            Log.Error($"Detele student with id {studentId} failed. Student not found");
            return false;
        }
        try
        {
            var courseApiUrl = Environment.GetEnvironmentVariable("CourseApiUrl");
            var courseApiClient = new HttpClient { BaseAddress = new Uri(courseApiUrl) };
            var response = await courseApiClient.GetAsync($"api/enrollments/students/{studentId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"Failed to retrieve enrollments with studentId {studentId} from StudentApi: {response.StatusCode}");
                return false;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<EnrollmentDto>>>(responseContent);
            if (apiResponse.Data.Any(e => e.StudentId == studentId))
            {
                Log.Error($"Detele student with id {studentId} failed. Student are in course");
                return false;
            }

            return await _studentRepository.DeleteStudentAsync(student);
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from StudentApi: {e.Message}");
            return false;
        }
    }

    public async Task<Student?> GetStudentByEmailAsync(string email)
    {
        return await _studentRepository.GetStudentByEmailAsync(email);
    }

    public async Task<Student?> GetStudentByIdAsync(int studentId)
    {
        return await _studentRepository.GetStudentByIdAsync(studentId);
    }

    public Task<List<Student>> GetStudentsAsync(StudentQuery query)
    {
        return _studentRepository.GetAllStudentsAsync(query);
    }

    public async Task<List<Student>?> GetStudentsByIdsAsync(List<int> ids)
    {
        return await _studentRepository.GetStudentsByIdsAsync(ids);
    }

    public async Task<Student?> UpdateStudentAsync(int id, UpdateStudentDto updatedStudentDto)
    {
        if (id != updatedStudentDto.StudentId)
        {
            Log.Error("Id in UpdateStudentDto does not match the id in the URL");
            return null;
        }
        var existingStudent = await _studentRepository.GetStudentByIdAsync(id);
        if (existingStudent == null)
        {
            Log.Error($"Update student with id {id} failed. Student not found");
            return null;
        }
        _mapper.Map(updatedStudentDto, existingStudent);
        return await _studentRepository.UpdateStudentAsync(existingStudent);
    }
}