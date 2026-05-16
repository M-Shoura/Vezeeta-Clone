using Application.DTOs.Drugs;
using Application.DTOs.Medical_Records;
using Application.DTOs.Prescriptions;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Drug, DrugDto>().ReverseMap();
            CreateMap<Drug, DrugFilterDto>().ReverseMap();

            CreateMap<Prescription, PrescriptionDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.PrescriptionItems))
                .ReverseMap()
                .ForMember(dest => dest.Appointment, opt => opt.Ignore())
                .ForMember(dest => dest.PrescriptionItems, opt => opt.Ignore());

            CreateMap<PrescriptionItem, PrescriptionItemDto>()
                .ForMember(dest => dest.DrugName, opt => opt.MapFrom(src => src.Drug != null ? src.Drug.Name : null))
                .ReverseMap()
                .ForMember(dest => dest.Prescription, opt => opt.Ignore())
                .ForMember(dest => dest.Drug, opt => opt.Ignore());

            CreateMap<MedicalRecord, MedicalRecordDto>().ReverseMap();
        }
    }
}
