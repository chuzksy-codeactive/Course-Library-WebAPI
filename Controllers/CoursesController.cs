using System;
using System.Collections.Generic;

using AutoMapper;

using Library.API.Entities;
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

        [HttpGet ("{id}", Name = "GetCourseForAuthor")]
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

            return Ok (_mapper.Map<CourseDto> (courseForAuthorFromRepo));
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor (Guid authorId, CourseForCreationDto course)
        {
            if (!_libraryRepository.AuthorExists (authorId))
            {
                return NotFound ();
            }
            var courseEntity = _mapper.Map<Course> (course);
            _libraryRepository.AddCourse (authorId, courseEntity);
            _libraryRepository.Save ();

            var courseToReturn = _mapper.Map<CourseDto> (courseEntity);
            return CreatedAtRoute ("GetCourseForAuthor", new { authorId, id = courseToReturn.Id }, courseToReturn);
        }

        [HttpPut ("{courseId}")]
        public IActionResult UpdateCourseForAuthor (Guid authorId, Guid courseId, CourseForUpdateDto course)
        {
            if (!_libraryRepository.AuthorExists (authorId))
            {
                return NotFound ();
            }

            var courseForAuthorFromRepo = _libraryRepository.GetCourse (authorId, courseId);

            if (courseForAuthorFromRepo == null)
            {
                var courseToAdd = _mapper.Map<Course> (course);
                courseToAdd.Id = courseId;

                _libraryRepository.AddCourse (authorId, courseToAdd);

                _libraryRepository.Save ();

                var courseToReturn = _mapper.Map<CourseDto> (courseToAdd);

                return CreatedAtRoute ("GetCourseForAuthor",
                    new { authorId, id = courseToReturn.Id },
                    courseToReturn);
            }

            // map the entity to the courseForUpdateDto
            // apply the updated fields value to that Dto
            // map the courseForUpdateDto back to an entity
            _mapper.Map (course, courseForAuthorFromRepo);

            _libraryRepository.UpdateCourse (courseForAuthorFromRepo);
            _libraryRepository.Save ();

            return NoContent ();
        }
    }
}
