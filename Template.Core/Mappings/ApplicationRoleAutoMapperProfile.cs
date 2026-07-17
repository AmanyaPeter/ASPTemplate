using AutoMapper;
using Template.Core.Models.Roles;
using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Mappings
{
    public class ApplicationRoleAutoMapperProfile : Profile
    {
        public ApplicationRoleAutoMapperProfile()
        {
            CreateMap<IdentityRole<Guid>, ApplicationRoleViewModel>().ReverseMap();
            //CreateMap<IdentityRole<Guid>, RoleListViewModel>();

            //CreateMap<RoleListViewModel, IdentityRole<Guid>>().ReverseMap();
            CreateMap<RoleListViewModel, IdentityRole<Guid>>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? Guid.NewGuid().ToString()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NormalizedName, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}
