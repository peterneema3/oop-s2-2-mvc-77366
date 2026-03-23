namespace FoodInspectionService.ViewModels
{
    public class DashboardViewModel
    {
        public int InspectionsThisMonth { get; set; }
        public int FailedInspectionsThisMonth { get; set; }
        public int OverdueOpenFollowUps { get; set; }

        public string? SelectedTown { get; set; }
        public string? SelectedRiskRating { get; set; }

        public List<string> Towns { get; set; } = new();
        public List<string> RiskRatings { get; set; } = new();
    }
}