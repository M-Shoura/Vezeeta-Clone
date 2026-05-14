namespace Application.DTOs.Prescriptions
{
    public class PrescriptionItemDto
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public int DrugId { get; set; }
        public string Dosage { get; set; } = null!;
        public int DurationInDays { get; set; }
        public int TimesPerDay { get; set; }
        public string? Instructions { get; set; }

        // For display
        public string? DrugName { get; set; }
    }
}
