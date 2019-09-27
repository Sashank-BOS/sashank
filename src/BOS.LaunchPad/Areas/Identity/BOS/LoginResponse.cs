using System;

namespace BOS.LaunchPad.Areas.Identity.BOS
{
    public class LoginResponse
    {
        public Guid? UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        
    }
}
