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
    public async Task<ServiceResult> CreateStudentAsync(CreateStudentDto createStudentDto)
    {
        if (await _studentRepository.GetStudentByEmailAsync(createStudentDto.Email) != null)
        {
            Log.Error($"Create student failed. Student with email {createStudentDto.Email} already exists");
            return new ServiceResult(ResultType.BadRequest, "Student with email already exists");
        }
        var result = await _studentRepository.CreateStudentAsync(_mapper.Map<Student>(createStudentDto));
        return new ServiceResult(_mapper.Map<StudentDto>(result), "Create student successfully");
    }

    public async Task<ServiceResult> DeleteStudentAsync(int studentId)
    {
        var student = await _studentRepository.GetStudentByIdAsync(studentId);
        if (student == null)
        {
            Log.Error($"Detele student with id {studentId} failed. Student not found");
            return new ServiceResult(ResultType.NotFound, "Student not found");
        }
        try
        {
            var courseApiUrl = Environment.GetEnvironmentVariable("CourseApiUrl");
            var courseApiClient = new HttpClient { BaseAddress = new Uri(courseApiUrl) };
            var response = await courseApiClient.GetAsync($"api/enrollments/students/{studentId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"Failed to retrieve enrollments with studentId {studentId} from CourseApi: {response.StatusCode}");
                return new ServiceResult(ResultType.BadRequest, "Failed to retrieve enrollments from CourseApi");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<EnrollmentDto>>>(responseContent);
            if (apiResponse.Data.Any(e => e.StudentId == studentId))
            {
                Log.Error($"Detele student with id {studentId} failed. Student are in course");
                return new ServiceResult(ResultType.BadRequest, "Student are in course");
            }
            var result = await _studentRepository.DeleteStudentAsync(student);
            if (!result)
            {
                Log.Error($"Detele student with id {studentId} failed.");
                return new ServiceResult(ResultType.InternalServerError, "Failed to delete student from CourseApi");
            }
            return new ServiceResult(ResultType.Ok, "Delete student successfully");

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
        var result = await _studentRepository.UpdateStudentAsync(_mapper.Map<Student>(updatedStudentDto));
        return new ServiceResult(_mapper.Map<StudentDto>(result), "Update student successfully");
    }
}