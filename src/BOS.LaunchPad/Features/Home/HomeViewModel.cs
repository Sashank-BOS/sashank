using BOS.IA.Client.ClientModels;
using BOS.LaunchPad.Models;
using System.Collections.Generic;

namespace BOS.LaunchPad.Features.Home
{
    public class HomeViewModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
        public List<Module> Modules { get; set; } = new List<Module>();
        public List<PermissionsSet> Permissions { get; set; } = new List<PermissionsSet>();
        public bool IsAdmin { get; set; }
    }

    public class SetPermissionsViewModel
    {
        public List<PermissionsSet> Permissions { get; set; } = new List<PermissionsSet>();
        public List<IOperation> Operations { get; set; } = new List<IOperation>();
        public bool IsAdmin { get; set; } = false;
    }

    public class SetModulePermissionsViewModel
    {
        public List<PermissionsSet> Permissions { get; set; } = new List<PermissionsSet>();
        public List<IModule> Modules { get; set; } = new List<IModule>();
        public bool IsAdmin { get; set; } = false;
    }
}
