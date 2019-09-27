using BOS.LaunchPad.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BOS.LaunchPad.Features.Permissions
{
    public class PermissionsViewModel
    {
        public List<Role> Roles { get; set; } = new List<Role>();
        public List<Module> Modules { get; set; } = new List<Module>();
        public List<User> Users { get; set; } = new List<User>();
        public List<PermissionsSet> Permissions { get; set; } = new List<PermissionsSet>();
        public List<Module> AssignedModules { get; set; } = new List<Module>();
        public List<Module> NotAssignedModules { get; set; } = new List<Module>();
        public List<Module> PartiallyAssignedModules { get; set; } = new List<Module>();
        [Required]
        [Display(Name = "Name")]
        public string NewRoleName { get; set; }   //to add new role name
        [Display(Name = "Description")]
        public string NewDescription { get; set; }   //to add new role name
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
    }

    public class PermissionModules
    {
        public Guid OwnerId { get; set; }
        public Guid ReferenceId { get; set; }
        public string ReferenceName { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public List<PermissionOperations> Permissions { get; set; } = new List<PermissionOperations>();
    }

    public class PermissionOperations
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class SavePermissions
    {
        public Guid OwnerId { get; set; }

        public List<PermissionModules> Permissions { get; set; } = new List<PermissionModules>();
    }
}
