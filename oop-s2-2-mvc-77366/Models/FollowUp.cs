namespace FoodInspectionService.Models
{
    public class FollowUp
    {
        public int Id { get; set; }
        public int InspectionId { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedDate { get; set; }

        public Inspection Inspection { get; set; }
    }
}
