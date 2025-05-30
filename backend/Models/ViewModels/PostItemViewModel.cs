using System.ComponentModel.DataAnnotations;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Models.ViewModels
{
    public class PostItemViewModel
    {
        [Required]
        [Display(Name = "Item Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Category")]
        public ItemCategory Category { get; set; }

        [Required]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Date Lost/Found")]
        [DataType(DataType.Date)]
        public DateTime? DateLostOrFound { get; set; }

        [Required]
        [Display(Name = "Contact Information")]
        public string ContactInfo { get; set; } = string.Empty;

        [Display(Name = "Upload Image")]
        public IFormFile? Image { get; set; }
    }
} 