using FoodInspectionService.Data;
using FoodInspectionService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FoodInspectionService.Tests
{
    public class DashboardAndFollowUpTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Overdue_Open_FollowUps_Query_Returns_Correct_Count()
        {
            // Arrange
            using var context = GetDbContext(nameof(Overdue_Open_FollowUps_Query_Returns_Correct_Count));

            var premises = new Premises
            {
                Id = 1,
                Name = "Test Premises",
                Address = "Test Address",
                Town = "Dublin",
                RiskRating = "High"
            };

            var inspection = new Inspection
            {
                Id = 1,
                PremisesId = 1,
                InspectionDate = DateTime.Today.AddDays(-10),
                Score = 40,
                Outcome = "Fail",
                Notes = "Test"
            };

            context.Premises.Add(premises);
            context.Inspections.Add(inspection);

            context.FollowUps.AddRange(
                new FollowUp
                {
                    Id = 1,
                    InspectionId = 1,
                    DueDate = DateTime.Today.AddDays(-2),
                    Status = "Open",
                    ClosedDate = null
                },
                new FollowUp
                {
                    Id = 2,
                    InspectionId = 1,
                    DueDate = DateTime.Today.AddDays(2),
                    Status = "Open",
                    ClosedDate = null
                },
                new FollowUp
                {
                    Id = 3,
                    InspectionId = 1,
                    DueDate = DateTime.Today.AddDays(-5),
                    Status = "Closed",
                    ClosedDate = DateTime.Today.AddDays(-1)
                }
            );

            context.SaveChanges();

            // Act
            var overdueCount = context.FollowUps
                .Count(f => f.Status == "Open" && f.DueDate < DateTime.Today);

            // Assert
            Assert.Equal(1, overdueCount);
        }

        [Fact]
        public void Closed_FollowUp_Must_Have_ClosedDate()
        {
            // Arrange
            var followUp = new FollowUp
            {
                Id = 1,
                InspectionId = 1,
                DueDate = DateTime.Today,
                Status = "Closed",
                ClosedDate = null
            };

            // Act
            var isValid = !(followUp.Status == "Closed" && followUp.ClosedDate == null);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void Dashboard_Failed_Inspections_This_Month_Returns_Correct_Count()
        {
            // Arrange
            using var context = GetDbContext(nameof(Dashboard_Failed_Inspections_This_Month_Returns_Correct_Count));

            var premises = new Premises
            {
                Id = 1,
                Name = "Test Premises",
                Address = "Test Address",
                Town = "Cork",
                RiskRating = "Medium"
            };

            context.Premises.Add(premises);

            context.Inspections.AddRange(
                new Inspection
                {
                    Id = 1,
                    PremisesId = 1,
                    InspectionDate = DateTime.Today.AddDays(-2),
                    Score = 45,
                    Outcome = "Fail",
                    Notes = "Fail this month"
                },
                new Inspection
                {
                    Id = 2,
                    PremisesId = 1,
                    InspectionDate = DateTime.Today.AddDays(-3),
                    Score = 80,
                    Outcome = "Pass",
                    Notes = "Pass this month"
                },
                new Inspection
                {
                    Id = 3,
                    PremisesId = 1,
                    InspectionDate = DateTime.Today.AddMonths(-1),
                    Score = 30,
                    Outcome = "Fail",
                    Notes = "Fail last month"
                }
            );

            context.SaveChanges();

            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);

            // Act
            var failedThisMonth = context.Inspections.Count(i =>
                i.InspectionDate >= startOfMonth &&
                i.InspectionDate < startOfNextMonth &&
                i.Outcome == "Fail");

            // Assert
            Assert.Equal(1, failedThisMonth);
        }

        [Fact]
        public void Dashboard_Filter_By_Town_Returns_Correct_Inspection_Count()
        {
            // Arrange
            using var context = GetDbContext(nameof(Dashboard_Filter_By_Town_Returns_Correct_Inspection_Count));

            context.Premises.AddRange(
                new Premises
                {
                    Id = 1,
                    Name = "Premises 1",
                    Address = "Addr 1",
                    Town = "Dublin",
                    RiskRating = "High"
                },
                new Premises
                {
                    Id = 2,
                    Name = "Premises 2",
                    Address = "Addr 2",
                    Town = "Galway",
                    RiskRating = "Low"
                }
            );

            context.Inspections.AddRange(
                new Inspection
                {
                    Id = 1,
                    PremisesId = 1,
                    InspectionDate = DateTime.Today,
                    Score = 70,
                    Outcome = "Pass",
                    Notes = "Dublin inspection"
                },
                new Inspection
                {
                    Id = 2,
                    PremisesId = 2,
                    InspectionDate = DateTime.Today,
                    Score = 55,
                    Outcome = "Pass",
                    Notes = "Galway inspection"
                }
            );

            context.SaveChanges();

            // Act
            var dublinInspectionCount = context.Inspections
                .Include(i => i.Premises)
                .Count(i => i.Premises != null && i.Premises.Town == "Dublin");

            // Assert
            Assert.Equal(1, dublinInspectionCount);
        }
    }
}
