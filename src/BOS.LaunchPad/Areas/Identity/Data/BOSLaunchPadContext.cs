using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BOS.LaunchPad.Models
{
    public class BOSLaunchPadContext : IdentityDbContext<IdentityUser>
    {
        public BOSLaunchPadContext(DbContextOptions<BOSLaunchPadContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
