using BOS.Auth.Client;
using BOS.LaunchPad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace BOS.LaunchPad.Features.RoleManagement
{
    [Authorize(Roles = "Super Admin")]
    public class RoleManagementController : Controller
    {
        private readonly IAuthClient _authClient;
        public RoleManagementController(IAuthClient authClient)
        {
            _authClient = authClient;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var rolesResponse = await _authClient.GetRolesAsync<Role>();
                var model = new RoleManagementViewModel
                {
                    Roles = rolesResponse.Roles,
                    NewRoleName = null,
                    NewDescription = null
                };

                return View(model);
            }
            catch (Exception)
            {
                throw new Exception("Error while fetching Roles");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Remove(string id)
        {
            try
            {
                if (id != null)
                {
                    Guid _roleId = new Guid(id);
                    var deleteResponse = await _authClient.DeleteRoleAsync(_roleId);
                    if (deleteResponse.IsSuccessStatusCode)
                    {
                        return Json("");
                    }
                    return Json(deleteResponse.BOSErrors[0].Message);
                }
                return Json("Id passed cannot be null or empty");
            }
            catch (Exception)
            {
                return Json("Something went wrong, please contact administrator");
            }
        }

        public async Task<IActionResult> Add(RoleManagementViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.NewDescription))
                {
                    model.NewDescription = string.Empty;
                }
                Role role = new Role { Name = model.NewRoleName, Description = model.NewDescription };
                var addRoleResponse = await _authClient.AddRoleAsync<Role>(role);
                if (addRoleResponse.IsSuccessStatusCode)
                {
                    HttpContext.Session.SetString("SuccessMessage", "Role added successfully");
                    return RedirectToAction("Index", "RoleManagement");
                }
                else
                {
                    if(addRoleResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        HttpContext.Session.SetString("ErrorMessage", addRoleResponse.BOSErrors[0].Message);
                        return RedirectToAction("Index", "RoleManagement");
                    }
                }
                return RedirectToAction("Index", "Error");
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            try
            {
                var role = await _authClient.GetRoleByIdAsync<Role>(new Guid(id));
                var model = new EditRoleViewModel
                {
                    Name = role.Role.Name,
                    NameRecord = role.Role.Name,
                    Id = role.Role.Id,
                    Description = role.Role.Description,
                    DescriptionRecord = role.Role.Description
                };
                return View("_editRole", model);
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            try
            {
                Role role = new Role { Name = model.NameRecord, Description = model.DescriptionRecord };
                var response = await _authClient.UpdateRoleAsync<Role>(model.Id, role);
                if (response.IsSuccessStatusCode)
                {
                    HttpContext.Session.SetString("SuccessMessage", "Role updated successfully");
                    return RedirectToAction("Index", "RoleManagement");
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        HttpContext.Session.SetString("ErrorMessage", "Role with the same name already exists");
                        return RedirectToAction("Index", "RoleManagement");
                    }
                    return RedirectToAction("Index", "Error");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }
    }
}