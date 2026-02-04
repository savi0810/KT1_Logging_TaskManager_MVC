using System.ComponentModel.DataAnnotations;

namespace KT1_Logging_TaskManager_MVC.Models
{
    public class CurrentTasks
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Task name is required")]
        [Display(Name = "Task Name")]
        public string TaskName { get; set; } = string.Empty;

        [Display(Name = "Task Description")]
        public string TaskDescription { get; set; } = string.Empty;

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Medium";

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}