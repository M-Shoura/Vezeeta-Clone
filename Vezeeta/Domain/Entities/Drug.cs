using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Drug : AuditableEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? GenericName { get; set; }
        public string? Manufacturer { get; set; }
        public string? Strength { get; set; }
        public string? SideEffects { get; set; }
        
        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new HashSet<PrescriptionItem>();
    }
}
