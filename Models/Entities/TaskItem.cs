using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MyDashboard.Models.Entities;

[Table("tasks")]
public class TaskItem : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("room_id")]
    public long RoomId { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("points")]
    public int Points { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("status")]
    public string Status { get; set; }

    [Column("assigned_user_id")]
    public Guid? AssignedUserId { get; set; }

    [Column("answer")]
    public string? Answer { get; set; }

    [Column("review_comment")]
    public string? ReviewComment { get; set; }

    [Column("task_type")]
    public string TaskType { get; set; }
}