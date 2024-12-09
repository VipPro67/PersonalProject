using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Models;

namespace CourseApi.Mappings;
public class CourseMappingProfile : Profile
{
    public CourseMappingProfile()
    {
        CreateMap<CreateCourseDto, Course>();
        CreateMap<Course, CourseDto>();
        CreateMap<UpdateCourseDto, Course>();

        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName));
        CreateMap<Enrollment, EnrollmentDetailDto>()
            .ForMember(dest => dest.Course, opt => opt.MapFrom(src => src.Course))
            .ForMember(dest => dest.Student, opt => opt.MapFrom(src => src.Student));
        CreateMap<CreateEnrollmentDto, Enrollment>();
        CreateMap<Student, StudentDto>();
    }
}


