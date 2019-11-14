using AutoMapper;

using Library.API.Entities;
using Library.API.Models;

namespace Library.API.Profiles
{
    public class CoursesProfile : Profile
    {
        public CoursesProfile ()
        {
            // Used for GET actions
            CreateMap<Course, CourseDto> ();

            // Used for POST, PUT, PATCH actions respectively
            CreateMap<CourseForCreationDto, Course> ();
            CreateMap<CourseForUpdateDto, Course> ();
            CreateMap<Course, CourseForUpdateDto> ();
        }
    }
}
