using MyDashboard.Models.Entities;
using MyDashboard.Services.Backend;

namespace MyDashboard.Services.Tasks;

public class TasksService
{
    public async Task<List<TaskItem>> GetTasksByRoomId(long roomId)
    {
        var response = await SupabaseService.Client
            .From<TaskItem>()
            .Where(x => x.RoomId == roomId)
            .Get();

        return response.Models;
    }

    public async Task CreateTask(TaskItem taskItem)
    {
        await SupabaseService.Client
            .From<TaskItem>()
            .Insert(taskItem);
    }

    public async Task AcceptTask(long taskId)
    {
        await SupabaseService.Client
            .Rpc("accept_task_secure", new Dictionary<string, object>
            {
                { "p_task_id", taskId }
            });
    }

    public async Task UpdateTask(TaskItem taskItem)
    {
        await taskItem.Update<TaskItem>();
    }

    public async Task DeleteTask(TaskItem taskItem)
    {
        await taskItem.Delete<TaskItem>();
    }
}