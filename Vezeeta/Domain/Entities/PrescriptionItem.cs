using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class PrescriptionItem : BaseEntity
    {
        public int PrescriptionId { get; set; }
        public int? DrugId { get; set; }
        public string Dosage { get; set; } = null!;
        public int DurationInDays { get; set; }
        public int TimesPerDay { get; set; }
        public string? Instructions { get; set; }

        public Prescription Prescription { get; set; } = null!;
        public Drug? Drug { get; set; } = null!;
    }
}
