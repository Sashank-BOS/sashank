using BOS.LaunchPad.Models;
using JW;
using System.Collections.Generic;

namespace BOS.LaunchPad.Features.UserManagement
{
    public class UserManagementOverviewViewModel
    {
        public List<User> Users { get; set; }
        public List<User> FilteredUsers { get; set; }
        public UserCreationInput NewUser { get; set; }
        // below properties required for pagination
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public Pager Pager { get; set; }
    }
}
