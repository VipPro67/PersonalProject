using AutoMapper;
using CourseApi.DTOs;
using CourseApi.Models;

namespace CourseApi.Mappings
{
    public class CourseMappingProfile : Profile
    {
        public CourseMappingProfile()
        {
            // Map from CreateCourseDto to Course
            CreateMap<CreateCourseDto, Course>();

            // Map from UpdateCourseDto to Course
            CreateMap<UpdateCourseDto, Course>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Map from Course to CourseDto
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? src.StartDate.Value.ToString("yyyy-MM-dd") : null))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToString("yyyy-MM-dd") : null));
        }
    }
}