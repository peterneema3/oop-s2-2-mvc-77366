using oop_s2_2_mvc_77366.Models;
using Bogus;

namespace oop_s2_2_mvc_77366.Data
{

    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (context.Premises.Any()) return; // already seeded

            var towns = new[] { "Dublin", "Cork", "Galway" };
            var risks = new[] { "Low", "Medium", "High" };

            var premisesFaker = new Faker<Premises>()
                .RuleFor(p => p.Name, f => f.Company.CompanyName())
                .RuleFor(p => p.Address, f => f.Address.StreetAddress())
                .RuleFor(p => p.Town, f => f.PickRandom(towns))
                .RuleFor(p => p.RiskRating, f => f.PickRandom(risks));

            var premisesList = premisesFaker.Generate(12);
            context.Premises.AddRange(premisesList);
            context.SaveChanges();

            var inspections = new List<Inspection>();
            var random = new Random();

            foreach (var p in premisesList)
            {
                for (int i = 0; i < 2; i++)
                {
                    var score = random.Next(30, 100);

                    inspections.Add(new Inspection
                    {
                        PremisesId = p.Id,
                        InspectionDate = DateTime.Now.AddDays(-random.Next(1, 60)),
                        Score = score,
                        Outcome = score < 50 ? "Fail" : "Pass",
                        Notes = "Auto-generated inspection"
                    });
                }
            }

            context.Inspections.AddRange(inspections);
            context.SaveChanges();

            var followUps = inspections
                .Where(i => i.Outcome == "Fail")
                .Take(10)
                .Select(i => new FollowUp
                {
                    InspectionId = i.Id,
                    DueDate = DateTime.Now.AddDays(-random.Next(1, 10)),
                    Status = "Open"
                }).ToList();

            context.FollowUps.AddRange(followUps);
            context.SaveChanges();
        }
    }
}
