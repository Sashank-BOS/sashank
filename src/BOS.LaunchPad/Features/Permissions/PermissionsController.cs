using BOS.Auth.Client;
using BOS.IA.Client;
using BOS.IA.Client.ClientModels;
using BOS.LaunchPad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.LaunchPad.Features.Permissions
{
    [Authorize(Roles = "Super Admin")]
    public class PermissionsController : Controller
    {
        private readonly IIAClient _iaClient;
        private readonly IAuthClient _authClient;
        private readonly IEmailSender _emailSender;

        public PermissionsController(IIAClient iaClient, IAuthClient authClient, IEmailSender emailSender)
        {
            _iaClient = iaClient;
            _authClient = authClient;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                RemoveUserSessions();
                 var rolesResponse = await _authClient.GetRolesAsync<Role>();
                var modulesResponse = await _iaClient.GetModulesAsync<Module>(true,true);
                string roleId = string.Empty, roleName = string.Empty;
                if (HttpContext.Session.GetString("RoleId") != null && HttpContext.Session.GetString("RoleId") != "")
                {
                    roleId = HttpContext.Session.GetString("RoleId");
                    roleName = HttpContext.Session.GetString("RoleName");
                }
                else
                {
                    roleId = Convert.ToString(rolesResponse.Roles[0].Id);
                    roleName = rolesResponse.Roles[0].Name;
                    RemoveRoleSessions();
                    HttpContext.Session.SetString("RoleId", roleId);
                    HttpContext.Session.SetString("RoleName", roleName);
                }
                var userPermissionsSets = await GetUserPermissionsSets(Guid.Parse(roleId));
                var model = new PermissionsViewModel
                {
                    Roles = rolesResponse.Roles,
                    Modules = modulesResponse.Modules,
                    Permissions = userPermissionsSets
                };
                foreach (Module r in modulesResponse.Modules)
                {
                    var OperationsResponse = await _iaClient.GetFilteredOperationsAsync<Operation>($"$filter=deleted eq false and parentOperationId eq null and moduleId eq {r.Id}&$expand=ChildOperations($levels=max)&$orderBy=LastModifiedOn desc");
                    if (OperationsResponse != null && OperationsResponse.IsSuccessStatusCode)
                        r.Operations = new List<IOperation>();
                    r.Operations.AddRange(OperationsResponse.Operations);
                    foreach (Module m in r.ChildModules)
                    {
                        if (userPermissionsSets.Any(p => p.ReferenceId == r.Id))
                        {
                            m.IsAssigned = true;
                        }
                        else
                        {
                            m.IsAssigned = false;
                        }
                        var set = (userPermissionsSets.Any(p => p.ReferenceId == m.Id)) ? userPermissionsSets.Find(p => p.ReferenceId == m.Id) : new PermissionsSet();
                        if (m.Operations != null && m.Operations.Count > 0)
                        {
                            var childOperations = SetIsAssignedOperations(m.Operations, set.Permissions);
                            m.Operations = childOperations;
                        }
                        if (r.ChildModules != null && r.ChildModules.Count > 0)
                        {
                            var childmodules = SetIsAssignedForSubmodules(r.ChildModules, userPermissionsSets);
                            r.ChildModules = childmodules;
                        }
                    }
                    model = SetViewModelPermissionSet(r, userPermissionsSets, model);
                }
                return View(model);
            }
            catch (Exception)
            {
                throw new Exception("Error while fetching values");
            }
        }

        [HttpGet]
        public async Task<IActionResult> FetchModules(string id, string name, string mode = "role", string roles = "")
        {
            try
            {
                if (string.IsNullOrEmpty(roles))
                {
                    roles = string.Empty;
                }
                if (mode.Equals("role"))
                {
                    RemoveRoleSessions();
                    HttpContext.Session.SetString("RoleId", id);
                    HttpContext.Session.SetString("RoleName", name);
                }
                else
                {
                    RemoveUserSessions();
                    HttpContext.Session.SetString("UserId", id);
                    HttpContext.Session.SetString("UserName", name);
                    HttpContext.Session.SetString("UserRoles", roles);
                }
                var modulesResponse = await _iaClient.GetModulesAsync<Module>(true,true);                
                var userPermissionsSets = await GetUserPermissionsSets(Guid.Parse(id));
                var model = new PermissionsViewModel
                {
                    Permissions = userPermissionsSets
                };
                foreach (Module r in modulesResponse.Modules)
                {
                    var OperationsResponse = await _iaClient.GetFilteredOperationsAsync<Operation>($"$filter=deleted eq false and parentOperationId eq null and moduleId eq {r.Id}&$expand=ChildOperations($levels=max)&$orderBy=LastModifiedOn desc");
                    if (OperationsResponse != null && OperationsResponse.IsSuccessStatusCode)
                        r.Operations = new List<IOperation>();
                    r.Operations.AddRange(OperationsResponse.Operations);
                    foreach (Module m in r.ChildModules)
                    {
                        if (userPermissionsSets.Any(p => p.ReferenceId == r.Id))
                        {
                            m.IsAssigned = true;
                        }
                        else
                        {
                            m.IsAssigned = false;
                        }
                        var set = (userPermissionsSets.Any(p => p.ReferenceId == m.Id)) ? userPermissionsSets.Find(p => p.ReferenceId == m.Id) : new PermissionsSet();
                        if (m.Operations != null && m.Operations.Count > 0)
                        {
                            var childOperations = SetIsAssignedOperations(m.Operations, set.Permissions);
                            m.Operations = childOperations;
                        }
                        if (r.ChildModules != null && r.ChildModules.Count > 0)
                        {
                            var childmodules = SetIsAssignedForSubmodules(r.ChildModules, userPermissionsSets);
                            r.ChildModules = childmodules;
                        }
                    }
                    model = SetViewModelPermissionSet(r, userPermissionsSets, model);
                }
                return  Json(model);
            }
            catch (Exception e)
            {
                return Json("Something went wrong, please contact administrator");
            }
        }

        public async Task<IActionResult> AddRole(PermissionsViewModel model)
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
                    return RedirectToAction("EditPermissions", "Permissions");
                }
                else
                {
                    if (addRoleResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        HttpContext.Session.SetString("ErrorMessage", addRoleResponse.BOSErrors[0].Message);
                        return RedirectToAction("EditPermissions", "Permissions");
                    }
                }
                return RedirectToAction("Index", "Error");
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public async Task<IActionResult> EditPermissions()
        {
            try
            {
                var rolesResponse = await _authClient.GetRolesAsync<Role>();
                var modulesResponse = await _iaClient.GetModulesAsync<Module>(true, true); //,true
                string roleId = string.Empty, roleName = string.Empty;
                if (HttpContext.Session.GetString("RoleId") != null && HttpContext.Session.GetString("RoleId") != "")
                {
                    roleId = HttpContext.Session.GetString("RoleId");
                    roleName = HttpContext.Session.GetString("RoleName");
                }
                else
                {
                    roleId = Convert.ToString(rolesResponse.Roles[0].Id);
                    roleName = rolesResponse.Roles[0].Name;
                    RemoveRoleSessions();
                    HttpContext.Session.SetString("RoleId", roleId);
                    HttpContext.Session.SetString("RoleName", roleName);
                }
                var userPermissionsSets = await GetUserPermissionsSets(Guid.Parse(roleId));
                var model = new PermissionsViewModel
                {
                    Roles = rolesResponse.Roles,
                    Modules = modulesResponse.Modules,
                    Permissions = userPermissionsSets
                };
                foreach (Module r in modulesResponse.Modules)
                {
                    var OperationsResponse = await _iaClient.GetFilteredOperationsAsync<Operation>($"$filter=deleted eq false and parentOperationId eq null and moduleId eq {r.Id}&$expand=ChildOperations($levels=max)&$orderBy=LastModifiedOn desc");
                    if (OperationsResponse != null && OperationsResponse.IsSuccessStatusCode)
                        r.Operations = new List<IOperation>();
                        r.Operations.AddRange(OperationsResponse.Operations);
                        foreach (Module m in r.ChildModules)
                        {
                            if (userPermissionsSets.Any(p => p.ReferenceId == r.Id))
                            {
                                m.IsAssigned = true;
                            }
                            else
                            {
                                m.IsAssigned = false;
                            }
                            var set = (userPermissionsSets.Any(p => p.ReferenceId == m.Id)) ? userPermissionsSets.Find(p => p.ReferenceId == m.Id) : new PermissionsSet();
                            if (m.Operations != null && m.Operations.Count > 0)
                            {
                                var childOperations = SetIsAssignedOperations(m.Operations, set.Permissions);
                                m.Operations = childOperations;
                            }                                              
                        if (r.ChildModules != null && r.ChildModules.Count > 0)
                            {
                                var childmodules = SetIsAssignedForSubmodules(r.ChildModules, userPermissionsSets);
                                r.ChildModules = childmodules;
                            }
                        }
                   model=  SetViewModelPermissionSet(r, userPermissionsSets, model);
                }
                return View("EditIndex", model);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching values");
            }
        }


        public PermissionsViewModel SetViewModelPermissionSet(Module Module, List<PermissionsSet> userPermissionsSets, PermissionsViewModel model)
        {
            var AllPermissionsEnabled = false;
            var set = (userPermissionsSets.Any(p => p.ReferenceId == Module.Id)) ? userPermissionsSets.Find(p => p.ReferenceId == Module.Id) : new PermissionsSet();
            foreach (Operation o in Module.Operations)
            {
                if (set.Permissions.Any(p => p.Id == o.Id))
                {
                    o.IsAssigned = true;
                }
                else
                {
                    o.IsAssigned = false;
                }
                if (o.ChildOperations != null && o.ChildOperations.Count > 0)
                {
                    var childOperations = SetIsAssignedOperations(o.ChildOperations, set.Permissions);
                    o.ChildOperations = childOperations;
                }
            }
            var operationList = Module.Operations.Select(o => o.Id).ToList();
            var permitedOperationsList = set.Permissions.Select(p => p.Id).ToList();
            AllPermissionsEnabled = operationList.All(o=> permitedOperationsList.Contains(o));
            if (AllPermissionsEnabled == true)
            {
                Module.IsAssigned = true;
            }
            if (userPermissionsSets.Any(m => m.ReferenceId == Module.Id))
            {                              
                if (AllPermissionsEnabled)
                {                    
                    model.AssignedModules.Add(Module);
                }
                else
                {
                    bool toPush = true;                   
                    if (toPush)
                    {
                        if (!model.PartiallyAssignedModules.Any(am => am.Id == set.ReferenceId))
                        {
                            model.PartiallyAssignedModules.Add(Module);
                        }
                    }
                }
            }
            else
            {
                model.NotAssignedModules.Add(Module);
            }
            return model;
        }

        public List<IModule> SetIsAssignedForSubmodules(IEnumerable<IModule> modules, List<PermissionsSet> userPermissionsSets)
        {
            foreach (Module m in modules)
            {
               
                if (userPermissionsSets.Any(p => p.ReferenceId == m.Id))
                {
                    m.IsAssigned = true;
                }
                else
                {
                    m.IsAssigned = false;
                }
                foreach (Operation o in m.Operations)
                {
                    var set =(userPermissionsSets.Any(p => p.ReferenceId == m.Id))? userPermissionsSets.Find(p => p.ReferenceId == m.Id): new PermissionsSet();
                    if (set!=null && set.Permissions.Any(p => p.Id == o.Id))
                    {
                        o.IsAssigned = true;
                    }
                    else
                    {
                        o.IsAssigned = false;
                    }
                    if (o.ChildOperations != null && o.ChildOperations.Count > 0)
                    {
                        var childOperations = SetIsAssignedOperations(o.ChildOperations, set.Permissions);
                        o.ChildOperations = childOperations;
                    }
                }

                if (m.ChildModules != null && m.ChildModules.Count > 0)
                {
                    var childmodules = SetIsAssignedForSubmodules(m.ChildModules, userPermissionsSets);
                    m.ChildModules = childmodules;
                }
            }
            return modules.ToList();
        }

        public List<IOperation> SetIsAssignedOperations(IEnumerable<IOperation> operations, List<IOperation> permissions) {
            foreach (Operation op in operations)
            {
                if (permissions.Any(p => p.Id == op.Id))
                {
                    op.IsAssigned = true;
                }
                else
                {
                    op.IsAssigned = false;                   
                }
                if (op.ChildOperations != null && op.ChildOperations.Count > 0)
                op.ChildOperations =  SetIsAssignedOperations(op.ChildOperations.AsEnumerable(), permissions);
            }
            return operations.ToList();
        }


        [HttpPost]
        public async Task<IActionResult> UpdatePermissions([FromBody]SavePermissions updateData)
        {
            try
            {
              
                if (updateData != null)
                {
                    var updateResponse = await _iaClient.AssignMultiplePermissionSetsAsync(Guid.Parse(Convert.ToString(updateData.OwnerId)), updateData.Permissions);
                    if (updateResponse.IsSuccessStatusCode)
                    {
                        return Json("");
                    }
                    return Json(updateResponse.BOSErrors[0].Message);
                }
                return Json("The parameters sent cannot be null");
            }
            catch (Exception e)
            {
                return Json("Something went wrong, please contact administrator");
            }
        }

        public async Task<IActionResult> Users()
        {
            try
            {
                RemoveRoleSessions();
                var usersResponse = await _authClient.GetUsersWithRolesAsync<User>();
                var modulesResponse = await _iaClient.GetModulesAsync<Module>(true,true);
                string userId = string.Empty, userName = string.Empty, userRoles = string.Empty;
                if (HttpContext.Session.GetString("UserId") != null && HttpContext.Session.GetString("UserId") != "")
                {
                    userId = HttpContext.Session.GetString("UserId");
                    userName = HttpContext.Session.GetString("UserName");
                    userRoles = HttpContext.Session.GetString("UserRoles");
                }
                else
                {
                    if (usersResponse.Users[0].Roles.Count > 0)
                    {
                        var thisRoles = "";
                        foreach (var role in usersResponse.Users[0].Roles)
                        {
                            if (thisRoles == "")
                            {
                                thisRoles = role.Role.Name;
                            }
                            else
                            {
                                thisRoles += "," + role.Role.Name;
                            }
                        }
                        userRoles = thisRoles;
                    }
                    userId = Convert.ToString(usersResponse.Users[0].Id);
                    userName = usersResponse.Users[0].LastName + ", " + usersResponse.Users[0].FirstName;
                    RemoveUserSessions();
                    HttpContext.Session.SetString("UserId", userId);
                    HttpContext.Session.SetString("UserName", userName);
                    HttpContext.Session.SetString("UserRoles", userRoles);
                }
                var userPermissionsSets = await GetUserPermissionsSets(Guid.Parse(userId));
                var model = new PermissionsViewModel
                {
                    Users = usersResponse.Users,
                    Modules = modulesResponse.Modules,
                    Permissions = userPermissionsSets
                };
                foreach (Module r in modulesResponse.Modules)
                {
                    var OperationsResponse = await _iaClient.GetFilteredOperationsAsync<Operation>($"$filter=deleted eq false and parentOperationId eq null and moduleId eq {r.Id}&$expand=ChildOperations($levels=max)&$orderBy=LastModifiedOn desc");
                    if (OperationsResponse != null && OperationsResponse.IsSuccessStatusCode)
                        r.Operations = new List<IOperation>();
                    r.Operations.AddRange(OperationsResponse.Operations);
                    foreach (Module m in r.ChildModules)
                    {
                        if (userPermissionsSets.Any(p => p.ReferenceId == r.Id))
                        {
                            m.IsAssigned = true;
                        }
                        else
                        {
                            m.IsAssigned = false;
                        }
                        var set = (userPermissionsSets.Any(p => p.ReferenceId == m.Id)) ? userPermissionsSets.Find(p => p.ReferenceId == m.Id) : new PermissionsSet();
                        if (m.Operations != null && m.Operations.Count > 0)
                        {
                            var childOperations = SetIsAssignedOperations(m.Operations, set.Permissions);
                            m.Operations = childOperations;
                        }
                        if (r.ChildModules != null && r.ChildModules.Count > 0)
                        {
                            var childmodules = SetIsAssignedForSubmodules(r.ChildModules, userPermissionsSets);
                            r.ChildModules = childmodules;
                        }
                    }
                    model = SetViewModelPermissionSet(r, userPermissionsSets, model);
                }                
                return View("Users", model);
            }
            catch (Exception)
            {
                throw new Exception("Error while fetching values");
            }
        }

        public async Task<IActionResult> EditUserPermissions()
        {
            try
            {
                var usersResponse = await _authClient.GetUsersWithRolesAsync<User>();
                var modulesResponse = await _iaClient.GetModulesAsync<Module>(true,true);
                string userId = string.Empty, userName = string.Empty, userRoles = string.Empty;
                if (HttpContext.Session.GetString("UserId") != null && HttpContext.Session.GetString("UserId") != "")
                {
                    userId = HttpContext.Session.GetString("UserId");
                    userName = HttpContext.Session.GetString("UserName");
                    userRoles = HttpContext.Session.GetString("UserRoles");
                }
                else
                {
                    if (usersResponse.Users[0].Roles.Count > 0)
                    {
                        var thisRoles = "";
                        foreach (var role in usersResponse.Users[0].Roles)
                        {
                            if (thisRoles == "")
                            {
                                thisRoles = role.Role.Name;
                            }
                            else
                            {
                                thisRoles += "," + role.Role.Name;
                            }
                        }
                        userRoles = thisRoles;
                    }
                    userId = Convert.ToString(usersResponse.Users[0].Id);
                    userName = usersResponse.Users[0].LastName + ", " + usersResponse.Users[0].FirstName;
                    RemoveUserSessions();
                    HttpContext.Session.SetString("UserId", userId);
                    HttpContext.Session.SetString("UserName", userName);
                    HttpContext.Session.SetString("UserRoles", userRoles);
                }
                var userPermissionsSets = await GetUserPermissionsSets(Guid.Parse(userId));
                var model = new PermissionsViewModel
                {
                    Users = usersResponse.Users,
                    Modules = modulesResponse.Modules,
                    Permissions = userPermissionsSets
                };
                foreach (Module r in modulesResponse.Modules)
                {
                    var OperationsResponse = await _iaClient.GetFilteredOperationsAsync<Operation>($"$filter=deleted eq false and parentOperationId eq null and moduleId eq {r.Id}&$expand=ChildOperations($levels=max)&$orderBy=LastModifiedOn desc");
                    if (OperationsResponse != null && OperationsResponse.IsSuccessStatusCode)
                        r.Operations = new List<IOperation>();
                    r.Operations.AddRange(OperationsResponse.Operations);
                    foreach (Module m in r.ChildModules)
                    {
                        if (userPermissionsSets.Any(p => p.ReferenceId == r.Id))
                        {
                            m.IsAssigned = true;
                        }
                        else
                        {
                            m.IsAssigned = false;
                        }
                        var set = (userPermissionsSets.Any(p => p.ReferenceId == m.Id)) ? userPermissionsSets.Find(p => p.ReferenceId == m.Id) : new PermissionsSet();
                        if (m.Operations != null && m.Operations.Count > 0)
                        {
                            var childOperations = SetIsAssignedOperations(m.Operations, set.Permissions);
                            m.Operations = childOperations;
                        }
                        if (r.ChildModules != null && r.ChildModules.Count > 0)
                        {
                            var childmodules = SetIsAssignedForSubmodules(r.ChildModules, userPermissionsSets);
                            r.ChildModules = childmodules;
                        }
                    }
                    model = SetViewModelPermissionSet(r, userPermissionsSets, model);
                }
                return View("EditUser", model);
            }
            catch (Exception)
            {
                throw new Exception("Error while fetching values");
            }
        }

        public async Task<IActionResult> AddUser(PermissionsViewModel model)
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
                        await _emailSender.SendEmailAsync(
                                   model.Email,
                                   "Welcome to BOS",
                                   $"<h1>Welcome!</h1><hr /><p>Sign in with your username and password.</p><br /><p>Username: {model.Email}, Password: {model.Password}</p>");
                        HttpContext.Session.SetString("SuccessMessage", "User added successfully");
                        return RedirectToAction("EditUserPermissions", "Permissions");
                    }
                    else
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                        {
                            HttpContext.Session.SetString("ErrorMessage", response.BOSErrors[0].Message);
                            return RedirectToAction("EditUserPermissions", "Permissions");
                        }
                    }
                    return RedirectToAction("Index", "Error");
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        HttpContext.Session.SetString("ErrorMessage", response.BOSErrors[0].Message);
                        return RedirectToAction("EditUserPermissions", "Permissions");
                    }
                }

                return RedirectToAction("Index", "Error");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public async Task<IActionResult> Modules()
        {
            try
            {
                RemoveAllSessions();
                var modulesResponse = await _iaClient.GetModulesAsync<Module>(true,true);
                var model = new PermissionsViewModel
                {
                    Modules = modulesResponse.Modules,
                };

                return View("Modules", model);
            }
            catch (Exception)
            {
                throw new Exception("Error while fetching modules");
            }
        }

        private async Task<List<PermissionsSet>> GetUserPermissionsSets(Guid ownerId)
        {
            List<PermissionsSet> allPermSets = new List<PermissionsSet>();
            var rolePerms = await _iaClient.GetOwnerPermissionsSetsAsync<PermissionsSet>(ownerId);
            allPermSets.AddRange(rolePerms.Permissions);
            return allPermSets;
        }

        private void RemoveAllSessions()
        {
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("UserRoles");
            HttpContext.Session.Remove("RoleId");
            HttpContext.Session.Remove("RoleName");
        }

        private void RemoveUserSessions()
        {
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("UserRoles");
        }

        private void RemoveRoleSessions()
        {
            HttpContext.Session.Remove("RoleId");
            HttpContext.Session.Remove("RoleName");
        }
    }
}
