using FoodInspectionService.Data;
using FoodInspectionService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodInspectionService.Controllers
{
    [Authorize]
    public class InspectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InspectionsController> _logger;

        public InspectionsController(ApplicationDbContext context, ILogger<InspectionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Inspections
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Inspection list viewed.");

            var applicationDbContext = _context.Inspections
                .Include(i => i.Premises);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Inspections/Details/5
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Inspection details requested with null id.");
                return NotFound();
            }

            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inspection == null)
            {
                _logger.LogWarning("Inspection details not found. InspectionId: {InspectionId}", id);
                return NotFound();
            }

            _logger.LogInformation(
                "Inspection details viewed. InspectionId: {InspectionId}, PremisesId: {PremisesId}",
                inspection.Id, inspection.PremisesId);

            return View(inspection);
        }

        // GET: Inspections/Create
        [Authorize(Roles = "Admin,Inspector")]
        public IActionResult Create()
        {
            _logger.LogInformation("Inspection create page viewed.");

            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Id");
            return View();
        }

        // POST: Inspections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create([Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Inspection creation failed validation. PremisesId: {PremisesId}, Score: {Score}",
                        inspection.PremisesId, inspection.Score);

                    ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Id", inspection.PremisesId);
                    return View(inspection);
                }

                var premises = await _context.Premises.FindAsync(inspection.PremisesId);
                if (premises == null)
                {
                    _logger.LogWarning(
                        "Inspection creation failed. Premises not found. PremisesId: {PremisesId}",
                        inspection.PremisesId);

                    ModelState.AddModelError("", "Selected premises does not exist.");
                    ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Id", inspection.PremisesId);
                    return View(inspection);
                }

                if (inspection.Score < 0 || inspection.Score > 100)
                {
                    _logger.LogWarning(
                        "Inspection creation blocked. Invalid score. PremisesId: {PremisesId}, Score: {Score}",
                        inspection.PremisesId, inspection.Score);

                    ModelState.AddModelError("", "Score must be between 0 and 100.");
                    ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Id", inspection.PremisesId);
                    return View(inspection);
                }

                _context.Add(inspection);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Inspection created. InspectionId: {InspectionId}, PremisesId: {PremisesId}, Score: {Score}, Outcome: {Outcome}",
                    inspection.Id, inspection.PremisesId, inspection.Score, inspection.Outcome);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating inspection. PremisesId: {PremisesId}",
                    inspection.PremisesId);

                throw;
            }
        }

        // GET: Inspections/Edit/5
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Inspection edit requested with null id.");
                return NotFound();
            }

            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null)
            {
                _logger.LogWarning("Inspection edit target not found. InspectionId: {InspectionId}", id);
                return NotFound();
            }

            _logger.LogInformation("Inspection edit page viewed. InspectionId: {InspectionId}", inspection.Id);

            ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Id", inspection.PremisesId);
            return View(inspection);
        }

        // POST: Inspections/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            if (id != inspection.Id)
            {
                _logger.LogWarning(
                    "Inspection edit id mismatch. RouteId: {RouteId}, InspectionId: {InspectionId}",
                    id, inspection.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Inspection update failed validation. InspectionId: {InspectionId}",
                    inspection.Id);

                ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Id", inspection.PremisesId);
                return View(inspection);
            }

            try
            {
                var premises = await _context.Premises.FindAsync(inspection.PremisesId);
                if (premises == null)
                {
                    _logger.LogWarning(
                        "Inspection update failed. Premises not found. InspectionId: {InspectionId}, PremisesId: {PremisesId}",
                        inspection.Id, inspection.PremisesId);

                    ModelState.AddModelError("", "Selected premises does not exist.");
                    ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Id", inspection.PremisesId);
                    return View(inspection);
                }

                if (inspection.Score < 0 || inspection.Score > 100)
                {
                    _logger.LogWarning(
                        "Inspection update blocked. Invalid score. InspectionId: {InspectionId}, Score: {Score}",
                        inspection.Id, inspection.Score);

                    ModelState.AddModelError("", "Score must be between 0 and 100.");
                    ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Id", inspection.PremisesId);
                    return View(inspection);
                }

                _context.Update(inspection);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Inspection updated. InspectionId: {InspectionId}, PremisesId: {PremisesId}, Score: {Score}, Outcome: {Outcome}",
                    inspection.Id, inspection.PremisesId, inspection.Score, inspection.Outcome);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!InspectionExists(inspection.Id))
                {
                    _logger.LogWarning(
                        "Inspection update failed because record was not found. InspectionId: {InspectionId}",
                        inspection.Id);
                    return NotFound();
                }

                _logger.LogError(ex,
                    "Concurrency error updating inspection. InspectionId: {InspectionId}",
                    inspection.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating inspection. InspectionId: {InspectionId}",
                    inspection.Id);
                throw;
            }
        }

        // GET: Inspections/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Inspection delete requested with null id.");
                return NotFound();
            }

            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inspection == null)
            {
                _logger.LogWarning("Inspection delete target not found. InspectionId: {InspectionId}", id);
                return NotFound();
            }

            _logger.LogInformation("Inspection delete page viewed. InspectionId: {InspectionId}", inspection.Id);

            return View(inspection);
        }

        // POST: Inspections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var inspection = await _context.Inspections.FindAsync(id);

                if (inspection == null)
                {
                    _logger.LogWarning(
                        "Inspection delete failed because record was not found. InspectionId: {InspectionId}",
                        id);
                    return NotFound();
                }

                _context.Inspections.Remove(inspection);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Inspection deleted. InspectionId: {InspectionId}, PremisesId: {PremisesId}",
                    inspection.Id, inspection.PremisesId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deleting inspection. InspectionId: {InspectionId}",
                    id);
                throw;
            }
        }

        private bool InspectionExists(int id)
        {
            return _context.Inspections.Any(e => e.Id == id);
        }
    }
}