using BOS.LaunchPad.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BOS.LaunchPad.Features.UserManagement
{
    public class AddUserViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public List<string> SelectedRoles { get; set; }

        public List<Role> AllRoles { get; set; }

    }
}
