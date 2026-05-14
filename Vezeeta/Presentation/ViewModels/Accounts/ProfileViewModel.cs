namespace Presentation.ViewModels.Accounts
{
    public class ProfileViewModel
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime BirthDate { get; set; }

        // public string Area { get; set; }
    }
}