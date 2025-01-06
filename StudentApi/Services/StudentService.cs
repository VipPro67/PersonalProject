using System.Net;
using AutoMapper;
using Grpc.Net.Client;
using Serilog;
using StudentApi.DTOs;
using StudentApi.Helpers;
using StudentApi.Models;
using StudentApi.Protos;
using StudentApi.Repositories;
namespace StudentApi.Services;
public interface IStudentService
{
    Task<ServiceResult<List<StudentDto>>> GetStudentsAsync(StudentQuery query);

    Task<ServiceResult<StudentDto>> GetStudentByIdAsync(int id);
    Task<ServiceResult<List<StudentDto>>> GetStudentsByIdsAsync(List<int> ids);
    Task<ServiceResult<StudentDto>> GetStudentByEmailAsync(string email);
    Task<ServiceResult<StudentDto>> CreateStudentAsync(CreateStudentDto student);
    Task<ServiceResult<StudentDto>> UpdateStudentAsync(int id, UpdateStudentDto updatedStudent);
    Task<ServiceResult<bool>> DeleteStudentAsync(int studentId);
}

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;

    private readonly EnrollmentService.EnrollmentServiceClient _enrollmentServiceClient;
    private readonly IMapper _mapper;

    public StudentService(IStudentRepository studentRepository, IMapper mapper, EnrollmentService.EnrollmentServiceClient enrollmentServiceClient)
    {
        _studentRepository = studentRepository;
        _mapper = mapper;
        _enrollmentServiceClient = enrollmentServiceClient;
    }
    public async Task<ServiceResult<StudentDto>> CreateStudentAsync(CreateStudentDto createStudentDto)
    {
        if (await _studentRepository.GetStudentByEmailAsync(createStudentDto.Email) != null)
        {
            Log.Error($"Create student failed. Student with email {createStudentDto.Email} already exists");
            return new ServiceResult<StudentDto>(ResultType.BadRequest, $"Student with email {createStudentDto.Email} already exists");
        }
        if (await _studentRepository.GetStudentByPhoneNumberAsync(createStudentDto.PhoneNumber) != null)
        {
            Log.Error($"Create student failed. Student with phoneNumber {createStudentDto.PhoneNumber} already exists");
            return new ServiceResult<StudentDto>(ResultType.BadRequest, $"Student with phoneNumber {createStudentDto.PhoneNumber} already exists");
        }
        var result = await _studentRepository.CreateStudentAsync(_mapper.Map<Student>(createStudentDto));
        return new ServiceResult<StudentDto>(_mapper.Map<StudentDto>(result), "Create student successfully");
    }
    public async Task<ServiceResult<bool>> DeleteStudentAsync(int studentId)
    {
        var student = await _studentRepository.GetStudentByIdAsync(studentId);
        if (student == null)
        {
            Log.Error($"Delete student with id {studentId} failed. Student not found");
            return new ServiceResult<bool>(ResultType.NotFound, "Student not found");
        }
        try
        {
            var request = new CheckStudentEnrollmentRequest { StudentId = studentId };
            var response = await _enrollmentServiceClient.CheckStudentEnrollmentAsync(request);
            if (response.IsEnrolled)  //this return true false
            {
                Log.Error($"Delete student with id {studentId} failed. Student is enrolled in a course");
                return new ServiceResult<bool>(ResultType.BadRequest, "Student is enrolled in a course");
            }
            await _studentRepository.DeleteStudentAsync(student);
            return new ServiceResult<bool>(ResultType.Ok, "Delete student successfully");
        }
        catch (Exception e)
        {
            Log.Error($"Error retrieving students from CourseApi: {e.Message}");
            return new ServiceResult<bool>(ResultType.InternalServerError, "Error retrieving students from CourseApi");
        }
    }
    public async Task<ServiceResult<StudentDto>> GetStudentByEmailAsync(string email)
    {
        var student = await _studentRepository.GetStudentByEmailAsync(email);
        if (student == null)
        {
            Log.Error($"Get student by email {email} failed. Student not found");
            return new ServiceResult<StudentDto>(ResultType.NotFound, "Student not found");
        }
        return new ServiceResult<StudentDto>(_mapper.Map<StudentDto>(student), "Get student by email successfully");
    }
    public async Task<ServiceResult<StudentDto>> GetStudentByIdAsync(int studentId)
    {
        var student = await _studentRepository.GetStudentByIdAsync(studentId);
        if (student == null)
        {
            Log.Error($"Get student by id {studentId} failed. Student not found");
            return new ServiceResult<StudentDto>(ResultType.NotFound, "Student not found");
        }
        return new ServiceResult<StudentDto>(_mapper.Map<StudentDto>(student), "Get student by id successfully");
    }
    public async Task<ServiceResult<List<StudentDto>>> GetStudentsAsync(StudentQuery query)
    {
        var students = await _studentRepository.GetAllStudentsAsync(query);
        if (students == null || students.Count == 0)
        {
            Log.Error("Get list students failed. Students not found");
            return new ServiceResult<List<StudentDto>>(ResultType.NotFound, "Students not found");
        }
        int totalItems = await _studentRepository.GetTotalStudentsAsync(query);
        var pagination = new Pagination
        {
            TotalItems = totalItems,
            CurrentPage = query.Page.Value,
            TotalPage = (int)Math.Ceiling(totalItems / (double)query.ItemsPerPage),
            ItemsPerPage = query.ItemsPerPage.Value
        };

        return new ServiceResult<List<StudentDto>>(_mapper.Map<List<StudentDto>>(students), "Get list students successfully", pagination);
    }
    public async Task<ServiceResult<List<StudentDto>>> GetStudentsByIdsAsync(List<int> ids)
    {
        var students = await _studentRepository.GetStudentsByIdsAsync(ids);
        if (students == null || students.Count == 0)
        {
            Log.Error("Get list students by ids failed. Students not found");
            return new ServiceResult<List<StudentDto>>(ResultType.NotFound, "Students not found");
        }
        return new ServiceResult<List<StudentDto>>(_mapper.Map<List<StudentDto>>(students), "Get list students by ids successfully");
    }
    public async Task<ServiceResult<StudentDto>> UpdateStudentAsync(int id, UpdateStudentDto updatedStudentDto)
    {
        if (id != updatedStudentDto.StudentId)
        {
            Log.Error("Id in UpdateStudentDto does not match the id in the URL");
            return new ServiceResult<StudentDto>(ResultType.BadRequest, "Id in UpdateStudentDto does not match the id in the URL");
        }

        var existingStudent = await _studentRepository.GetStudentByIdAsync(id);
        if (existingStudent == null)
        {
            Log.Error($"Update student with id {id} failed. Student not found");
            return new ServiceResult<StudentDto>(ResultType.NotFound, "Student not found");
        }
        var studentByEmail = await _studentRepository.GetStudentByEmailAsync(updatedStudentDto.Email);
        if (studentByEmail != null && studentByEmail.StudentId != existingStudent.StudentId)
        {
            Log.Error($"Update student failed. Student with email {updatedStudentDto.Email} already exists");
            return new ServiceResult<StudentDto>(ResultType.BadRequest, $"Student with email {updatedStudentDto.Email} already exists");
        }
        var studentByPhoneNumber = await _studentRepository.GetStudentByPhoneNumberAsync(updatedStudentDto.PhoneNumber);
        if (studentByPhoneNumber != null && studentByPhoneNumber.StudentId != existingStudent.StudentId)
        {
            Log.Error($"Update student failed. Student with phoneNumber {updatedStudentDto.PhoneNumber} already exists");
            return new ServiceResult<StudentDto>(ResultType.BadRequest, $"Student with phoneNumber {updatedStudentDto.PhoneNumber} already exists");
        }
        _mapper.Map(updatedStudentDto, existingStudent);

        var result = await _studentRepository.UpdateStudentAsync(existingStudent);
        return new ServiceResult<StudentDto>(_mapper.Map<StudentDto>(result), "Update student successfully");
    }

}