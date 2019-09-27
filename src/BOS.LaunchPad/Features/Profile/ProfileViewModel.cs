using BOS.LaunchPad.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BOS.LaunchPad.Features.Profile
{
    public class ProfileViewModel
    {
        public User User { get; set; }

        public List<Role> UserRoles { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string NewEmail { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string NewUsername { get; set; }

        [Required]
        [Display(Name = "FirstName")]
        public string NewFirstName { get; set; }

        [Required]
        [Display(Name = "LastName")]
        public string NewLastName { get; set; }
    }
}
