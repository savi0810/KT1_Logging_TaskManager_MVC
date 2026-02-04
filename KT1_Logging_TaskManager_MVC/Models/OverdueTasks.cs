using System.ComponentModel.DataAnnotations;

namespace KT1_Logging_TaskManager_MVC.Models
{
    public class OverdueTasks
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Task name is required")]
        [Display(Name = "Task Name")]
        public string TaskName { get; set; } = string.Empty;

        [Display(Name = "Task Description")]
        public string TaskDescription { get; set; } = string.Empty;

        [Display(Name = "When Was Overdue Date")]
        public DateTime WhenOverdueDate { get; set; } = DateTime.Now;

    }
}
