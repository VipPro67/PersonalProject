using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Helpers;
using CourseApi.Models;
namespace CourseApi.Mappings;
public class CourseMappingProfile : Profile
{
    public CourseMappingProfile()
    {
        CreateMap<CreateCourseDto, Course>()
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId.Trim().ToUpper()))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName.Trim()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => HandleHTML.SanitizeAndEncodeHTML(src.Description)))
            .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => HandleHTML.SanitizeAndEncodeHTML(src.Schedule)));

        CreateMap<Course, CourseDto>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => HandleHTML.DecodeHTMLEntities(src.Description)))
            .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => HandleHTML.DecodeHTMLEntities(src.Schedule)));


        CreateMap<UpdateCourseDto, Course>()
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName.Trim()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => HandleHTML.SanitizeAndEncodeHTML(src.Description)))
            .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => HandleHTML.SanitizeAndEncodeHTML(src.Schedule)));

        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName));

        CreateMap<Enrollment, EnrollmentDetailDto>()
            .ForMember(dest => dest.Course, opt => opt.MapFrom(src => src.Course))
            .ForMember(dest => dest.Student, opt => opt.MapFrom(src => src.Student));

        CreateMap<CreateEnrollmentDto, Enrollment>()
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId.Trim().ToUpper()));

        CreateMap<Student, StudentDto>();
    }
}

