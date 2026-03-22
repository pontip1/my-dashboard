using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using MyDashboard.Helpers;
using MyDashboard.Models.Entities;
using MyDashboard.Services.Backend;
using MyDashboard.Services.Rooms;
using MyDashboard.Services.Tasks;
using MyDashboard.Pages.Popups;

namespace MyDashboard.Pages;

public partial class DashBoardPage : ContentPage
{
    private readonly TasksService _tasksService = new();
    private readonly RoomsService _roomsService = new();
    private Room? _selectedRoom;
    private Button? _selectedRoomButton;
    private string? _initializedUserId;
    private bool _isRoomSwitching;
    public DashBoardPage()
    {
        InitializeComponent();
    }

    private async void Leaderboard_Clicked(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new LeaderboardPopup());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var currentUser = SupabaseService.Client.Auth.CurrentUser;
        var currentUserId = currentUser?.Id;

        if (string.IsNullOrEmpty(currentUserId))
        {
            ClearSelectedRoom();
            RoomsList.Children.Clear();
            RoomKeyEntry.Text = string.Empty;
            _initializedUserId = null;
            return;
        }

        if (_initializedUserId == currentUserId)
            return;

        _initializedUserId = currentUserId;

        ClearSelectedRoom();
        RoomsList.Children.Clear();
        RoomKeyEntry.Text = string.Empty;

        await LoadMyRooms();

        if (_selectedRoom == null)
            return;

        var myRooms = await _roomsService.GetMyRoomMembers();
        bool stillHasAccess = myRooms.Any(x => x.RoomId == _selectedRoom.Id);

        if (!stillHasAccess)
        {
            ClearSelectedRoom();
            return;
        }

        SelectedRoomKeyLabel.Text = _selectedRoom.RoomKey;
        await LoadMembers(_selectedRoom);
        await LoadTasks(_selectedRoom.Id);
    }

    private void ClearSelectedRoom()
    {
        _selectedRoom = null;
        _selectedRoomButton = null;
        SelectedRoomKeyLabel.Text = "-";
        RoomActionButton.Text = "Leave room";

        MemberList.Children.Clear();
        ToDoTaskList.Children.Clear();
        InProgressTaskList.Children.Clear();
        InReviewTaskList.Children.Clear();
        DoneTaskList.Children.Clear();
    }

    private async void Account_Clicked(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new ProfilePopup());
    }

    private async void CreateTask_Clicked(object sender, EventArgs e)
    {
        if (_selectedRoom == null)
        {
            await DisplayAlert("Error", "Select a room first", "OK");
            return;
        }

        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
        {
            await DisplayAlert("Error", "User is not authorized", "OK");
            return;
        }

        var currentUserId = Guid.Parse(user.Id);

        if (_selectedRoom.OwnerId != currentUserId)
        {
            await DisplayAlert("Access denied", "Only room owner can create tasks", "OK");
            return;
        }

        await this.ShowPopupAsync(new AddTaskPopup(AddTaskToList));
    }

    private async void AddTaskToList(TaskItem taskItem)
    {
        if (_selectedRoom == null)
        {
            await DisplayAlert("Error", "No room selected", "OK");
            return;
        }

        try
        {
            taskItem.RoomId = _selectedRoom.Id;

            await _tasksService.CreateTask(taskItem);

            await LoadTasks(_selectedRoom.Id);

            await DisplayAlert("Success", "Task created", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task LoadTasks(long roomId)
    {
        ToDoTaskList.Children.Clear();
        InProgressTaskList.Children.Clear();
        InReviewTaskList.Children.Clear();
        DoneTaskList.Children.Clear();

        var tasks = await _tasksService.GetTasksByRoomId(roomId);

        foreach (var task in tasks)
        {
            Color backgroundColor;
            Color textColor;
            Color borderColor;

            if (task.Status == "in_progress")
            {
                backgroundColor = Color.FromArgb("#FEF3C7");
                textColor = Color.FromArgb("#92400E");
                borderColor = Color.FromArgb("#FCD34D");
            }
            else if (task.Status == "in_review")
            {
                backgroundColor = Color.FromArgb("#E0E7FF");
                textColor = Color.FromArgb("#3730A3");
                borderColor = Color.FromArgb("#A5B4FC");
            }
            else if (task.Status == "done")
            {
                backgroundColor = Color.FromArgb("#DCFCE7");
                textColor = Color.FromArgb("#166534");
                borderColor = Color.FromArgb("#86EFAC");
            }
            else
            {
                backgroundColor = Colors.White;
                textColor = Color.FromArgb("#111827");
                borderColor = Color.FromArgb("#E5E7EB");
            }

            var taskButton = new Button
            {
                Text = task.Name,
                HorizontalOptions = LayoutOptions.Fill,
                FontAttributes = FontAttributes.Bold,
                FontSize = 17,
                HeightRequest = 50,
                CornerRadius = 14,
                BackgroundColor = backgroundColor,
                TextColor = textColor,
                BorderColor = borderColor,
                BorderWidth = 1,
                Padding = new Thickness(12, 8)
            };

            taskButton.Clicked += async (s, e) =>
            {
                if (_selectedRoom == null)
                    return;

                await this.ShowPopupAsync(new TaskDetailsPopup(task, _selectedRoom.OwnerId, async () =>
                {
                    await LoadTasks(roomId);
                }));
            };

            if (task.Status == "in_progress")
            {
                InProgressTaskList.Children.Add(taskButton);
            }
            else if (task.Status == "in_review")
            {
                InReviewTaskList.Children.Add(taskButton);
            }
            else if (task.Status == "done")
            {
                DoneTaskList.Children.Add(taskButton);
            }
            else
            {
                ToDoTaskList.Children.Add(taskButton);
            }
        }
    }

    private async void CreateRoom_Clicked(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new CreateRoomPopup(CreateRoomWithName));
    }

    private async void CreateRoomWithName(string roomName)
    {
        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
        {
            await DisplayAlert("Error", "User is not authorized", "OK");
            return;
        }

        string key = RoomKeyGenerator.Generate();

        try
        {
            var insertedRoom = await _roomsService.CreateRoom(roomName, key);

            if (insertedRoom == null)
            {
                await DisplayAlert("Error", "Room was not created", "OK");
                return;
            }

            _selectedRoom = insertedRoom;
            SelectedRoomKeyLabel.Text = insertedRoom.RoomKey;

            await LoadMembers(insertedRoom);
            await LoadTasks(insertedRoom.Id);
            await LoadMyRooms();

            await DisplayAlert("Success", $"Room created. Key: {insertedRoom.RoomKey}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void JoinRoom_Clicked(object sender, EventArgs e)
    {
        string key = RoomKeyEntry.Text;

        if (string.IsNullOrWhiteSpace(key))
        {
            await DisplayAlert("Error", "Enter room key", "OK");
            return;
        }

        try
        {
            await _roomsService.JoinRoomByKey(key);
            RoomKeyEntry.Text = string.Empty;

            await LoadMyRooms();

            await DisplayAlert("Success", "You joined the room", "OK");
        }
        catch (Exception ex)
        {
            string message = ex.Message;

            if (message.Contains("Room not found", StringComparison.OrdinalIgnoreCase))
                message = "Room not found";

            if (message.Contains("already in this room", StringComparison.OrdinalIgnoreCase))
                message = "You are already in this room";

            await DisplayAlert("Error", message, "OK");
        }
    }

    private async Task LoadMyRooms()
    {
        RoomsList.Children.Clear();

        var myRoomMembers = await _roomsService.GetMyRoomMembers();

        foreach (var member in myRoomMembers)
        {
            var room = await _roomsService.GetRoomById(member.RoomId);

            if (room == null)
                continue;

            var roomButton = new Button
            {
                Text = string.IsNullOrWhiteSpace(room.RoomName) ? room.RoomKey : room.RoomName,
                BackgroundColor = Colors.White,
                TextColor = Color.FromArgb("#111827"),
                HeightRequest = 52,
                CornerRadius = 14,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                BorderColor = Color.FromArgb("#E5E7EB"),
                BorderWidth = 1,
                HorizontalOptions = LayoutOptions.Fill
            };

            if (_selectedRoom != null && _selectedRoom.Id == room.Id)
            {
                roomButton.BackgroundColor = Color.FromArgb("#DBEAFE");
                roomButton.TextColor = Color.FromArgb("#1D4ED8");
                roomButton.BorderColor = Color.FromArgb("#93C5FD");
                _selectedRoomButton = roomButton;
            }

            roomButton.Clicked += async (s, e) =>
            {
                if (_isRoomSwitching)
                    return;

                if (_selectedRoom?.Id == room.Id)
                    return;

                try
                {
                    _isRoomSwitching = true;

                    roomButton.IsEnabled = false;

                    var previousButton = _selectedRoomButton;
                    var previousRoom = _selectedRoom;

                    _selectedRoom = room;
                    SelectedRoomKeyLabel.Text = room.RoomKey;

                    if (previousButton != null && previousButton != roomButton)
                    {
                        previousButton.BackgroundColor = Colors.White;
                        previousButton.TextColor = Color.FromArgb("#111827");
                        previousButton.BorderColor = Color.FromArgb("#E5E7EB");
                    }

                    roomButton.BackgroundColor = Color.FromArgb("#DBEAFE");
                    roomButton.TextColor = Color.FromArgb("#1D4ED8");
                    roomButton.BorderColor = Color.FromArgb("#93C5FD");
                    _selectedRoomButton = roomButton;

                    var user = SupabaseService.Client.Auth.CurrentUser;

                    if (user != null && room.OwnerId == Guid.Parse(user.Id))
                        RoomActionButton.Text = "Delete room";
                    else
                        RoomActionButton.Text = "Leave room";

                    await LoadMembers(room);
                    await LoadTasks(room.Id);
                }
                finally
                {
                    roomButton.IsEnabled = true;
                    _isRoomSwitching = false;
                }
            };

            RoomsList.Children.Add(roomButton);
        }
    }

    private async void RoomActionButton_Clicked(object sender, EventArgs e)
    {
        if (_selectedRoom == null)
        {
            await DisplayAlert("Error", "Select a room first", "OK");
            return;
        }

        var user = SupabaseService.Client.Auth.CurrentUser;

        if (user == null)
        {
            await DisplayAlert("Error", "User is not authorized", "OK");
            return;
        }

        var currentUserId = Guid.Parse(user.Id);

        if (_selectedRoom.OwnerId == currentUserId)
        {
            bool confirm = await DisplayAlert(
                "Delete room",
                "Are you sure you want to delete this room?",
                "Yes",
                "No");

            if (!confirm)
                return;

            await DeleteRoom(_selectedRoom);
        }
        else
        {
            bool confirm = await DisplayAlert(
                "Leave room",
                "Are you sure you want to leave this room?",
                "Yes",
                "No");

            if (!confirm)
                return;

            await LeaveRoom(_selectedRoom.Id, currentUserId);
        }
    }

    private async Task LeaveRoom(long roomId, Guid userId)
    {
        await _roomsService.LeaveRoom(roomId, userId);

        _selectedRoom = null;
        _selectedRoomButton = null;
        SelectedRoomKeyLabel.Text = "-";
        RoomActionButton.Text = "Leave room";

        MemberList.Children.Clear();
        ToDoTaskList.Children.Clear();
        InProgressTaskList.Children.Clear();
        InReviewTaskList.Children.Clear();
        DoneTaskList.Children.Clear();

        await LoadMyRooms();

        await DisplayAlert("Success", "You left the room", "OK");
    }

    private async Task DeleteRoom(Room room)
    {
        await _roomsService.DeleteRoom(room);

        _selectedRoom = null;
        _selectedRoomButton = null;
        SelectedRoomKeyLabel.Text = "-";
        RoomActionButton.Text = "Leave room";

        MemberList.Children.Clear();
        ToDoTaskList.Children.Clear();
        InProgressTaskList.Children.Clear();
        InReviewTaskList.Children.Clear();
        DoneTaskList.Children.Clear();

        await LoadMyRooms();

        await DisplayAlert("Success", "Room deleted", "OK");
    }

    private async void CopyRoomKey_Clicked(object sender, EventArgs e)
    {
        if (_selectedRoom == null)
        {
            await DisplayAlert("Error", "Select a room first", "OK");
            return;
        }

        await Clipboard.Default.SetTextAsync(_selectedRoom.RoomKey);

        await DisplayAlert("Copied", "Room key copied to clipboard", "OK");
    }

    private async Task LoadMembers(Room room)
    {
        MemberList.Children.Clear();

        var members = await _roomsService.GetMembersByRoomId(room.Id);

        var currentUser = SupabaseService.Client.Auth.CurrentUser;
        Guid? currentUserId = currentUser != null ? Guid.Parse(currentUser.Id) : null;

        foreach (var member in members)
        {
            string displayName = !string.IsNullOrWhiteSpace(member.Name)
                ? member.Name
                : member.Email ?? "Unknown user";

            bool isOwnerMember = member.UserId == room.OwnerId;
            bool isCurrentUserOwner = currentUserId != null && currentUserId == room.OwnerId;
            bool isNotSelf = currentUserId == null || member.UserId != currentUserId.Value;

            var row = new Grid
            {
                ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
                ColumnSpacing = 10,
                Padding = new Thickness(12),
                BackgroundColor = isOwnerMember
                    ? Color.FromArgb("#FEF2F2")
                    : Colors.White
            };

            var border = new Border
            {
                Stroke = isOwnerMember
                    ? Color.FromArgb("#FCA5A5")
                    : Color.FromArgb("#E5E7EB"),
                StrokeThickness = 1,
                Padding = 0,
                Margin = new Thickness(0, 0, 0, 8),
                Content = row
            };

            border.StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(14)
            };

            var labelButton = new Button
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = Colors.Transparent,
                BorderWidth = 0,
                Padding = 0,
                Margin = 0,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                Text = isOwnerMember ? $"OWNER: {displayName}" : displayName,
                TextColor = isOwnerMember
                    ? Color.FromArgb("#DC2626")
                    : Color.FromArgb("#111827")
            };

            labelButton.Clicked += async (s, e) =>
            {
                await this.ShowPopupAsync(new UserInfoPopup(member));
            };

            row.Add(labelButton);
            Grid.SetColumn(labelButton, 0);

            if (isCurrentUserOwner && isNotSelf)
            {
                var deleteButton = new Button
                {
                    Text = "Delete",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    HeightRequest = 40,
                    CornerRadius = 12,
                    Padding = new Thickness(14, 6),
                    BackgroundColor = Color.FromArgb("#FEE2E2"),
                    TextColor = Color.FromArgb("#B91C1C")
                };

                deleteButton.Clicked += async (s, e) =>
                {
                    bool confirm = await DisplayAlert(
                        "Delete member",
                        $"Remove {displayName} from room?",
                        "Yes",
                        "No");

                    if (!confirm)
                        return;

                    try
                    {
                        await _roomsService.RemoveMember(room.Id, member.UserId);
                        await LoadMembers(room);
                        await LoadMyRooms();
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", ex.Message, "OK");
                    }
                };

                row.Add(deleteButton);
                Grid.SetColumn(deleteButton, 1);
            }

            MemberList.Children.Add(border);
        }
    }

    private async void Achievements_Clicked(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new AchievementsPopup());
    }

}