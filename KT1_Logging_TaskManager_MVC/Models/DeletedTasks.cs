using System.ComponentModel.DataAnnotations;

namespace KT1_Logging_TaskManager_MVC.Models
{
    public class DeletedTasks
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Task name is required")]
        [Display(Name = "Task Name")]
        public string TaskName { get; set; } = string.Empty;

        [Display(Name = "Task Description")]
        public string TaskDescription { get; set; } = string.Empty;

        [Display(Name = "Deleted Date")]
        public DateTime DeletedDate { get; set; } = DateTime.Now;
    }
}
