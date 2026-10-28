using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusDeadline.Models.Entities
{
    [Table("task_status_history")]
    public class TaskStatusHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("task_id")]
        public int TaskItemId { get; set; }

        [MaxLength(20)]
        [Column("old_status")]
        public string? OldStatus { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("new_status")]
        public string NewStatus { get; set; } = string.Empty;

        [Column("changed_at")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("TaskItemId")]
        public virtual TaskItem? TaskItem { get; set; }
    }
}
