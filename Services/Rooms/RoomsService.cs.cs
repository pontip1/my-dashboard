using MyDashboard.Models.Dtos;
using MyDashboard.Models.Entities;
using MyDashboard.Services.Backend;
using System.Text.Json;

namespace MyDashboard.Services.Rooms;

public class RoomsService
{
    public async Task<Room?> GetRoomById(long id)
    {
        var response = await SupabaseService.Client
            .From<Room>()
            .Where(x => x.Id == id)
            .Get();

        return response.Models.FirstOrDefault();
    }

    public async Task<List<RoomMember>> GetMyRoomMembers()
    {
        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
            return new List<RoomMember>();

        var userId = Guid.Parse(user.Id);

        var response = await SupabaseService.Client
            .From<RoomMember>()
            .Where(x => x.UserId == userId)
            .Get();

        return response.Models;
    }

    public async Task<Room?> CreateRoom(string roomName, string roomKey)
    {
        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
            throw new Exception("User is not authorized");

        var room = new Room
        {
            RoomName = roomName,
            RoomKey = roomKey,
            OwnerId = Guid.Parse(user.Id)
        };

        var insertedRoomResponse = await SupabaseService.Client
            .From<Room>()
            .Insert(room);

        var insertedRoom = insertedRoomResponse.Models.FirstOrDefault();

        if (insertedRoom == null)
            throw new Exception("Room was not created");

        var creatorMember = new RoomMember
        {
            RoomId = insertedRoom.Id,
            UserId = Guid.Parse(user.Id)
        };

        await SupabaseService.Client
            .From<RoomMember>()
            .Insert(creatorMember);

        return insertedRoom;
    }

    public async Task JoinRoomByKey(string key)
    {
        await SupabaseService.Client
            .Rpc("join_room_by_key", new Dictionary<string, object>
            {
                { "p_room_key", key }
            });
    }

    public async Task<List<RoomMemberInfo>> GetMembersByRoomId(long roomId)
    {
        var result = await SupabaseService.Client
            .Rpc("get_room_members", new Dictionary<string, object>
            {
                { "p_room_id", roomId }
            });

        var json = result.Content;

        var members = JsonSerializer.Deserialize<List<RoomMemberInfo>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return members ?? new List<RoomMemberInfo>();
    }

    public async Task RemoveMember(long roomId, Guid userId)
    {
        await SupabaseService.Client
            .Rpc("remove_room_member", new Dictionary<string, object>
            {
                { "p_room_id", roomId },
                { "p_user_id", userId }
            });
    }

    public async Task LeaveRoom(long roomId, Guid userId)
    {
        var response = await SupabaseService.Client
            .From<RoomMember>()
            .Where(x => x.RoomId == roomId && x.UserId == userId)
            .Get();

        var member = response.Models.FirstOrDefault();

        if (member != null)
            await member.Delete<RoomMember>();
    }

    public async Task DeleteRoom(Room room)
    {
        var membersResponse = await SupabaseService.Client
            .From<RoomMember>()
            .Where(x => x.RoomId == room.Id)
            .Get();

        foreach (var member in membersResponse.Models)
            await member.Delete<RoomMember>();

        var tasksResponse = await SupabaseService.Client
            .From<TaskItem>()
            .Where(x => x.RoomId == room.Id)
            .Get();

        foreach (var task in tasksResponse.Models)
            await task.Delete<TaskItem>();

        await room.Delete<Room>();
    }
}