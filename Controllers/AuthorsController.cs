using System;
using System.Collections.Generic;
using System.Text.Json;
using AutoMapper;

using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.ResourceParameters;
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

        [HttpGet (Name = "GetAuthors")]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors ([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            var authorsFromRepo = _libraryRepository.GetAuthors (authorsResourceParameters);

            var previousPageLink = authorsFromRepo.HasPrevious ?
                CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage) : 
                null;

            var nextPageLink = authorsFromRepo.HasNext ?
                CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage) :
                null;

            var paginationMetadata = new 
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok (_mapper.Map<IEnumerable<AuthorDto>> (authorsFromRepo));
        }

        [HttpGet ("{authorId}", Name = "GetAuthor")]
        public ActionResult<AuthorDto> GetAuthor (Guid authorId)
        {
            var authorFromRepo = _libraryRepository.GetAuthor (authorId);

            if (authorFromRepo == null)
            {
                return NotFound ();
            }

            return Ok (_mapper.Map<AuthorDto> (authorFromRepo));
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor (AuthorForCreationDto authorForCreationDto)
        {
            var authorEntity = _mapper.Map<Author> (authorForCreationDto);
            _libraryRepository.AddAuthor (authorEntity);
            _libraryRepository.Save ();

            var authorToReturn = _mapper.Map<AuthorDto> (authorEntity);
            return CreatedAtRoute ("GetAuthor", new { authorId = authorToReturn.Id }, authorToReturn);
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions ()
        {
            Response.Headers.Add ("Allow", "GET,OPTIONS,POST");
            return Ok ();
        }

        [HttpDelete ("{authorId}")]
        public ActionResult DeleteAuthor (Guid authorId)
        {
            var authorFromRepo = _libraryRepository.GetAuthor (authorId);

            if (authorFromRepo == null)
            {
                return NotFound ();
            }

            _libraryRepository.DeleteAuthor (authorFromRepo);

            _libraryRepository.Save ();

            return NoContent ();
        }

        private string CreateAuthorsResourceUri (
            AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link ("GetAuthors",
                        new
                        {
                            orderBy = authorsResourceParameters.OrderBy,
                            pageNumber = authorsResourceParameters.PageNumber - 1,
                            pageSize = authorsResourceParameters.PageSize,
                            mainCategory = authorsResourceParameters.MainCategory,
                            searchQuery = authorsResourceParameters.SearchQuery
                        });
                case ResourceUriType.NextPage:
                    return Url.Link ("GetAuthors",
                        new
                        {
                            orderBy = authorsResourceParameters.OrderBy,
                            pageNumber = authorsResourceParameters.PageNumber + 1,
                            pageSize = authorsResourceParameters.PageSize,
                            mainCategory = authorsResourceParameters.MainCategory,
                            searchQuery = authorsResourceParameters.SearchQuery
                        });
                default:
                    return Url.Link ("GetAuthors",
                        new
                        {
                            orderBy = authorsResourceParameters.OrderBy,
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize,
                            mainCategory = authorsResourceParameters.MainCategory,
                            searchQuery = authorsResourceParameters.SearchQuery
                        });
            }

        }
    }
}
