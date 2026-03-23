using FoodInspectionService.Data;
using FoodInspectionService.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodInspectionService.Controllers
{
    [Authorize(Roles = "Admin,Viewer,Inspector")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? town, string? riskRating)
        {
            try
            {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var startOfNextMonth = startOfMonth.AddMonths(1);

                var inspectionsQuery = _context.Inspections
                    .Include(i => i.Premises)
                    .AsQueryable();

                var followUpsQuery = _context.FollowUps
                    .Include(f => f.Inspection)
                    .ThenInclude(i => i.Premises)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(town))
                {
                    inspectionsQuery = inspectionsQuery.Where(i => i.Premises != null && i.Premises.Town == town);
                    followUpsQuery = followUpsQuery.Where(f => f.Inspection != null &&
                                                               f.Inspection.Premises != null &&
                                                               f.Inspection.Premises.Town == town);
                }

                if (!string.IsNullOrEmpty(riskRating))
                {
                    inspectionsQuery = inspectionsQuery.Where(i => i.Premises != null && i.Premises.RiskRating == riskRating);
                    followUpsQuery = followUpsQuery.Where(f => f.Inspection != null &&
                                                               f.Inspection.Premises != null &&
                                                               f.Inspection.Premises.RiskRating == riskRating);
                }

                var inspectionsThisMonth = await inspectionsQuery
                    .Where(i => i.InspectionDate >= startOfMonth && i.InspectionDate < startOfNextMonth)
                    .CountAsync();

                var failedInspectionsThisMonth = await inspectionsQuery
                    .Where(i => i.InspectionDate >= startOfMonth &&
                                i.InspectionDate < startOfNextMonth &&
                                i.Outcome == "Fail")
                    .CountAsync();

                var overdueOpenFollowUps = await followUpsQuery
                    .Where(f => f.Status == "Open" && f.DueDate < today)
                    .CountAsync();

                var vm = new DashboardViewModel
                {
                    InspectionsThisMonth = inspectionsThisMonth,
                    FailedInspectionsThisMonth = failedInspectionsThisMonth,
                    OverdueOpenFollowUps = overdueOpenFollowUps,
                    SelectedTown = town,
                    SelectedRiskRating = riskRating,
                    Towns = await _context.Premises
                        .Select(p => p.Town)
                        .Distinct()
                        .OrderBy(t => t)
                        .ToListAsync(),
                    RiskRatings = await _context.Premises
                        .Select(p => p.RiskRating)
                        .Distinct()
                        .OrderBy(r => r)
                        .ToListAsync()
                };

                _logger.LogInformation(
                    "Dashboard viewed. TownFilter: {Town}, RiskRatingFilter: {RiskRating}, InspectionsThisMonth: {InspectionsThisMonth}, FailedInspectionsThisMonth: {FailedInspectionsThisMonth}, OverdueOpenFollowUps: {OverdueOpenFollowUps}",
                    town, riskRating, inspectionsThisMonth, failedInspectionsThisMonth, overdueOpenFollowUps);

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard. TownFilter: {Town}, RiskRatingFilter: {RiskRating}", town, riskRating);
                throw;
            }
        }
    }
}
