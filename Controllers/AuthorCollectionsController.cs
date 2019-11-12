using System;
using System.Collections.Generic;
using System.Linq;

using AutoMapper;

using CourseLibrary.API.Helpers;

using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;

using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route ("api/authorcollections")]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionsController (ILibraryRepository libraryReposity, IMapper mapper)
        {
            _libraryRepository = libraryReposity;
            _mapper = mapper;
        }

        [HttpGet ("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection (
            [FromRoute]
            [ModelBinder (BinderType = typeof (ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest ();
            }

            var authorEntities = _libraryRepository.GetAuthors (ids);

            if (ids.Count () != authorEntities.Count ())
            {
                return NotFound ();
            }

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>> (authorEntities);

            return Ok (authorsToReturn);
        }

        [HttpPost]
        public ActionResult<IEnumerable<AuthorDto>> CreatAuthorCollection (IEnumerable<AuthorForCreationDto> authors)
        {
            var authorEntities = _mapper.Map<IEnumerable<Author>> (authors);

            foreach (var author in authorEntities)
            {
                _libraryRepository.AddAuthor (author);
            }
            _libraryRepository.Save ();

            var authorCollectionToReturn = _mapper.Map<IEnumerable<AuthorDto>> (authorEntities);
            var idsAsString = string.Join (",", authorCollectionToReturn.Select (a => a.Id));
            return CreatedAtRoute ("GetAuthorCollection",
                new { ids = idsAsString },
                authorCollectionToReturn);
        }
    }
}
