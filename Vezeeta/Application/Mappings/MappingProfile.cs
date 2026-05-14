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

            CreateMap<Prescription, PrescriptionDto>().ReverseMap();
            CreateMap<PrescriptionItem, PrescriptionItemDto>().ReverseMap();

            CreateMap<MedicalRecord, MedicalRecordDto>().ReverseMap();
        }
    }
}
