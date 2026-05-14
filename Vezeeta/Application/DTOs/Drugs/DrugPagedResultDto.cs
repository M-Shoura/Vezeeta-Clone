using System.Collections.Generic;

namespace Application.DTOs.Drugs
{
    public class DrugPagedResultDto
    {
        public IEnumerable<DrugDto> Items { get; set; } = new List<DrugDto>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);
        public string? Search { get; set; }
    }
}
