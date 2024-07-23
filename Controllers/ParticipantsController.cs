using CommitteeCalendarAPI.ActionModels;
using CommitteeCalendarAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommitteeCalendarAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParticipantsController : ControllerBase
    {
        private readonly CommitteeCalendarContext _context;

        public ParticipantsController(CommitteeCalendarContext context)
        {
            _context = context;
        }

        // GET: api/Participants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParticipantGet>>> GetParticipants()
        {
            var participants = await _context.Participants
                .Select(p => new ParticipantGet
                {
                    Id = p.ParticipantsId,
                    ParticipantsName = p.ParticipantsName
                })
                .ToListAsync();

            return Ok(participants);
        }

        // GET: api/Participants/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ParticipantGet>> GetParticipant(Guid id)
        {
            var participant = await _context.Participants
                .Select(p => new ParticipantGet
                {
                    Id = p.ParticipantsId,
                    ParticipantsName = p.ParticipantsName
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            if (participant == null)
            {
                return NotFound();
            }

            return Ok(participant);
        }

        // PUT: api/Participants/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutParticipant(Guid id, ParticipantPutPost participantPutPost)
        {
            var participant = await _context.Participants.FindAsync(id);

            if (participant == null)
            {
                return NotFound();
            }

            participant.ParticipantsName = participantPutPost.ParticipantsName;
            participant.ParticipantsRepresentative = participantPutPost.ParticipantsRepresentative;
            participant.ParticipantsPhonenumber = participantPutPost.ParticipantsPhonenumber;
            participant.ParticipantsEmail = participantPutPost.ParticipantsEmail;

            _context.Entry(participant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParticipantExists(id))
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

        private bool ParticipantExists(Guid id)
        {
            return _context.Participants.Any(e => e.ParticipantsId == id);
        }
    }
}
