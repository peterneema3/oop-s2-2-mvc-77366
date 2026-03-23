using FoodInspectionService.Data;
using FoodInspectionService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodInspectionService.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PremisesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PremisesController> _logger;

        public PremisesController(ApplicationDbContext context, ILogger<PremisesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Premises
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Premises list viewed.");
            return View(await _context.Premises.ToListAsync());
        }

        // GET: Premises/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Premises details requested with null id.");
                return NotFound();
            }

            var premises = await _context.Premises
                .FirstOrDefaultAsync(m => m.Id == id);

            if (premises == null)
            {
                _logger.LogWarning("Premises details not found. PremisesId: {PremisesId}", id);
                return NotFound();
            }

            _logger.LogInformation("Premises details viewed. PremisesId: {PremisesId}", premises.Id);
            return View(premises);
        }

        // GET: Premises/Create
        public IActionResult Create()
        {
            _logger.LogInformation("Premises create page viewed.");
            return View();
        }

        // POST: Premises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Premises creation failed validation. Name: {Name}, Town: {Town}",
                        premises.Name, premises.Town);
                    return View(premises);
                }

                _context.Add(premises);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Premises created. PremisesId: {PremisesId}, Name: {Name}, Town: {Town}, RiskRating: {RiskRating}",
                    premises.Id, premises.Name, premises.Town, premises.RiskRating);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating premises. Name: {Name}, Town: {Town}",
                    premises.Name, premises.Town);
                throw;
            }
        }

        // GET: Premises/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Premises edit requested with null id.");
                return NotFound();
            }

            var premises = await _context.Premises.FindAsync(id);
            if (premises == null)
            {
                _logger.LogWarning("Premises edit target not found. PremisesId: {PremisesId}", id);
                return NotFound();
            }

            _logger.LogInformation("Premises edit page viewed. PremisesId: {PremisesId}", premises.Id);
            return View(premises);
        }

        // POST: Premises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
        {
            if (id != premises.Id)
            {
                _logger.LogWarning("Premises edit id mismatch. RouteId: {RouteId}, PremisesId: {PremisesId}",
                    id, premises.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Premises update failed validation. PremisesId: {PremisesId}",
                    premises.Id);
                return View(premises);
            }

            try
            {
                _context.Update(premises);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Premises updated. PremisesId: {PremisesId}, Name: {Name}, Town: {Town}, RiskRating: {RiskRating}",
                    premises.Id, premises.Name, premises.Town, premises.RiskRating);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!PremisesExists(premises.Id))
                {
                    _logger.LogWarning("Premises update failed because record was not found. PremisesId: {PremisesId}",
                        premises.Id);
                    return NotFound();
                }

                _logger.LogError(ex, "Concurrency error updating premises. PremisesId: {PremisesId}",
                    premises.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating premises. PremisesId: {PremisesId}",
                    premises.Id);
                throw;
            }
        }

        // GET: Premises/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Premises delete requested with null id.");
                return NotFound();
            }

            var premises = await _context.Premises
                .FirstOrDefaultAsync(m => m.Id == id);

            if (premises == null)
            {
                _logger.LogWarning("Premises delete target not found. PremisesId: {PremisesId}", id);
                return NotFound();
            }

            _logger.LogInformation("Premises delete page viewed. PremisesId: {PremisesId}", premises.Id);
            return View(premises);
        }

        // POST: Premises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var premises = await _context.Premises.FindAsync(id);

                if (premises == null)
                {
                    _logger.LogWarning("Premises delete failed because record was not found. PremisesId: {PremisesId}", id);
                    return NotFound();
                }

                _context.Premises.Remove(premises);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Premises deleted. PremisesId: {PremisesId}, Name: {Name}",
                    premises.Id, premises.Name);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting premises. PremisesId: {PremisesId}", id);
                throw;
            }
        }

        private bool PremisesExists(int id)
        {
            return _context.Premises.Any(e => e.Id == id);
        }
    }
}