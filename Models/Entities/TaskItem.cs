using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusDeadline.Models.Entities
{
    [Table("tasks")]
    public class TaskItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("subject_id")]
        public int SubjectId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("task_type")]
        public string TaskType { get; set; } = "Assignment"; // Assignment, Practical, Examination, Project, Other

        [Required]
        [MaxLength(20)]
        [Column("priority")]
        public string Priority { get; set; } = "medium"; // low, medium, high

        [Required]
        [Column("due_date")]
        public DateTime DueDate { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "pending"; // pending, in_progress, completed

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject? Subject { get; set; }

        public virtual ICollection<TaskStatusHistory> StatusHistories { get; set; } = new List<TaskStatusHistory>();
    }
}
