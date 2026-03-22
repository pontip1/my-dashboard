using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MyDashboard.Models.Entities;

[Table("profiles")]
public class Profile : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("email")]
    public string Email { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("points")]
    public int Points { get; set; }

    [Column("shop_points")]
    public int ShopPoints { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }
}