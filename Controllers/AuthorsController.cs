using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using AutoMapper;

using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.ResourceParameters;
using Library.API.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Library.API.Controllers
{
    [ApiController]
    [Route ("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;
        private readonly IMapper _mapper;

        public AuthorsController (
            ILibraryRepository libraryRepository,
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService,
            IMapper mapper)
        {
            _libraryRepository = libraryRepository ??
                throw new ArgumentNullException (nameof (libraryRepository));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException (nameof (propertyMappingService));
            _propertyCheckerService = propertyCheckerService ??
                throw new ArgumentNullException (nameof (propertyCheckerService));
            _mapper = mapper ??
                throw new ArgumentException (nameof (mapper));
        }

        [HttpGet (Name = "GetAuthors")]
        public IActionResult GetAuthors ([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>
                (authorsResourceParameters.OrderBy))
            {
                return BadRequest ();
            }

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto> (authorsResourceParameters.Fields))
            {
                return BadRequest ();
            }

            var authorsFromRepo = _libraryRepository.GetAuthors (authorsResourceParameters);

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
            };

            Response.Headers.Add ("X-Pagination", JsonSerializer.Serialize (paginationMetadata));

            var links = CreateLinksForAuthors (authorsResourceParameters, authorsFromRepo.HasNext, authorsFromRepo.HasPrevious);
            var shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>> (authorsFromRepo).ShapeData (authorsResourceParameters.Fields);

            var shapedAuthorsWithLinks = shapedAuthors.Select (author =>
            {
                var authorAsDictionary = author as IDictionary<string, object>;
                var authorLinks = CreateLinksForAuthor ((Guid) authorAsDictionary["Id"], null);
                authorAsDictionary.Add ("links", authorLinks);
                return authorAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links
            };

            return Ok (linkedCollectionResource);
        }

        [Produces ("application/json",
            "application/vnd.marvin.hateoas+json",
            "application/vnd.marvin.author.full+json",
            "application/vnd.marvin.author.full.hateoas+json",
            "application/vnd.marvin.author.friendly+json",
            "application/vnd.marvin.author.friendly.hateoas+json")]
        [HttpGet ("{authorId}", Name = "GetAuthor")]
        public ActionResult<AuthorDto> GetAuthor (Guid authorId, string fields, [FromHeader (Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse (mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest ();
            }

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto> (fields))
            {
                return BadRequest ();
            }

            var authorFromRepo = _libraryRepository.GetAuthor (authorId);

            if (authorFromRepo == null)
            {
                return NotFound ();
            }

            var includeLinks = parsedMediaType.SubTypeWithoutSuffix
                .EndsWith ("hateoas", StringComparison.InvariantCultureIgnoreCase);

            IEnumerable<LinkDto> links = new List<LinkDto> ();

            if (includeLinks)
            {
                links = CreateLinksForAuthor (authorId, fields);
            }

            var primaryMediaType = includeLinks ?
                parsedMediaType.SubTypeWithoutSuffix
                .Substring (0, parsedMediaType.SubTypeWithoutSuffix.Length - 8) :
                parsedMediaType.SubTypeWithoutSuffix;

            // full author
            if (primaryMediaType == "vnd.marvin.author.full")
            {
                var fullResourceToReturn = _mapper.Map<AuthorFullDto> (authorFromRepo)
                    .ShapeData (fields) as IDictionary<string, object>;

                if (includeLinks)
                {
                    fullResourceToReturn.Add ("links", links);
                }

                return Ok (fullResourceToReturn);
            }

            // friendly author
            var friendlyResourceToReturn = _mapper.Map<AuthorDto> (authorFromRepo)
                .ShapeData (fields) as IDictionary<string, object>;

            if (includeLinks)
            {
                friendlyResourceToReturn.Add ("links", links);
            }

            return Ok (friendlyResourceToReturn);
        }

        [HttpPost (Name = "CreateAuthor")]
        public ActionResult<AuthorDto> CreateAuthor (AuthorForCreationDto authorForCreationDto)
        {
            var authorEntity = _mapper.Map<Author> (authorForCreationDto);
            _libraryRepository.AddAuthor (authorEntity);
            _libraryRepository.Save ();

            var authorToReturn = _mapper.Map<AuthorDto> (authorEntity);

            var links = CreateLinksForAuthor (authorToReturn.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData (null)
            as IDictionary<string, object>;

            linkedResourceToReturn.Add ("links", links);

            return CreatedAtRoute ("GetAuthor", new { authorId = linkedResourceToReturn["Id"] }, linkedResourceToReturn);
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions ()
        {
            Response.Headers.Add ("Allow", "GET,OPTIONS,POST");
            return Ok ();
        }

        [HttpDelete ("{authorId}", Name = "DeleteAuthor")]
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

        private string CreateAuthorsResourceUri (AuthorsResourceParameters authorsResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
            case ResourceUriType.PreviousPage:
                return Url.Link ("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
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
                        fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            pageNumber = authorsResourceParameters.PageNumber + 1,
                            pageSize = authorsResourceParameters.PageSize,
                            mainCategory = authorsResourceParameters.MainCategory,
                            searchQuery = authorsResourceParameters.SearchQuery
                    });
            case ResourceUriType.Current:
            default:
                return Url.Link ("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize,
                            mainCategory = authorsResourceParameters.MainCategory,
                            searchQuery = authorsResourceParameters.SearchQuery
                    });
            }

        }

        private IEnumerable<LinkDto> CreateLinksForAuthor (Guid authorId, string fields)
        {
            var links = new List<LinkDto> ();

            if (string.IsNullOrWhiteSpace (fields))
            {
                links.Add (
                    new LinkDto (Url.Link ("GetAuthor", new { authorId }),
                        "self",
                        "GET"));
            }
            else
            {
                links.Add (
                    new LinkDto (Url.Link ("GetAuthor", new { authorId, fields }),
                        "self",
                        "GET"));
            }
            links.Add (
                new LinkDto (Url.Link ("DeleteAuthor", new { authorId }),
                    "delete_author",
                    "DELETE"));
            links.Add (
                new LinkDto (Url.Link ("CreateCourseForAuthor", new { authorId }),
                    "create_course_for_author",
                    "POST"));
            links.Add (
                new LinkDto (Url.Link ("GetCoursesForAuthor", new { authorId }),
                    "courses",
                    "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors (AuthorsResourceParameters authorsResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto> ();

            // self 
            links.Add (
                new LinkDto (CreateAuthorsResourceUri (
                    authorsResourceParameters, ResourceUriType.Current), "self", "GET"));
            if (hasNext)
            {
                links.Add (
                    new LinkDto (CreateAuthorsResourceUri (authorsResourceParameters, ResourceUriType.NextPage),
                        "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add (
                    new LinkDto (CreateAuthorsResourceUri (authorsResourceParameters, ResourceUriType.PreviousPage),
                        "previousPage", "GET"));
            }

            return links;
        }
    }
}
