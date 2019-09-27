using BOS.IA.Client;
using BOS.LaunchPad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BOS.LaunchPad.Features.Home
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IIAClient _iaClient;

        public HomeController(IIAClient iaClient)
        {
            _iaClient = iaClient;
        }

        public async Task<IActionResult> Index()
        {

            var modules = await _iaClient.GetModulesAsync<Module>(true,true);

            var userPermissionsSets = await GetUserPermissionsSets();

            var model = new HomeViewModel
            {
                Id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value,
                Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value,
                Username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value,
                Modules = modules.Modules,
                Permissions = userPermissionsSets,
                IsAdmin = User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.ToLower().Trim() == "super admin")
            };
            return View(model);
        }

        private async Task<List<PermissionsSet>> GetUserPermissionsSets()
        {
            List<PermissionsSet> allPermSets = new List<PermissionsSet>();

            foreach (var claim in User.Claims.Where(c => c.Type == ClaimTypes.Role))
            {
                var rolePerms = await _iaClient.GetOwnerPermissionsSetsAsync<PermissionsSet>(new Guid(claim.Properties["roleId"]));
                allPermSets.AddRange(rolePerms.Permissions);
            }
            return allPermSets;
        }
    }
}