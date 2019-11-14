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

            // Used for POST,PUT actions
            CreateMap<CourseForCreationDto, Course> ();
            CreateMap<CourseForUpdateDto, Course> ();
        }
    }
}
