using CommitteeCalendarAPI.ActionModels;
using CommitteeCalendarAPI.IMPLogic.Helpers;
using CommitteeCalendarAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        public async Task<ActionResult<EventRequestGet>> GetEvent(Guid id)
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
                return NotFound("Error: User not exist.");
            }

            var participantId = user.ParticipantsId.Value;

            var @event = await _context.Events.Include(e => e.EventsParticipants)
                                              .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
            {
                return NotFound("Error: Event not found.");
            }

            if (!@event.EventsParticipants.Any(ep => ep.ParticipantsId == participantId))
            {
                return Content("Error: You are not a participant in this event.");
            }

            var eventResponse = new EventRequestGet
            {
                EventId = @event.EventId,
                EventName = @event.EventName,
                HostPerson = @event.HostPerson,
                StartDate = @event.StartDate,
                StartTime = @event.StartTime,
                Duration = @event.Duration,
                Detail = @event.Detail,
                LocationId = @event.LocationId,
                IsAppoved = @event.IsAppoved,
                ParticipantIds = @event.EventsParticipants.Select(ep => ep.ParticipantsId).ToList()
            };

            return Ok(eventResponse);
        }

        // PUT: api/Events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(Guid id, EventRequestPostPut eventRequest)
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
            //existingEvent.Participants = eventRequest.Participants;
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
        public async Task<ActionResult<Event>> PostEvent(EventRequestPostPut eventRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            bool isAdmin = await _authHelper.IsUserAdminAsync(User);

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
                IsAppoved = isAdmin ? true : false
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

        // PUT: api/Events/Approve/5
        [HttpPut("Approve/{id}")]
        public async Task<IActionResult> ApproveEvent(Guid id, [FromBody] bool isApproved)
        {
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Unauthorized("Admin permission required.");
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound("Event not found.");
            }

            @event.IsAppoved = isApproved;
            _context.Entry(@event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                {
                    return NotFound("Event not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // GET: api/Events/GetMyEvents
        [HttpGet("GetMyEvents")]
        public async Task<ActionResult<IEnumerable<EventRequestGet>>> GetMyEvents()
        {
            if (await _authHelper.IsUserAdminAsync(User))
            {
                var allEvents = await _context.Events.Include(e => e.EventsParticipants).ToListAsync();
                var eventResponses = allEvents.Select(e => new EventRequestGet
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    HostPerson = e.HostPerson,
                    StartDate = e.StartDate,
                    StartTime = e.StartTime,
                    Duration = e.Duration,
                    Detail = e.Detail,
                    LocationId = e.LocationId,
                    IsAppoved = e.IsAppoved,
                    ParticipantIds = e.EventsParticipants.Select(ep => ep.ParticipantsId).ToList()
                }).ToList();

                return Ok(eventResponses);
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.Name);
                var user = await _context.UserAccounts.Include(u => u.Participants).FirstOrDefaultAsync(u => u.Id.ToString() == userId);
                var participantId = user.ParticipantsId.Value;

                var events = await _context.Events
                    .Include(e => e.EventsParticipants)
                    .Where(e => e.EventsParticipants.Any(ep => ep.ParticipantsId == participantId))
                    .ToListAsync();

                var eventResponses = events.Select(e => new EventRequestGet
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    HostPerson = e.HostPerson,
                    StartDate = e.StartDate,
                    StartTime = e.StartTime,
                    Duration = e.Duration,
                    Detail = e.Detail,
                    LocationId = e.LocationId,
                    IsAppoved = e.IsAppoved,
                    ParticipantIds = e.EventsParticipants.Select(ep => ep.ParticipantsId).ToList()
                }).ToList();

                return Ok(eventResponses);
            }
        }
    }
}
