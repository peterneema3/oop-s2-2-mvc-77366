using Bogus;
using FoodInspectionService.Models;

namespace FoodInspectionService.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (context.Premises.Any())
                return;

            var towns = new[] { "Dublin", "Cork", "Galway" };
            var risks = new[] { "Low", "Medium", "High" };
            var random = new Random();

            var premisesFaker = new Faker<Premises>()
                .RuleFor(p => p.Name, f => f.Company.CompanyName())
                .RuleFor(p => p.Address, f => f.Address.StreetAddress())
                .RuleFor(p => p.Town, f => f.PickRandom(towns))
                .RuleFor(p => p.RiskRating, f => f.PickRandom(risks));

            var premisesList = premisesFaker.Generate(12);
            context.Premises.AddRange(premisesList);
            context.SaveChanges();

            var inspections = new List<Inspection>();

            foreach (var p in premisesList)
            {
                for (int i = 0; i < 2; i++)
                {
                    var score = random.Next(30, 101);

                    inspections.Add(new Inspection
                    {
                        PremisesId = p.Id,
                        InspectionDate = DateTime.Today.AddDays(-random.Next(1, 60)),
                        Score = score,
                        Outcome = score < 50 ? "Fail" : "Pass",
                        Notes = "Auto-generated inspection"
                    });
                }
            }

            // Add 1 extra inspection to make total = 25
            var extraPremises = premisesList[random.Next(premisesList.Count)];
            var extraScore = random.Next(30, 101);

            inspections.Add(new Inspection
            {
                PremisesId = extraPremises.Id,
                InspectionDate = DateTime.Today.AddDays(-random.Next(1, 60)),
                Score = extraScore,
                Outcome = extraScore < 50 ? "Fail" : "Pass",
                Notes = "Extra auto-generated inspection"
            });

            context.Inspections.AddRange(inspections);
            context.SaveChanges();

            var failedInspections = inspections
                .Where(i => i.Outcome == "Fail")
                .Take(10)
                .ToList();

            var followUps = new List<FollowUp>();

            for (int i = 0; i < failedInspections.Count; i++)
            {
                var inspection = failedInspections[i];

                if (i < 4)
                {
                    // overdue open
                    followUps.Add(new FollowUp
                    {
                        InspectionId = inspection.Id,
                        DueDate = DateTime.Today.AddDays(-random.Next(1, 10)),
                        Status = "Open",
                        ClosedDate = null
                    });
                }
                else if (i < 7)
                {
                    // open but not overdue
                    followUps.Add(new FollowUp
                    {
                        InspectionId = inspection.Id,
                        DueDate = DateTime.Today.AddDays(random.Next(1, 10)),
                        Status = "Open",
                        ClosedDate = null
                    });
                }
                else
                {
                    // closed
                    var dueDate = DateTime.Today.AddDays(-random.Next(5, 15));
                    followUps.Add(new FollowUp
                    {
                        InspectionId = inspection.Id,
                        DueDate = dueDate,
                        Status = "Closed",
                        ClosedDate = dueDate.AddDays(random.Next(1, 5))
                    });
                }
            }

            context.FollowUps.AddRange(followUps);
            context.SaveChanges();
        }
    }
}