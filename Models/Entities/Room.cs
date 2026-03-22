using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MyDashboard.Models.Entities
{
    [Table("rooms")]
    public class Room: BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("room_key")]
        public string RoomKey { get; set; }

        [Column("owner_id")]
        public Guid OwnerId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("room_name")]
        public string? RoomName { get; set; }
    }
}
