using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommitteeCalendarAPI.Models;
using System.Security.Claims;
using CommitteeCalendarAPI.ActionModels;
using CommitteeCalendarAPI.BUS.Helpers;

namespace CommitteeCalendarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly CommitteeCalendarContext _context;
        private readonly AuthorizationHelper _authHelper;

        public EventsController(CommitteeCalendarContext context)
        {
            _context = context;
            _authHelper = new AuthorizationHelper(_context);
        }

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
        public async Task<IActionResult> PutEvent(Guid id, EventRequest eventRequest)
        {
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Content("Error: Only admin users can update events.");
            }

            var existingEvent = await _context.Events
                .Include(e => e.EventsParticipants)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            existingEvent.EventName = eventRequest.EventName;
            existingEvent.HostPerson = eventRequest.HostPerson;
            existingEvent.StartDate = eventRequest.StartDate;
            existingEvent.StartTime = eventRequest.StartTime;
            existingEvent.Duration = eventRequest.Duration;
            existingEvent.Detail = eventRequest.Detail;
            existingEvent.LocationId = eventRequest.LocationId;
            existingEvent.Participants = eventRequest.Participants;
            existingEvent.IsAppoved = eventRequest.IsAppoved;

            var existingParticipantIds = existingEvent.EventsParticipants.Select(ep => ep.ParticipantsId).ToList();
            var newParticipantIds = eventRequest.ParticipantIds.Except(existingParticipantIds).ToList();
            var removedParticipantIds = existingParticipantIds.Except(eventRequest.ParticipantIds).ToList();

            foreach (var participantId in removedParticipantIds)
            {
                var eventParticipant = existingEvent.EventsParticipants.FirstOrDefault(ep => ep.ParticipantsId == participantId);
                if (eventParticipant != null)
                {
                    _context.EventsParticipants.Remove(eventParticipant);
                }
            }

            foreach (var participantId in newParticipantIds)
            {
                var newEventParticipant = new EventsParticipant
                {
                    EvPartiId = Guid.NewGuid(),
                    EventId = existingEvent.EventId,
                    ParticipantsId = participantId
                };
                _context.EventsParticipants.Add(newEventParticipant);
            }

            _context.Entry(existingEvent).State = EntityState.Modified;

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
        public async Task<ActionResult<Event>> PostEvent(EventRequest eventRequest)
        {
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Content("Error: Only admin users can create events.");
            }

            var @event = new Event
            {
                EventId = Guid.NewGuid(),
                EventName = eventRequest.EventName,
                HostPerson = eventRequest.HostPerson,
                StartDate = eventRequest.StartDate,
                StartTime = eventRequest.StartTime,
                Duration = eventRequest.Duration,
                Detail = eventRequest.Detail,
                LocationId = eventRequest.LocationId,
                Participants = eventRequest.Participants,
                IsAppoved = eventRequest.IsAppoved
            };

            _context.Events.Add(@event);
            try
            {
                await _context.SaveChangesAsync();

                foreach (var participantId in eventRequest.ParticipantIds)
                {
                    var eventsParticipant = new EventsParticipant
                    {
                        EvPartiId = Guid.NewGuid(),
                        EventId = @event.EventId,
                        ParticipantsId = participantId
                    };
                    _context.EventsParticipants.Add(eventsParticipant);
                }

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
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Content("Error: Only admin users can delete events.");
            }

            var @event = await _context.Events
                .Include(e => e.EventsParticipants)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
            {
                return NotFound();
            }

            foreach (var eventParticipant in @event.EventsParticipants)
            {
                _context.EventsParticipants.Remove(eventParticipant);
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

                var events = await _context.Events
                    .Include(e => e.EventsParticipants)
                    .Where(e => e.EventsParticipants.Any(ep => ep.ParticipantsId == participantId))
                    .ToListAsync();

                return Ok(events);
            }
        }
    }
}
