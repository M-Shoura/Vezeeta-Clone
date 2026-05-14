// Application/DTOs/Auth/RegisterDto.cs
using Application.Attributes;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class RegisterDto
{

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number.")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = null!;

    [StringLength(250)]
    public string? Address { get; set; }

    public IFormFile? ProfilePicture { get; set; }


    [Required(ErrorMessage = "Please select a role.")]
    public UserRole Role { get; set; }         


    [RequiredIf(nameof(Role), UserRole.Doctor, ErrorMessage = "Specialization is required for doctors.")]
    [StringLength(100)]
    public string? Specialization { get; set; }

    [RequiredIf(nameof(Role), UserRole.Doctor, ErrorMessage = "Qualification is required for doctors.")]
    [StringLength(200)]
    public string? Qualification { get; set; }

    [Range(0, 60, ErrorMessage = "Years of experience must be between 0 and 60.")]
    public int? YearsOfExperience { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    [StringLength(50)]
    public string? LicenseNumber { get; set; }


    [RequiredIf(nameof(Role), UserRole.Patient, ErrorMessage = "Date of birth is required for patients.")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [RequiredIf(nameof(Role), UserRole.Patient, ErrorMessage = "Gender is required for patients.")]
    public Gender? Gender { get; set; }

    [StringLength(3)]
    public string? BloodType { get; set; }      

    [RequiredIf(nameof(Role), UserRole.Patient, ErrorMessage = "Emergency contact name is required.")]
    [StringLength(100)]
    public string? EmergencyContactName { get; set; }

    [RequiredIf(nameof(Role), UserRole.Patient, ErrorMessage = "Emergency contact phone is required.")]
    [Phone(ErrorMessage = "Invalid emergency contact phone.")]
    public string? EmergencyContactPhone { get; set; }
}