using BOS.LaunchPad.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BOS.LaunchPad.Features.UserManagement
{
    public class EditUserViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string UsernameRecord { get; set; } //used for comparison on update button click to only update if the value changed

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string EmailRecord { get; set; } //used for comparison on update button click to only update if the value changed

        [Required]
        [Display(Name = "FirstName")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "FirstName")]
        public string FirstNameRecord { get; set; }

        [Required]
        [Display(Name = "LastName")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "LastName")]
        public string LastNameRecord { get; set; }
        //public List<Role> Roles { get; set; }
        public List<Role> AssignedRoles { get; set; }
        public List<Role> AllRoles { get; set; }
        public List<Role> NotAssignedRoles { get; set; }
        public List<string> SelectedRoles { get; set; }
        public string Password { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        public EditUserViewModel()
        {
            AssignedRoles = new List<Role>();
            AllRoles = new List<Role>();
            NotAssignedRoles = new List<Role>();
            SelectedRoles = new List<string>();
        }
    }
}
