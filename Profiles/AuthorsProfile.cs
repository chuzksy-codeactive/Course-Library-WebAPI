using AutoMapper;

using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;

namespace Library.API.Profiles
{
    public class AuthorsProfile : Profile
    {
        public AuthorsProfile ()
        {
            CreateMap<Author, AuthorDto> ()
                .ForMember (
                    dest => dest.Name,
                    opt => opt.MapFrom (src => $"{src.FirstName} {src.LastName}"))
                .ForMember (
                    dest => dest.Age,
                    opt => opt.MapFrom (src => src.DateOfBirth.GetCurrentAge ()));

            CreateMap<AuthorForCreationDto, Author>();
            CreateMap<Author, AuthorFullDto>();
        }
    }
}
