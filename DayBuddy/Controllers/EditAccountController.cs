using DayBuddy.Filters;
using DayBuddy.Models;
using DayBuddy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace DayBuddy.Controllers
{
    [Authorize]
    [EnableRateLimiting("GeneralPolicy")]
    public class EditAccountController : Controller
    {
        private readonly UserProfileValidatorService userProfileValidatorService;
        private readonly ProfanityFilterService profanityFilterService;
        private readonly UserManager<DayBuddyUser> userManager;

        public EditAccountController(UserProfileValidatorService userProfileValidatorService, ProfanityFilterService profanityFilterService, UserManager<DayBuddyUser> userManager)
        {
            this.userProfileValidatorService = userProfileValidatorService;
            this.profanityFilterService = profanityFilterService;
            this.userManager = userManager;
        }

        public JsonResult GetGenders()
        {
            return Json(new { genders = userProfileValidatorService.Genders });
        }
        public JsonResult GetSexualities()
        {
            return Json(new { sexualities = userProfileValidatorService.Sexualities });
        }
        public JsonResult GetInterests()
        {
            return Json(new { interests = userProfileValidatorService.Interests });
        }

        [HttpPost]
        [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
        public async Task<JsonResult> EditName([MaxLength(20, ErrorMessage = "Name too long")]
        [MinLength(5, ErrorMessage = "Name too short")] [Required]string newName)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToArray();
                return Json(new { success = false, errors = errorMessages });
            }
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if (profanityFilterService.ContainsProfanity(newName))
            {
                return Json(new { success = false, errors = new[] { "Name cannot contain profanity" } });
            }

            user.UserName = newName;
            user.NormalizedUserName = userManager.NormalizeName(newName);

            await userManager.UpdateAsync(user);

            return Json(new { success = true, errors = Array.Empty<string>() });
        }

        [HttpPost]
        [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
        public async Task<JsonResult> EditAge([Range(18, 150, ErrorMessage = "Age must be between 18 and 150")][Required] int newAge)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToArray();
                return Json(new { success = false, errors = errorMessages });
            }
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            user.Age = newAge;
            await userManager.UpdateAsync(user);

            return Json(new { success = true, errors = Array.Empty<string>() });
        }

        [HttpPost]
        [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
        public async Task<JsonResult> EditGender([Required] string selectedGender)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToArray();
                return Json(new { success = false, errors = errorMessages });
            }
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if (!userProfileValidatorService.Genders.Contains(selectedGender))
            {
                return Json(new { success = false, errors = new[] { "Gender not available" } });
            }

            user.Gender = selectedGender;
            await userManager.UpdateAsync(user);

            return Json(new { success = true, errors = Array.Empty<string>() });
        }

        [HttpPost]
        [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
        public async Task<JsonResult> EditSexuality([Required] string selectedSexuality)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToArray();
                return Json(new { success = false, errors = errorMessages });
            }
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if (!userProfileValidatorService.Sexualities.Contains(selectedSexuality))
            {
                return Json(new { success = false, errors = new[] { "Gender not available" } });
            }

            user.Sexuality = selectedSexuality;
            await userManager.UpdateAsync(user);

            return Json(new { success = true, errors = Array.Empty<string>() });
        }

        [HttpPost]
        [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
        public async Task<JsonResult> EditInterests(string[] interests)
        {
            if (interests.Length > 5)
            {
                return Json(new { success = false, errors = new[] { "Too many interests" } });
            }
            if (interests.Length == 0)
            {
                return Json(new { success = false, errors = new[] { "No interests selected" } });
            }

            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            string[] validInterest = userProfileValidatorService.Interests.
                Intersect(interests)
                .Distinct()
                .ToArray();

            if (validInterest.Length == 0)
            {
                return Json(new { success = false, errors = new[] { "No valid Interest present" } });
            }

            user.Interests = interests;
            await userManager.UpdateAsync(user);

            return Json(new { success = true, errors = Array.Empty<string>() });
        }

        [HttpPost]
        [ServiceFilter(typeof(EnsureDayBuddyUserNotNullFilter))]
        public async Task<JsonResult> EditLocation(string city, string country)
        {
            DayBuddyUser user = (DayBuddyUser)HttpContext.Items[User]!;

            if (!userProfileValidatorService.Countries.Contains(country))
            {
                return Json(new { success = false, errors = new[] { "Country is not available" } });
            }

            user.Country = country;
            await userManager.UpdateAsync(user);

            return Json(new { success = true, errors = Array.Empty<string>() });
        }
    }
}
