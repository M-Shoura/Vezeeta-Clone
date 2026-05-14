using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Drugs
{
    public class DrugDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? GenericName { get; set; }
        public string? Manufacturer { get; set; }
        public string? Strength { get; set; }
        public string? SideEffects { get; set; }
    }
}
