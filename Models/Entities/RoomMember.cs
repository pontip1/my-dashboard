using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MyDashboard.Models.Entities;

[Table("room_members")]
public class RoomMember : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("room_id")]
    public long RoomId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("joined_at")]
    public DateTime JoinedAt { get; set; }
}