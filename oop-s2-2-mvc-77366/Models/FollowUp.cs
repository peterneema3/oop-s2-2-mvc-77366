namespace oop_s2_2_mvc_77366.Models
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
