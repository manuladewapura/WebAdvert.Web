using System.ComponentModel.DataAnnotations;

namespace WebAdvert.Web.Models
{
	public class SignupModel
	{
		[Required]
		[EmailAddress]
		[Display(Name ="Email")]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[StringLength(6, ErrorMessage ="Password must be at least 6 charactors long!")]
		public string Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Compare("Password", ErrorMessage ="Password and its confirmation do not match")]
		public string ConfirmPassword { get; set; }
	}
}
