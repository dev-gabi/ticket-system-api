using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers
{
    public static class AuthHelpers
    {
        public static string GenerateCorporateEmail(string userName, string emailSuffix)
        {
            return userName +emailSuffix;
        }

        public static string GenerateRandomPassword()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 12);
        }

        public static IEnumerable<Claim> SetUserClaims(IdentityUser user, UserManager<IdentityUser> _userManager)
        {
            return new[]
                {
                    new Claim("Id", user.Id),
                    new Claim("UserName", user.UserName),
                    new Claim("Email", user.Email),
                    new Claim("Role", _userManager.GetRolesAsync(user).Result.FirstOrDefault())
                };
        }

        public static string DecodeToken(string token)
        {
            var decodedToken = WebEncoders.Base64UrlDecode(token);
            return Encoding.UTF8.GetString(decodedToken);
        }


        //public static bool IsRoleExistsInRolesList(string roleName)
        //{
        //    foreach (var enumValue in Enum.GetValues(typeof(services.Enums.Roles)))
        //    {
        //        string enumValueAsString = (string)enumValue;
        //        if (enumValueAsString.ToLower() == roleName.ToLower())
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}
