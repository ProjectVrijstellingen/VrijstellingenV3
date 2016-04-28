using System.ComponentModel.DataAnnotations;

namespace VTP2015.Modules.Student.ViewModels
{
    public class AddEducationViewModel
    {
        [Display(Name = "Opleiding")]
        [Required]
        [StringLength(60, MinimumLength = 7, ErrorMessage = "De naam van de opleiding moet 7-60 tekens bevatten!")]
        public string Education { get; set; }
    }
}