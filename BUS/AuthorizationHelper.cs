using CommitteeCalendarAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CommitteeCalendarAPI.BUS.Helpers
{
    public class AuthorizationHelper
    {
        private readonly CommitteeCalendarContext _context;

        public AuthorizationHelper(CommitteeCalendarContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserAdminAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.Name);
            if (userId == null) return false;

            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            return userAccount != null && userAccount.Adminpermission;
        }
    }
}
