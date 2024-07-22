using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommitteeCalendarAPI.Models;
using CommitteeCalendarAPI.BUS;
using CommitteeCalendarAPI.ActionModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Configuration;
using CommitteeCalendarAPI.BUS.Helpers;

namespace CommitteeCalendarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountsController : ControllerBase
    {
        private readonly CommitteeCalendarContext _context;
        private readonly IConfiguration _configuration;
        private readonly AuthorizationHelper _authHelper;

        public UserAccountsController(CommitteeCalendarContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _authHelper = new AuthorizationHelper(_context);
        }

        // GET: api/UserAccounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserAccountMinimal>>> GetUserAccounts()
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Content("Unauthorized: Admin permission required.");
            }

            return await _context.UserAccounts
                .Where(u => !u.Adminpermission)
                .Select(u => new UserAccountMinimal
                {
                    Username = u.Username,
                    Avatar = u.Avatar
                })
                .ToListAsync();
        }

        // GET: api/UserAccounts/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<UserAccountMinimal>> GetUserAccountByUsername(string username)
        {
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Content("Unauthorized: Admin permission required.");
            }

            var userAccount = await _context.UserAccounts
                .Include(u => u.Participants)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (userAccount == null)
            {
                return NotFound();
            }

            return new UserAccountMinimal
            {
                Username = userAccount.Username,
                Info = userAccount.Info,
                Avatar = userAccount.Avatar,
                Email = userAccount.Email,
                Phonenumber = userAccount.Phonenumber,
                ParticipantsName = userAccount.Participants?.ParticipantsName
            };
        }

        // GET: api/UserAccounts/UsersWithoutParticipants
        [HttpGet("UsersWithoutParticipants")]
        public async Task<ActionResult<IEnumerable<UserAccountMinimal>>> GetUsersWithoutParticipants()
        {
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Unauthorized("Admin permission required.");
            }

            var usersWithoutParticipants = await _context.UserAccounts
                .Where(u => u.ParticipantsId == null && !u.Adminpermission)
                .Select(u => new UserAccountMinimal
                {
                    Username = u.Username,
                    Info = u.Info,
                    Avatar = u.Avatar,
                    Email = u.Email,
                    Phonenumber = u.Phonenumber
                })
                .ToListAsync();

            return Ok(usersWithoutParticipants);
        }

        // PUT: api/UserAccounts/username/{username}
        [HttpPut("username/{username}")]
        public async Task<IActionResult> PutUserAccountByUsername(string username, UserAccountUpdate userAccountUpdate)
        {
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Content("Unauthorized: Admin permission required.");
            }

            var userAccount = await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Username == username);

            if (userAccount == null)
            {
                return NotFound();
            }

            userAccount.Info = userAccountUpdate.Info;
            userAccount.Avatar = userAccountUpdate.Avatar;
            userAccount.Email = userAccountUpdate.Email;
            userAccount.Phonenumber = userAccountUpdate.Phonenumber;

            _context.Entry(userAccount).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserAccountExists(userAccount.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UserAccounts/Register
        [HttpPost("Register")]
        public async Task<ActionResult<UserAccount>> Register(UserLoginRequest userLoginRequest)
        {
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Content("Unauthorized: Admin permission required.");
            }

            var userAccount = new UserAccount
            {
                Id = Guid.NewGuid(),
                Username = userLoginRequest.Username,
                Password = BUSPWDHashing.EncryptData(userLoginRequest.Password),
                Adminpermission = false
            };

            _context.UserAccounts.Add(userAccount);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserAccountByUsername", new { username = userAccount.Username }, userAccount);
        }



        // POST: api/UserAccounts/ChangePassword
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest passwordChangeRequest)
        {
            var requester = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Username == passwordChangeRequest.RequesterUsername);
            if (requester == null)
            {
                return Unauthorized("Requester not found.");
            }

            var targetUser = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Username == passwordChangeRequest.TargetUsername);
            if (targetUser == null)
            {
                return NotFound("Target user not found.");
            }

            if (requester.Adminpermission)
            {
                targetUser.Password = BUSPWDHashing.EncryptData(passwordChangeRequest.NewPassword);
            }
            else
            {
                if (requester.Username != passwordChangeRequest.TargetUsername || !VerifyPassword(passwordChangeRequest.CurrentPassword, targetUser.Password))
                {
                    return Unauthorized("Current password is incorrect or you do not have permission to change this password.");
                }

                targetUser.Password = BUSPWDHashing.EncryptData(passwordChangeRequest.NewPassword);
            }

            _context.Entry(targetUser).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Password changed successfully.");
        }       

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginRequest loginRequest)
        {
            var userAccount = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

            if (userAccount == null || !VerifyPassword(loginRequest.Password, userAccount.Password))
            {
                return Unauthorized();
            }

            var token = GenerateJwtToken(userAccount);

            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(UserAccount user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private bool VerifyPassword(string inputPassword, string storedPasswordHash)
        {
            string inputPasswordHash = BUSPWDHashing.EncryptData(inputPassword);
            return inputPasswordHash == storedPasswordHash;
        }

        private bool UserAccountExists(Guid id)
        {
            return _context.UserAccounts.Any(e => e.Id == id);
        }
    }
}
