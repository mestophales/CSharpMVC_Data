using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MyChatGPT.Models

{
	public class Demographics
	{
		public int Id { get; set; } // Primary key, auto-incremented
		public string UserId { get; set; } // Foreign key to User table

		// Other fields
		public string? StreetAddress { get; set; }
		public string? City { get; set; }
		public string? State { get; set; }
		public string? Country { get; set; }
		public string? ZipCode { get; set; }
		public string? PhoneType { get; set; }
		public string? PhoneCarrier { get; set; }
		public string? InternetProvider { get; set; }
		public string? Gender { get; set; }
		public int? Age { get; set; }
		public bool? HasPets { get; set; }
		public string? PetType { get; set; }
		public string? FavoriteMovie { get; set; }
		public string? FavoriteBand { get; set; }

		// Navigation property to User
		public virtual ApplicationUser User { get; set; }
	}

}
