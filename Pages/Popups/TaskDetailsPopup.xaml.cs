using CommunityToolkit.Maui.Views;
using MyDashboard.Models.Entities;
using MyDashboard.Services.Backend;
using MyDashboard.Services.Tasks;

namespace MyDashboard.Pages;

public partial class TaskDetailsPopup : Popup
{
    private TaskItem _taskItem;
    private readonly Func<Task> _onTaskChanged;
    private readonly TasksService _tasksService = new();
    private Guid _roomOwnerId;

    public TaskDetailsPopup(TaskItem taskItem, Guid roomOwnerId, Func<Task> onTaskChanged)
    {
        InitializeComponent();

        _taskItem = taskItem;
        _roomOwnerId = roomOwnerId;
        _onTaskChanged = onTaskChanged;

        TaskNameLabel.Text = taskItem.Name;
        TaskDescriptionLabel.Text = taskItem.Description;
        TaskPointsLabel.Text = $"Points: {taskItem.Points}";

        LoadAssignedInfoOnStart();
        UpdateButtonsVisibility();
    }

    private async void LoadAssignedInfoOnStart()
    {
        await LoadAssignedInfo();
    }

    private async Task LoadAssignedInfo()
    {
        string name = await GetAssignedUserName();
        TaskAssignedLabel.Text = $"Taken by: {name}";
    }

    private async Task<string> GetAssignedUserName()
    {
        if (_taskItem.AssignedUserId == null)
            return "nobody";

        var response = await SupabaseService.Client
            .From<Profile>()
            .Where(x => x.Id == _taskItem.AssignedUserId.Value)
            .Get();

        var profile = response.Models.FirstOrDefault();

        if (profile == null)
            return "Unknown user";

        return !string.IsNullOrWhiteSpace(profile.Name)
            ? profile.Name
            : profile.Email ?? "Unknown user";
    }

    private async void TakeTask_Clicked(object sender, EventArgs e)
    {
        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "User is not authorized", "OK");
            return;
        }

        _taskItem.Status = "in_progress";
        _taskItem.AssignedUserId = Guid.Parse(user.Id);

        await _tasksService.UpdateTask(_taskItem);
        await LoadAssignedInfo();

        if (_onTaskChanged != null)
            await _onTaskChanged();

        await Application.Current.MainPage.DisplayAlert("Success", "Task taken", "OK");
        await CloseAsync();
    }

    private void UpdateButtonsVisibility()
    {
        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
            return;

        var currentUserId = Guid.Parse(user.Id);

        TakeTaskButton.IsVisible = false;
        GoToReviewButton.IsVisible = false;
        AcceptButton.IsVisible = false;
        ReturnButton.IsVisible = false;
        DeleteTaskButton.IsVisible = false;

        AnswerEditorBlock.IsVisible = false;
        AnswerEditor.IsVisible = false;

        AnswerViewBlock.IsVisible = false;
        AnswerTitleLabel.IsVisible = false;
        TaskAnswerBorder.IsVisible = false;
        TaskAnswerLabel.IsVisible = false;

        ReviewCommentBlock.IsVisible = false;
        ReviewCommentTitleLabel.IsVisible = false;
        ReviewCommentEditor.IsVisible = false;

        ReturnedCommentBlock.IsVisible = false;
        ReturnedCommentTitleLabel.IsVisible = false;
        ReturnedCommentBorder.IsVisible = false;
        ReturnedCommentLabel.IsVisible = false;

        if (_taskItem.Status == "todo")
        {
            TakeTaskButton.IsVisible = true;
        }
        else if (_taskItem.Status == "in_progress" && _taskItem.AssignedUserId == currentUserId)
        {
            AnswerEditorBlock.IsVisible = true;
            AnswerEditor.IsVisible = true;
            GoToReviewButton.IsVisible = true;

            if (!string.IsNullOrWhiteSpace(_taskItem.Answer))
                AnswerEditor.Text = _taskItem.Answer;
        }

        if (_taskItem.Status == "in_review" && IsCurrentUserOwner())
        {
            AnswerViewBlock.IsVisible = true;
            AnswerTitleLabel.IsVisible = true;
            TaskAnswerBorder.IsVisible = true;
            TaskAnswerLabel.IsVisible = true;
            TaskAnswerLabel.Text = _taskItem.Answer ?? "No answer";

            ReviewCommentBlock.IsVisible = true;
            ReviewCommentTitleLabel.IsVisible = true;
            ReviewCommentEditor.IsVisible = true;

            if (!string.IsNullOrWhiteSpace(_taskItem.ReviewComment))
                ReviewCommentEditor.Text = _taskItem.ReviewComment;

            AcceptButton.IsVisible = true;
            ReturnButton.IsVisible = true;
        }

        if (_taskItem.Status == "in_progress" &&
            _taskItem.AssignedUserId == currentUserId &&
            !string.IsNullOrWhiteSpace(_taskItem.ReviewComment))
        {
            ReturnedCommentBlock.IsVisible = true;
            ReturnedCommentTitleLabel.IsVisible = true;
            ReturnedCommentBorder.IsVisible = true;
            ReturnedCommentLabel.IsVisible = true;
            ReturnedCommentLabel.Text = _taskItem.ReviewComment;
        }

        if (IsCurrentUserOwner())
        {
            DeleteTaskButton.IsVisible = true;
        }
    }

    private async void GoToReview_Clicked(object sender, EventArgs e)
    {
        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "User is not authorized", "OK");
            return;
        }

        var currentUserId = Guid.Parse(user.Id);

        if (_taskItem.AssignedUserId != currentUserId)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Only assigned user can send task to review", "OK");
            return;
        }

        _taskItem.Answer = AnswerEditor.Text;
        _taskItem.Status = "in_review";

        await _tasksService.UpdateTask(_taskItem);

        if (_onTaskChanged != null)
            await _onTaskChanged();

        await Application.Current.MainPage.DisplayAlert("Success", "Task moved to review", "OK");
        await CloseAsync();
    }

    private bool IsCurrentUserOwner()
    {
        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
            return false;

        return _roomOwnerId == Guid.Parse(user.Id);
    }

    private async void Accept_Clicked(object sender, EventArgs e)
    {
        if (!IsCurrentUserOwner())
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Only owner can accept task", "OK");
            return;
        }

        try
        {
            await _tasksService.AcceptTask(_taskItem.Id);

            if (_onTaskChanged != null)
                await _onTaskChanged();

            await Application.Current.MainPage.DisplayAlert("Success", "Task accepted", "OK");
            await CloseAsync();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void Return_Clicked(object sender, EventArgs e)
    {
        if (!IsCurrentUserOwner())
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Only owner can return task", "OK");
            return;
        }

        _taskItem.Status = "in_progress";
        _taskItem.ReviewComment = ReviewCommentEditor.Text;

        await _tasksService.UpdateTask(_taskItem);

        if (_onTaskChanged != null)
            await _onTaskChanged();

        await Application.Current.MainPage.DisplayAlert("Success", "Task returned to in progress", "OK");
        await CloseAsync();
    }

    private async void DeleteTask_Clicked(object sender, EventArgs e)
    {
        if (!IsCurrentUserOwner())
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Only owner can delete tasks", "OK");
            return;
        }

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Delete task",
            "Are you sure you want to delete this task?",
            "Yes",
            "No");

        if (!confirm)
            return;

        await _tasksService.DeleteTask(_taskItem);

        if (_onTaskChanged != null)
            await _onTaskChanged();

        await Application.Current.MainPage.DisplayAlert("Success", "Task deleted", "OK");
        await CloseAsync();
    }

    private async void Close_Clicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}