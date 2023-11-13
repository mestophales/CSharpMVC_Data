using Microsoft.AspNetCore.Identity;

namespace MyChatGPT.Models

{
    public class ApplicationUser : IdentityUser
    {
        // Add the additional field for Avatar
        public string? Avatar { get; set; }
		public string? ScreenName { get; set; }
	}
}
