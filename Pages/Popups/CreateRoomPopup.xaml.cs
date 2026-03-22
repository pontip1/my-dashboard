using CommunityToolkit.Maui.Views;

namespace MyDashboard.Pages;

public partial class CreateRoomPopup : Popup
{
    private readonly Action<string> _onRoomCreated;

    public CreateRoomPopup(Action<string> onRoomCreated)
    {
        InitializeComponent();
        _onRoomCreated = onRoomCreated;
    }

    private void Create_Clicked(object sender, EventArgs e)
    {
        string roomName = RoomNameEntry.Text;

        if (string.IsNullOrWhiteSpace(roomName))
            return;

        _onRoomCreated?.Invoke(roomName);

        CloseAsync();
    }

    private void Close_Clicked(object sender, EventArgs e)
    {
        CloseAsync();
    }
}