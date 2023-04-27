using System.ComponentModel.DataAnnotations;


namespace PhoneBook.Model
{
    public class PhoneBookEntry
    {
        [RegularExpression(@"^([a-zA-Z]+[’'’]*[a-zA-Z]*[\s\,\.\-]*[a-zA-Z]+[’'’]*[\s\,\.\-]*[a-zA-Z]*[\s]*){1,3}$")]
        [StringLength(60, MinimumLength = 1)]
        [Required(ErrorMessage = "Name Required")]
        public string? Name { get; set; }

        [RegularExpression(@"^[\d\(\)\-\.\+\s]{5,20}$", ErrorMessage = "Invalid phone number")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(20, MinimumLength = 5)]
        [Required(ErrorMessage = "Phone Number Required")]
        public string? PhoneNumber { get; set; }
    }
}