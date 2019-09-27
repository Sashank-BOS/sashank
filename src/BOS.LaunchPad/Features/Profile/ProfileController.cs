using BOS.Auth.Client;
using BOS.LaunchPad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BOS.LaunchPad.Features.Profile
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IAuthClient _authClient;

        public ProfileController(IAuthClient authClient)
        {
            _authClient = authClient;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var userResponse = await _authClient.GetUserByIdWithRolesAsync<User>(new Guid(userId));
            if (userResponse.IsSuccessStatusCode)
            {
                var user = userResponse.User;
                var model = new ProfileViewModel() { User = user, NewEmail = user.Email, NewUsername = user.Username, NewFirstName = user.FirstName, NewLastName = user.LastName };
                return View(model);
            }
            return RedirectToAction("Index", "Error");
        }

        public async Task<IActionResult> Edit(ProfileViewModel data)
        {
            try
            {
                bool success = true;
                data.User.FirstName = data.NewFirstName;
                data.User.LastName = data.NewLastName;
                data.User.Email = data.NewEmail;
                data.User.Username = data.NewUsername;
                var result = await _authClient.ExtendUserAsync(data.User);
                if (result.IsSuccessStatusCode)
                {
                    success = true;
                }
                else
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        HttpContext.Session.SetString("ErrorMessage", result.BOSErrors[0].Message);
                        return RedirectToAction("Index", "Profile");
                    }
                }
                if (success)
                {
                    HttpContext.Session.SetString("SuccessMessage", "Profile updated successfully");
                    return RedirectToAction("Index", "Profile");
                }
                return RedirectToAction("Index", "Error");
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel data)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var changePasswordReponse = await _authClient.ChangePasswordAsync(new Guid(userId), data.OldPassword, data.Password);

                if (changePasswordReponse.IsSuccessStatusCode)
                {
                    HttpContext.Session.SetString("SuccessMessage", "Password updated successfully");
                    return RedirectToAction("Index", "Profile");
                }
                return RedirectToAction("Index", "Error");
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }
    }
}