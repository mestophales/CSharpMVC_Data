// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyChatGPT.Models;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MyChatGPT.Data;

namespace MyChatGPT.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ApplicationDbContext _context; // Database context

		public UpdateDemographicsViewModel DemographicsViewModel { get; set; }

		public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment,
			ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment; // Initialize _environment
			_context = context; // Initialize the database context
			DemographicsViewModel = new UpdateDemographicsViewModel(); // Initialize the ViewModel
		}
        // Property to hold the avatar URL
        public string AvatarUrl { get; set; }

        [BindProperty]
        public IFormFile AvatarFile { get; set; }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }
		public ApplicationUser ApplicationUser { get; private set; }
		public object StreetAddress { get; private set; }

		public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

			[RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Only letters and numbers are allowed.")]
			[Display(Name = "Username")]
            public string Username { get; set; }

			[Display(Name = "ScreenName")]
			public string ScreenName { get; set; }



			[Display(Name = "StreetAddress")]
			public string StreetAddress { get; set; }

			[Display(Name = "City")]
			public string City { get; set; }

		}

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            StreetAddress = DemographicsViewModel.StreetAddress;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
				Username = Username,
				ScreenName = user.ScreenName, // Use user.ScreenName here
 
			};
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }



			// Assuming Avatar is a path relative to wwwroot
			AvatarUrl = user.Avatar ?? "./avatars/noavatar.png"; // Provide a default avatar if none is set

			Input = new InputModel
			{
				// ... Other properties ...
				ScreenName = user.ScreenName  // Ensure this line is correctly assigning the ScreenName
			};

			var userId = _userManager.GetUserId(User); // Get current user's ID
			ApplicationUser = await _userManager.FindByIdAsync(userId); // Retrieve user from database
																		// Now ApplicationUser.ScreenName can be accessed in the view

			// Fetch demographic data for the user
			var demographics = await _context.Demographics
				.FirstOrDefaultAsync(d => d.UserId == user.Id);

			if (demographics != null)
			{
				// Populate the ViewModel
				DemographicsViewModel = new UpdateDemographicsViewModel
				{
					StreetAddress = demographics.StreetAddress,
					City = demographics.City,
					// ... populate other fields similarly

				};
				//await _context.SaveChangesAsync();
			}

			await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Handling the avatar file upload
            if (AvatarFile != null)
            {

                if (user == null)
                {
                    // Handle the case where the user is not found
                    // Log the error or set an error message to display to the user
                    return Page();
                }
                var fileName = Path.GetFileName(AvatarFile.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "avatars", fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(fileStream);
                }

                //user.Avatar = filePath; // Update the user's avatar path
                user.Avatar = "avatars/" + fileName;
                await _userManager.UpdateAsync(user);
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }


            if (Input.ScreenName != user.ScreenName)
            {
                // Update the ScreenName in the ApplicationUser object
                user.ScreenName = Input.ScreenName;
                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    // Update the ScreenName claim
                    var screenNameClaim = (await _userManager.GetClaimsAsync(user))
                                         .FirstOrDefault(c => c.Type == "ScreenName");
                    if (screenNameClaim != null)
                    {
                        await _userManager.ReplaceClaimAsync(user, screenNameClaim,
                                                             new Claim("ScreenName", user.ScreenName));
                    }
                    else
                    {
                        await _userManager.AddClaimAsync(user, new Claim("ScreenName", user.ScreenName));
                    }

                    // Refresh the sign-in session to update the security stamp and claims
                    await _signInManager.RefreshSignInAsync(user);

                    // Redirect or return a success message
                    StatusMessage = "Your profile has been updated";
                    return RedirectToPage();
                }
                else
                {
                    // Handle errors
                    StatusMessage = "An error occurred while updating your profile";
                    return RedirectToPage();
                }
            }


            // Fetch existing demographic data
            var demographics = await _context.Demographics
            .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (demographics != null)
            {
                // Update the demographic data
                demographics.StreetAddress = Input.StreetAddress;
                demographics.City = Input.City;
                // ... set other fields from the ViewModel

                // Save the changes to the database
                _context.Update(demographics);
                await _context.SaveChangesAsync();

                // Set a success message
                StatusMessage = "Your demographic information has been updated";
            }
            else
            {
                // Create a new Demographics record
                var newDemographics = new Demographics
                {
                    UserId = user.Id, // Set the foreign key to the user's ID
                    StreetAddress = Input.StreetAddress,
                    City = Input.City,
                    // ... other fields
                };
                _context.Demographics.Add(newDemographics);
                await _context.SaveChangesAsync();

            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
