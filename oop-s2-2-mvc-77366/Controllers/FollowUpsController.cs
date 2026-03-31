using FoodInspectionService.Data;
using FoodInspectionService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodInspectionService.Controllers
{
    [Authorize]
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FollowUpsController> _logger;

        public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: FollowUps
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Follow-up list viewed.");

            var applicationDbContext = _context.FollowUps
                .Include(f => f.Inspection);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: FollowUps/Details/5
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Follow-up details requested with null id.");
                return NotFound();
            }

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null)
            {
                _logger.LogWarning("Follow-up details not found. FollowUpId: {FollowUpId}", id);
                return NotFound();
            }

            _logger.LogInformation(
                "Follow-up details viewed. FollowUpId: {FollowUpId}, InspectionId: {InspectionId}",
                followUp.Id, followUp.InspectionId);

            return View(followUp);
        }

        // GET: FollowUps/Create
        [Authorize(Roles = "Admin,Inspector")]
        public IActionResult Create()
        {
            _logger.LogInformation("Follow-up create page viewed.");

            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id");
            return View();
        }

        // POST: FollowUps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create([Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Follow-up creation failed validation. InspectionId: {InspectionId}, Status: {Status}",
                        followUp.InspectionId, followUp.Status);

                    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
                    return View(followUp);
                }

                var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);

                if (inspection == null)
                {
                    _logger.LogWarning(
                        "Follow-up creation failed. Inspection not found. InspectionId: {InspectionId}",
                        followUp.InspectionId);

                    ModelState.AddModelError("", "Selected inspection does not exist.");
                    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
                    return View(followUp);
                }

                // Business rule: Due date must not be before inspection date
                if (followUp.DueDate < inspection.InspectionDate)
                {
                    _logger.LogWarning(
                        "Follow-up creation blocked. InspectionId: {InspectionId}, DueDate: {DueDate} is before InspectionDate: {InspectionDate}",
                        followUp.InspectionId, followUp.DueDate, inspection.InspectionDate);

                    ModelState.AddModelError("", "Due date cannot be before inspection date.");
                    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
                    return View(followUp);
                }

                _context.Add(followUp);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Follow-up created. FollowUpId: {FollowUpId}, InspectionId: {InspectionId}, DueDate: {DueDate}, Status: {Status}",
                    followUp.Id, followUp.InspectionId, followUp.DueDate, followUp.Status);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating follow-up. InspectionId: {InspectionId}",
                    followUp.InspectionId);
                throw;
            }
        }

        // GET: FollowUps/Edit/5
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Follow-up edit requested with null id.");
                return NotFound();
            }

            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp == null)
            {
                _logger.LogWarning("Follow-up edit target not found. FollowUpId: {FollowUpId}", id);
                return NotFound();
            }

            _logger.LogInformation("Follow-up edit page viewed. FollowUpId: {FollowUpId}", followUp.Id);

            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
            return View(followUp);
        }

        // POST: FollowUps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (id != followUp.Id)
            {
                _logger.LogWarning(
                    "Follow-up edit id mismatch. RouteId: {RouteId}, FollowUpId: {FollowUpId}",
                    id, followUp.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Follow-up update failed validation. FollowUpId: {FollowUpId}",
                    followUp.Id);

                ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
                return View(followUp);
            }

            try
            {
                var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);

                if (inspection == null)
                {
                    _logger.LogWarning(
                        "Follow-up update failed. Inspection not found. FollowUpId: {FollowUpId}, InspectionId: {InspectionId}",
                        followUp.Id, followUp.InspectionId);

                    ModelState.AddModelError("", "Selected inspection does not exist.");
                    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
                    return View(followUp);
                }

                // Business rule: Due date must not be before inspection date
                if (followUp.DueDate < inspection.InspectionDate)
                {
                    _logger.LogWarning(
                        "Follow-up update blocked. FollowUpId: {FollowUpId}, DueDate: {DueDate} is before InspectionDate: {InspectionDate}",
                        followUp.Id, followUp.DueDate, inspection.InspectionDate);

                    ModelState.AddModelError("", "Due date cannot be before inspection date.");
                    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
                    return View(followUp);
                }

                // Business rule: Closed status must have ClosedDate
                if (followUp.Status == "Closed" && followUp.ClosedDate == null)
                {
                    _logger.LogWarning(
                        "Follow-up close blocked. FollowUpId: {FollowUpId}, ClosedDate missing.",
                        followUp.Id);

                    ModelState.AddModelError("", "Closed follow-up must have a closed date.");
                    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", followUp.InspectionId);
                    return View(followUp);
                }

                _context.Update(followUp);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Follow-up updated. FollowUpId: {FollowUpId}, Status: {Status}, ClosedDate: {ClosedDate}",
                    followUp.Id, followUp.Status, followUp.ClosedDate);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!FollowUpExists(followUp.Id))
                {
                    _logger.LogWarning(
                        "Follow-up update failed because record not found. FollowUpId: {FollowUpId}",
                        followUp.Id);
                    return NotFound();
                }

                _logger.LogError(ex,
                    "Concurrency error updating follow-up. FollowUpId: {FollowUpId}",
                    followUp.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating follow-up. FollowUpId: {FollowUpId}",
                    followUp.Id);
                throw;
            }
        }

        // GET: FollowUps/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Follow-up delete requested with null id.");
                return NotFound();
            }

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null)
            {
                _logger.LogWarning("Follow-up delete target not found. FollowUpId: {FollowUpId}", id);
                return NotFound();
            }

            _logger.LogInformation("Follow-up delete page viewed. FollowUpId: {FollowUpId}", followUp.Id);

            return View(followUp);
        }

        // POST: FollowUps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var followUp = await _context.FollowUps.FindAsync(id);

                if (followUp == null)
                {
                    _logger.LogWarning(
                        "Follow-up delete failed because record not found. FollowUpId: {FollowUpId}",
                        id);
                    return NotFound();
                }

                _context.FollowUps.Remove(followUp);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Follow-up deleted. FollowUpId: {FollowUpId}, InspectionId: {InspectionId}",
                    followUp.Id, followUp.InspectionId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deleting follow-up. FollowUpId: {FollowUpId}",
                    id);
                throw;
            }
        }

        private bool FollowUpExists(int id)
        {
            return _context.FollowUps.Any(e => e.Id == id);
        }
    }
}