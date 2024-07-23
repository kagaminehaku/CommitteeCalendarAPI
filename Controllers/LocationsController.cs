using CommitteeCalendarAPI.ActionModels;
using CommitteeCalendarAPI.BUS.Helpers;
using CommitteeCalendarAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommitteeCalendarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly CommitteeCalendarContext _context;
        private readonly AuthorizationHelper _authHelper;

        public LocationsController(CommitteeCalendarContext context)
        {
            _context = context;
            _authHelper = new AuthorizationHelper(_context);
        }

        // GET: api/Locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationsMinimal>>> GetLocations()
        {
            var locations = await _context.Locations.ToListAsync();

            var locationsMinimal = locations.Select(location => new LocationsMinimal
            {
                LocationName = location.LocationName,
                LocationAddress = location.LocationAddress,
                LocationInfo = location.LocationInfo,
                LocationContact = location.LocationContact
            }).ToList();

            return Ok(locationsMinimal);
        }

        // GET: api/Locations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationsMinimal>> GetLocation(Guid id)
        {
            var location = await _context.Locations.FindAsync(id);

            if (location == null)
            {
                return NotFound();
            }

            var locationMinimal = new LocationsMinimal
            {
                LocationName = location.LocationName,
                LocationAddress = location.LocationAddress,
                LocationInfo = location.LocationInfo,
                LocationContact = location.LocationContact
            };

            return Ok(locationMinimal);
        }


        // PUT: api/Locations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(Guid id, LocationsMinimal locationMinimal)
        {
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Content("Unauthorized: Admin permission required.");
            }

            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            location.LocationName = locationMinimal.LocationName;
            location.LocationAddress = locationMinimal.LocationAddress;
            location.LocationInfo = locationMinimal.LocationInfo;
            location.LocationContact = locationMinimal.LocationContact;

            _context.Entry(location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok("Ok");
        }

        // POST: api/Locations
        [HttpPost]
        public async Task<ActionResult<Location>> PostLocation(LocationsMinimal locationMinimal)
        {
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Content("Unauthorized: Admin permission required.");
            }

            var location = new Location
            {
                LocationId = Guid.NewGuid(),
                LocationName = locationMinimal.LocationName,
                LocationAddress = locationMinimal.LocationAddress,
                LocationInfo = locationMinimal.LocationInfo,
                LocationContact = locationMinimal.LocationContact,
            };

            _context.Locations.Add(location);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LocationExists(location.LocationId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Ok("Ok");
        }

        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(Guid id)
        {
            if (!await _authHelper.IsUserAdminAsync(User))
            {
                return Content("Unauthorized: Admin permission required.");
            }

            var location = await _context.Locations
                .Include(l => l.Events)
                .ThenInclude(e => e.EventsParticipants)
                .FirstOrDefaultAsync(l => l.LocationId == id);

            if (location == null)
            {
                return NotFound();
            }

            foreach (var @event in location.Events)
            {
                foreach (var eventParticipant in @event.EventsParticipants)
                {
                    _context.EventsParticipants.Remove(eventParticipant);
                }
                _context.Events.Remove(@event);
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return Ok("Ok");
        }

        private bool LocationExists(Guid id)
        {
            return _context.Locations.Any(e => e.LocationId == id);
        }
    }
}
