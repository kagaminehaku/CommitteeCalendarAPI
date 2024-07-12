using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommitteeCalendarAPI.Models;
using BUS;
using CommitteeCalendarAPI.ActionModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Configuration;

namespace CommitteeCalendarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountsController : ControllerBase
    {
        private readonly CommitteeCalendarContext _context;
        private readonly IConfiguration _configuration;
        public UserAccountsController(CommitteeCalendarContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/UserAccounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserAccount>>> GetUserAccounts()
        {
            return await _context.UserAccounts.ToListAsync();
        }

        //// GET: api/UserAccounts/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<UserAccount>> GetUserAccount(Guid id)
        //{
        //    var userAccount = await _context.UserAccounts.FindAsync(id);

        //    if (userAccount == null)
        //    {
        //        return NotFound();
        //    }

        //    return userAccount;
        //}

        // PUT: api/UserAccounts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserAccount(Guid id, UserAccount userAccount)
        {
            if (id != userAccount.Id)
            {
                return BadRequest();
            }

            _context.Entry(userAccount).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserAccountExists(id))
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

        // POST: api/UserAccounts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<UserAccount>> PostUserAccount(UserAccount userAccount)
        {
            _context.UserAccounts.Add(userAccount);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserAccount", new { id = userAccount.Id }, userAccount);
        }

        //// DELETE: api/UserAccounts/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteUserAccount(Guid id)
        //{
        //    var userAccount = await _context.UserAccounts.FindAsync(id);
        //    if (userAccount == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.UserAccounts.Remove(userAccount);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

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
                //Expires = DateTime.UtcNow.AddSeconds(15),
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
