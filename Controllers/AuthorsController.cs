using System;

using Library.API.Services;

using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route ("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorsController (ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository ??
                throw new ArgumentNullException (nameof (libraryRepository));
        }

        [HttpGet ()]
        public IActionResult GetAuthors ()
        {
            var authorsFromRepo = _libraryRepository.GetAuthors ();

            return Ok (authorsFromRepo);
        }

        [HttpGet ("{authorId}")]
        public IActionResult GetAuthor (Guid authorId)
        {
            var authorFromRepo = _libraryRepository.GetAuthor (authorId);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            return Ok (authorFromRepo);
        }
    }
}
