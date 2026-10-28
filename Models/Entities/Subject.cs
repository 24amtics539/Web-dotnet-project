using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusDeadline.Models.Entities
{
    [Table("subjects")]
    public class Subject
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("subject_name")]
        public string SubjectName { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("subject_code")]
        public string? SubjectCode { get; set; }

        [Range(1, 12)]
        [Column("semester")]
        public int Semester { get; set; } = 1;

        [MaxLength(20)]
        [Column("color")]
        public string Color { get; set; } = "#B98AFF";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
