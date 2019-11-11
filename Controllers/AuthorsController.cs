using System;
using System.Collections.Generic;

using AutoMapper;

using Library.API.Helpser;
using Library.API.Models;
using Library.API.Services;

using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route ("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMapper _mapper;

        public AuthorsController (ILibraryRepository libraryRepository, IMapper mapper)
        {
            _libraryRepository = libraryRepository ??
                throw new ArgumentNullException (nameof (libraryRepository));
            _mapper = mapper ??
                throw new ArgumentException (nameof (mapper));
        }

        [HttpGet ()]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors ()
        {
            var authorsFromRepo = _libraryRepository.GetAuthors ();

            return Ok (_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
        }

        [HttpGet ("{authorId}")]
        public ActionResult<AuthorDto> GetAuthor (Guid authorId)
        {
            var authorFromRepo = _libraryRepository.GetAuthor (authorId);

            if (authorFromRepo == null)
            {
                return NotFound ();
            }

            return Ok (_mapper.Map<AuthorDto>(authorFromRepo));
        }
    }
}
