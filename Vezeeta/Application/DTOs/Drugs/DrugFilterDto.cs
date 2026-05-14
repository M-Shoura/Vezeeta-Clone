namespace Application.DTOs.Drugs
{
    public class DrugFilterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Manufacturer { get; set; }
    }
}
