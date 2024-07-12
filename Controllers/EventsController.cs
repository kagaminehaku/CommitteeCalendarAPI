using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommitteeCalendarAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net;

namespace CommitteeCalendarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly CommitteeCalendarContext _context;

        public EventsController(CommitteeCalendarContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events.ToListAsync();
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(Guid id)
        {
            var @event = await _context.Events.FindAsync(id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }

        // PUT: api/Events/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(Guid id, Event @event)
        {
            if (id != @event.EventId)
            {
                return BadRequest();
            }

            _context.Entry(@event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
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

        // POST: api/Events
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(Event @event)
        {
            _context.Events.Add(@event);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventExists(@event.EventId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEvent", new { id = @event.EventId }, @event);
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventExists(Guid id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }

        [HttpGet("MyEvents")]
        public async Task<ActionResult<IEnumerable<Event>>> GetMyEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            //var userId = "7a996d69-1f48-4a70-857d-b2f48e37f17e";

            if (userId == null)
            {
                //return Unauthorized();
                //return NotFound("ok run");
                return Content("Unauthorized: Not logged in yet or token invalid.");
            }

            var user = await _context.UserAccounts.Include(u => u.Participants).FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                //return NotFound("User not associated with any participant");
                //return Unauthorized("LMAO");
                return Content("Error: Your account not binding yet.");
            }

            if (user.ParticipantsId == null)
            {
                //return NotFound("User not associated with any participant");
                //return Unauthorized("LMAO");
                return Content("Success: You have no events.");
            }

            var participantId = user.ParticipantsId.Value;

            var events = await _context.Events
                .Include(e => e.EventsParticipants)
                .Where(e => e.EventsParticipants.Any(ep => ep.ParticipantsId == participantId))
                .ToListAsync();

            return events;
        }
    }
}
