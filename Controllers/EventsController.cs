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

        //// GET: api/Events
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        //{
        //    return await _context.Events.ToListAsync();
        //}

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);

            if (userId == null)
            {
                return Content("Unauthorized: Not logged in yet or token invalid.");
            }

            var user = await _context.UserAccounts.Include(u => u.Participants)
                                                  .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return Content("Error: Your account is not bound to any participant.");
            }

            if (user.ParticipantsId == null)
            {
                return Content("Error: You have no associated participant.");
            }

            var participantId = user.ParticipantsId.Value;

            var @event = await _context.Events.Include(e => e.EventsParticipants)
                                              .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
            {
                return Content("Error: Event not found.");
            }

            if (!@event.EventsParticipants.Any(ep => ep.ParticipantsId == participantId))
            {
                return Content("Error: You are not a participant in this event.");
            }

            return Ok(@event);
        }


        // PUT: api/Events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(Guid id, Event @event)
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null || !user.Adminpermission)
            {
                return Content("Error: Only admin users can update events.");
            }

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
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(Event @event)
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null || !user.Adminpermission)
            {
                return Content("Error: Only admin users can create events.");
            }

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
            var userId = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null || !user.Adminpermission)
            {
                return Content("Error: Only admin users can delete events.");
            }

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

        [HttpGet("GetMyEvents")]
        public async Task<ActionResult<IEnumerable<Event>>> GetMyEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);

            if (userId == null)
            {
                return Content("Unauthorized: Not logged in yet or token invalid.");
            }

            var user = await _context.UserAccounts.Include(u => u.Participants).FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return Content("Error: Your account not binding yet.");
            }

            if (user.Adminpermission)
            {
                // Return all events if the user has admin permissions
                var allEvents = await _context.Events.ToListAsync();
                return Ok(allEvents);
            }
            else
            {
                if (user.ParticipantsId == null)
                {
                    return Content("Success: You have no events.");
                }

                var participantId = user.ParticipantsId.Value;

                // Return events that the user has joined
                var events = await _context.Events
                    .Include(e => e.EventsParticipants)
                    .Where(e => e.EventsParticipants.Any(ep => ep.ParticipantsId == participantId))
                    .ToListAsync();

                return Ok(events);
            }
        }
    }
}
