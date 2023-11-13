namespace MyChatGPT.Models
{
	public class UpdateDemographicsViewModel
	{
		public string StreetAddress { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Country { get; set; }
		public string ZipCode { get; set; }
		public string PhoneType { get; set; }
		public string PhoneCarrier { get; set; }
		public string InternetProvider { get; set; }
		public string Gender { get; set; }
		public int? Age { get; set; }  // Nullable
		public bool? HasPets { get; set; }  // Nullable
		public string PetType { get; set; }
		public string FavoriteMovie { get; set; }
		public string FavoriteBand { get; set; }
	}

}
