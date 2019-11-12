using AutoMapper;

using Library.API.Entities;
using Library.API.Models;

namespace Library.API.Profiles
{
    public class CoursesProfile : Profile
    {
        public CoursesProfile ()
        {
            CreateMap<Course, CourseDto> ();
            CreateMap<CourseForCreationDto, Course>();
        }
    }
}
