using System;
using System.Collections.Generic;

using AutoMapper;

using Library.API.Models;
using Library.API.Services;

using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route ("api/authors/{authorId}/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMapper _mapper;

        public CoursesController (ILibraryRepository libraryRepository, IMapper mapper)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor (Guid authorId)
        {
            if (!_libraryRepository.AuthorExists (authorId))
            {
                return NotFound ();
            }

            var coursesForAuthorFromRepo = _libraryRepository.GetCourses (authorId);

            if (coursesForAuthorFromRepo == null)
            {
                return NotFound ();
            }

            return Ok (_mapper.Map<IEnumerable<CourseDto>> (coursesForAuthorFromRepo));
        }

        [HttpGet("{id}")]
        public ActionResult<CourseDto> GetCourseForAuthor (Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists (authorId))
            {
                return NotFound ();
            }

            var courseForAuthorFromRepo = _libraryRepository.GetCourse (authorId, id);

            if (courseForAuthorFromRepo == null)
            {
                return NotFound ();
            }

            return Ok (_mapper.Map<CourseDto>(courseForAuthorFromRepo));
        }
    }
}
