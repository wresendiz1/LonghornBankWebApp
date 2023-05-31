
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LonghornBankWebApp.Models
{
    public class RoleEditModel
    {
        public IdentityRole Role { get; set; }
        public IEnumerable<AppUser> ActiveMembers { get; set; }
        public IEnumerable<AppUser> InActiveMembers { get; set; }
    }

    public class RoleModificationModel
    {
        [Required]
        public string RoleName { get; set; }
        public string[] IdsToActivate { get; set; }
        public string[] IdsToDeactivate { get; set; }
    }

}