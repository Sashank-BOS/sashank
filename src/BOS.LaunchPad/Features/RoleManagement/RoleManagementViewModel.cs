using BOS.LaunchPad.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BOS.LaunchPad.Features.RoleManagement
{
    public class RoleManagementViewModel
    {
        public List<Role> Roles { get; set; }    //list to fetch existing roles
        [Required]
        [Display(Name = "Name")]
        public string NewRoleName { get; set; }   //to add new role name
        [Display(Name = "Description")]
        public string NewDescription { get; set; }   //to add new role name
    }

    public class EditRoleViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string NameRecord { get; set; }
        public Guid Id { get; set; }
        public string Description { get; set; }
        [Display(Name = "Description")]
        public string DescriptionRecord { get; set; }
    }
}
