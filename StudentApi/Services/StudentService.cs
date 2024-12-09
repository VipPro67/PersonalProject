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
    Task<ServiceResult<StudentDto?>> GetStudentByIdAsync(int id);
    Task<ServiceResult<List<StudentDto>?>> GetStudentsByIdsAsync(List<int> ids);
    Task<ServiceResult<StudentDto?>> GetStudentByEmailAsync(string email);
    Task<ServiceResult<StudentDto?>> CreateStudentAsync(CreateStudentDto student);
    Task<ServiceResult<List<StudentDto>>> GetStudentsAsync(StudentQuery query);
    Task<ServiceResult<StudentDto?>> UpdateStudentAsync(int id, UpdateStudentDto updatedStudent);
    Task<ServiceResult<bool>> DeleteStudentAsync(int studentId);
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
    public async Task<ServiceResult<StudentDto?>> CreateStudentAsync(CreateStudentDto createStudentDto)
    {
        if (await _studentRepository.GetStudentByEmailAsync(createStudentDto.Email) != null)
        {
            Log.Error($"Create student failed. Student with email {createStudentDto.Email} already exists");
            return new ServiceResult<StudentDto?>(ResultType.BadRequest, "Student with email already exists");
        }
        var result = await _studentRepository.CreateStudentAsync(_mapper.Map<Student>(createStudentDto));
        return new ServiceResult<StudentDto?>(_mapper.Map<StudentDto?>(result), "Create student successfully");
    }

    public async Task<ServiceResult<bool>> DeleteStudentAsync(int studentId)
    {
        var student = await _studentRepository.GetStudentByIdAsync(studentId);
        if (student == null)
        {
            Log.Error($"Detele student with id {studentId} failed. Student not found");
            return new ServiceResult<bool>(ResultType.NotFound, "Student not found");
        }
        try
        {
            var courseApiUrl = Environment.GetEnvironmentVariable("CourseApiUrl");
            var courseApiClient = new HttpClient { BaseAddress = new Uri(courseApiUrl) };
            var response = await courseApiClient.GetAsync($"api/enrollments/students/{studentId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"Failed to retrieve enrollments with studentId {studentId} from CourseApi: {response.StatusCode}");
                return new ServiceResult<bool>(ResultType.BadRequest, "Failed to retrieve enrollments from CourseApi");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<EnrollmentDto>>>(responseContent);
            if (apiResponse.Data.Any(e => e.StudentId == studentId))
            {
                Log.Error($"Detele student with id {studentId} failed. Student are in course");
                return new ServiceResult<bool>(ResultType.BadRequest, "Student are in course");
            }
            var result = await _studentRepository.DeleteStudentAsync(student);
            if (!result)
            {
                Log.Error($"Detele student with id {studentId} failed.");
                return new ServiceResult<bool>(ResultType.InternalServerError, "Failed to delete student from CourseApi");
            }
            return new ServiceResult<bool>(ResultType.Ok, "Delete student successfully");

        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from CourseApi: {e.Message}");
            return new ServiceResult<bool>(ResultType.InternalServerError, "Error retrieving students from CourseApi");
        }
    }

    public async Task<ServiceResult<StudentDto?>> GetStudentByEmailAsync(string email)
    {
        var student = await _studentRepository.GetStudentByEmailAsync(email);
        return new ServiceResult<StudentDto?>(_mapper.Map<StudentDto?>(student), "Get student by email successfully");
    }

    public async Task<ServiceResult<StudentDto?>> GetStudentByIdAsync(int studentId)
    {
        var student = await _studentRepository.GetStudentByIdAsync(studentId);
        return new ServiceResult<StudentDto?>(_mapper.Map<StudentDto?>(student), "Get student by id successfully");
    }

    public async Task<ServiceResult<List<StudentDto>>> GetStudentsAsync(StudentQuery query)
    {
        var students = await _studentRepository.GetAllStudentsAsync(query);
        return new ServiceResult<List<StudentDto>>(_mapper.Map<List<StudentDto>>(students), "Get list students successfully");
    }


    public async Task<ServiceResult<List<StudentDto>?>> GetStudentsByIdsAsync(List<int> ids)
    {
        var students = await _studentRepository.GetStudentsByIdsAsync(ids);
        return new ServiceResult<List<StudentDto>?>(_mapper.Map<List<StudentDto>>(students), "Get list students by ids successfully");
    }

    public async Task<ServiceResult<StudentDto?>> UpdateStudentAsync(int id, UpdateStudentDto updatedStudentDto)
    {
        if (id != updatedStudentDto.StudentId)
        {
            Log.Error("Id in UpdateStudentDto does not match the id in the URL");
            return new ServiceResult<StudentDto?>(ResultType.BadRequest, "Id in UpdateStudentDto does not match the id in the URL");
        }
        var existingStudent = await _studentRepository.GetStudentByIdAsync(id);
        if (existingStudent == null)
        {
            Log.Error($"Update student with id {id} failed. Student not found");
            return new ServiceResult<StudentDto?>(ResultType.NotFound, "Student not found");
        }
        var result = await _studentRepository.UpdateStudentAsync(_mapper.Map<Student>(updatedStudentDto));
        return new ServiceResult<StudentDto?>(_mapper.Map<StudentDto?>(result), "Update student successfully");
    }
}