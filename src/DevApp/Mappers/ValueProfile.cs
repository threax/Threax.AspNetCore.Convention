using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Threax.AspNetCore.Models;
using Threax.AspNetCore.Tracking;
using DevApp.InputModels;
using DevApp.Database;
using DevApp.ViewModels;

namespace DevApp.Mappers
{
    public partial class ValueProfile : Profile
    {
        public ValueProfile()
        {
            //Map the input model to the entity
            CreateMap<ValueInput, ValueEntity>()
                .ForMember(d => d.ValueId, opt => opt.Ignore())
                .ForMember(d => d.Created, opt => opt.ResolveUsing<ICreatedResolver>())
                .ForMember(d => d.Modified, opt => opt.ResolveUsing<IModifiedResolver>());

            //Map the entity to the view model.
            CreateMap<ValueEntity, Value>();
        }
    }
}