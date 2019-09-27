using BOS.Auth.Client;
using BOS.LaunchPad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.LaunchPad.Features.UserManagement
{
    [Authorize(Roles = "Super Admin")]
    public class UserManagementController : Controller
    {
        private readonly IAuthClient _authClient;
        private readonly IEmailSender _emailSender;
        private List<User> userList = new List<User>();
        private readonly int pageSize = 5;
        private readonly int maxPages = 5;

        public UserManagementController(IAuthClient authClient, IEmailSender emailSender)
        {
            _authClient = authClient;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            var usersResponse = await _authClient.GetUsersWithRolesAsync<User>();
            var model = new UserManagementOverviewViewModel { Users = usersResponse.Users };
            string userList = JsonConvert.SerializeObject(usersResponse.Users);
            HttpContext.Session.SetString("UserList", userList);
            model.TotalItems = usersResponse.Users.Count;
            model.MaxPages = maxPages;
            model.PageSize = pageSize;
            model.Pager = new JW.Pager(model.TotalItems, 1, model.PageSize, model.MaxPages);
            var x = model.Users.Skip((model.Pager.CurrentPage - 1) * model.Pager.PageSize).Take(model.Pager.PageSize);
            model.FilteredUsers = x.ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult OnPageClick(string id)
        {
            try
            {
                var data = HttpContext.Session.GetString("UserList");
                userList = JsonConvert.DeserializeObject<List<User>>(data);
                var model = new UserManagementOverviewViewModel { Users = userList };
                model.TotalItems = userList.Count;
                model.MaxPages = maxPages;
                model.PageSize = pageSize;
                model.Pager = new JW.Pager(model.TotalItems, Int32.Parse(id), model.PageSize, model.MaxPages);
                var x = model.Users.Skip((model.Pager.CurrentPage - 1) * model.Pager.PageSize).Take(model.Pager.PageSize);
                model.FilteredUsers = x.ToList();
                return View("index", model);
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddUser()
        {
            var rolesResponse = await _authClient.GetRolesAsync<Role>();
            if (rolesResponse.IsSuccessStatusCode)
            {
                var model = new AddUserViewModel { AllRoles = rolesResponse.Roles };
                return View(model);
            }
            return RedirectToAction("Index", "Error");
        }

        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            try
            {
                var response = await _authClient.AddNewUserAsync<User>(model.Email, model.Email, model.Password);
                if (response.IsSuccessStatusCode)
                {
                    User launchPadUser = new User { Id = response.User.Id, FirstName = model.FirstName.ToString(), LastName = model.LastName.ToString(), Email = model.Email, Username = model.Email };
                    var myresult = await _authClient.ExtendUserAsync(launchPadUser);
                    if (response.IsSuccessStatusCode)
                    {
                        if (model.SelectedRoles != null)
                        {
                            if (model.SelectedRoles.Count > 0)
                            {
                                List<Role> roleList = new List<Role>();
                                foreach (var i in model.SelectedRoles)
                                {
                                    roleList.Add(new Role { Id = Guid.Parse(i) });
                                }
                                var roleResponse = await _authClient.AssociateUserToMultipleRolesAsync(response.User.Id, roleList);
                                if (!roleResponse.IsSuccessStatusCode)
                                {
                                    HttpContext.Session.SetString("ErrorMessage", response.BOSErrors[0].Message);
                                    return RedirectToAction("Index", "UserManagement");
                                }
                            }
                        }
                        await _emailSender.SendEmailAsync(
                                   model.Email,
                                   "Welcome to BOS",
                                   $"<h1>Welcome!</h1><hr /><p>Sign in with your username and password.</p><br /><p>Username: {model.Email}, Password: {model.Password}</p>");
                        HttpContext.Session.SetString("SuccessMessage", "User added successfully");
                        return RedirectToAction("Index", "UserManagement");
                    }
                    else
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                        {
                            HttpContext.Session.SetString("ErrorMessage", response.BOSErrors[0].Message);
                            return RedirectToAction("Index", "UserManagement");
                        }
                    }
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        HttpContext.Session.SetString("ErrorMessage", response.BOSErrors[0].Message);
                        return RedirectToAction("Index", "UserManagement");
                    }
                }
                return RedirectToAction("Index", "Error");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var userId = new Guid(id);
                var profile = await _authClient.GetUserByIdAsync<User>(userId);
                var rolesResponse = await _authClient.GetRolesAsync<Role>();
                var assignedRolesResponse = await _authClient.GetUserRolesByUserIdAsync<Role>(userId);
                var model = new EditUserViewModel
                {
                    Id = userId,
                    EmailRecord = profile.User.Email,
                    Email = profile.User.Email,
                    Username = profile.User.Username,
                    UsernameRecord = profile.User.Username,
                    FirstName = profile.User.FirstName,
                    FirstNameRecord = profile.User.FirstName,
                    LastName = profile.User.LastName,
                    LastNameRecord = profile.User.LastName,
                    AllRoles = rolesResponse.Roles,
                };

                foreach (Role r in rolesResponse.Roles)
                {
                    if (assignedRolesResponse.Roles.Any(ro => ro.Id == r.Id))
                    {
                        model.AssignedRoles.Add(r);
                    }
                    else
                    {
                        model.NotAssignedRoles.Add(r);
                    }
                }
                return View(model);
            }
            catch(Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            try
            {
                bool success = true;
                User launchPadUser = new User { Id = model.Id, Username = model.UsernameRecord, Email = model.EmailRecord, FirstName = model.FirstNameRecord, LastName = model.LastNameRecord };
                var result = await _authClient.ExtendUserAsync(launchPadUser);
                if (result.IsSuccessStatusCode)
                {
                    success = true;
                }
                else
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        HttpContext.Session.SetString("ErrorMessage", result.BOSErrors[0].Message);
                        return RedirectToAction("Index", "UserManagement");
                    }
                }
                if (success)
                {
                    if (model.SelectedRoles != null)
                    {
                        List<Role> roleList = new List<Role>();
                        foreach (var i in model.SelectedRoles)
                        {
                            roleList.Add(new Role { Id = Guid.Parse(i) });
                        }
                        var response = await _authClient.AssociateUserToMultipleRolesAsync(model.Id, roleList);
                        if (!response.IsSuccessStatusCode)
                        {
                            HttpContext.Session.SetString("ErrorMessage", response.BOSErrors[0].Message);
                            return RedirectToAction("Index", "UserManagement");
                        }
                    }
                    HttpContext.Session.SetString("SuccessMessage", "User updated successfully");
                    return RedirectToAction("Index", "UserManagement");
                }
                return RedirectToAction("Index", "Error");
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (id != null)
                {
                    Guid userId = new Guid(id);
                    var deleteUserResponse = await _authClient.DeleteUserAsync(userId);
                    if (deleteUserResponse.IsSuccessStatusCode)
                    {
                        return Json("");
                    }
                    return Json(deleteUserResponse.BOSErrors[0].Message);
                }
                return Json("Id passed cannot be null or empty");
            }
            catch (Exception ex)
            {
                return Json("Something went wrong, please contact administrator");
            }
        }

        [HttpGet]
        public IActionResult ChangePassword(string id)
        {
            var model = new EditUserViewModel { Id = Guid.Parse(id) };
            return View(model);
        }
        public async Task<IActionResult> ChangePassword(EditUserViewModel model)
        {
            try
            {
                var updatePasswordResponse = await _authClient.ForcePasswordChangeAsync(model.Id, model.NewPassword);

                if (updatePasswordResponse.IsSuccessStatusCode)
                {
                    HttpContext.Session.SetString("SuccessMessage", "Password updated successfully");
                    return RedirectToAction("Index", "UserManagement");
                }
                else
                {
                    HttpContext.Session.SetString("ErrorMessage", updatePasswordResponse.BOSErrors[0].Message);
                    return RedirectToAction("Index", "UserManagement");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }
    }
}