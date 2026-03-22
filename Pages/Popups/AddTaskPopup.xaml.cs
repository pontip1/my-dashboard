using CommunityToolkit.Maui.Views;
using MyDashboard.Models.Entities;

namespace MyDashboard.Pages;

public partial class AddTaskPopup : Popup
{
    private readonly Action<TaskItem> _onTaskCreated;

    public AddTaskPopup(Action<TaskItem> onTaskCreated)
    {
        InitializeComponent();
        _onTaskCreated = onTaskCreated;
    }

    private async void Create_Clicked(object sender, EventArgs e)
    {
        string name = NameOfTask.Text;
        string description = Task.Text;
        string taskType = TaskTypePicker.SelectedItem?.ToString() ?? "backend";

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (!int.TryParse(Points.Text, out int points))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                "Points must be a number",
                "OK");

            return;
        }

        if (points < 1 || points > 10)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                "Points must be from 1 to 10",
                "OK");

            return;
        }

        var taskItem = new TaskItem
        {
            Name = name,
            Description = description,
            Points = points,
            TaskType = taskType,
            Status = "todo"
        };

        _onTaskCreated?.Invoke(taskItem);

        await CloseAsync();
    }
}