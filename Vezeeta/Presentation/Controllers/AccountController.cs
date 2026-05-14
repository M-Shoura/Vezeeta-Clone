using Application.DTOs.Profiles;
using Application.DTOs.Profiles.Patients;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModels.Accounts;

namespace Presentation.Controllers
{
    public class AccountController : Controller
    {
        private readonly IPatientProfileService _profileService;
        public AccountController(
            IPatientProfileService profileService
            )
        {
            _profileService = profileService;
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var profile = await _profileService
                .GetCurrentPatientProfileAsync();
            
            if (profile == null)
            {
                return View(
                    new ProfileViewModel()
                    {
                        FullName = "Mohamed Atef",
                        Email = "mohamed.atef@up.edu.eg",
                        PhoneNumber = "+201012345678",
                        BirthDate = DateTime.Now
                    }
                );
            }

            var viewModel = new ProfileViewModel()
            {
                FullName = profile.FullName,
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                BirthDate = profile.BirthDate
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new PatientProfileDto()
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate
            };
            
            
            await _profileService
                .UpdatePatientProfileAsync(dto);

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new ChangePasswordDto()
            {
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword
            };
        
            await _profileService
                .ChangePasswordAsync(dto);

            return View(model);
        }
    }
}