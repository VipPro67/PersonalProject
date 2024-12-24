using AutoMapper;
using StudentApi.DTOs;
using StudentApi.Models;
using StudentApi.Helpers;
namespace StudentApi.Mappings;
public class StudentMappingProfile : Profile
{
    public StudentMappingProfile()
    {
        CreateMap<CreateStudentDto, Student>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => HandleHTML.SanitizeAndEncodeHTML(src.Address)));

        CreateMap<UpdateStudentDto, Student>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => HandleHTML.SanitizeAndEncodeHTML(src.Address)));

        CreateMap<Student, StudentDto>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => HandleHTML.DecodeHTMLEntities(src.Address)));
    }
}