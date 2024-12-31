using System.Net;
using AutoMapper;
using Grpc.Core;
using Serilog;
using StudentApi.DTOs;
using StudentApi.Helpers;
using StudentApi.Models;
using StudentApi.Protos;
using StudentApi.Repositories;
namespace StudentApi.Services;
public class GrpcStudentService : Protos.StudentService.StudentServiceBase
{
    private readonly IStudentService _studentService;

    public GrpcStudentService(IStudentService studentService)
    {
        _studentService = studentService;
    }

    public override async Task<StudentsResponse> GetStudentsByIds(GetStudentsByIdsRequest request, ServerCallContext context)
    {
        var result = await _studentService.GetStudentsByIdsAsync(request.StudentIds.ToList());
        if (result.Type == ResultType.NotFound)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Students not found"));
        }
        var students = (List<StudentDto>)result.Data;
        var response = new StudentsResponse();
        response.Students.AddRange(students.Select(s => new StudentResponse
        {
            StudentId = s.StudentId,
            Name = s.FullName,
            Email = s.Email,
            PhoneNumber = s.PhoneNumber
        }));
        return response;
    }
    public override async Task<StudentResponse> GetStudentById(GetStudentByIdRequest request, ServerCallContext context)
    {
        var result = await _studentService.GetStudentByIdAsync(request.StudentId);
        if (result == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Student not found"));
        }
        var student = (StudentDto)result.Data;
        var response = new StudentResponse();
        response.StudentId = student.StudentId;
        response.Name = student.FullName;
        response.Email = student.Email;
        response.PhoneNumber = student.PhoneNumber;
        return response;
    }
}
