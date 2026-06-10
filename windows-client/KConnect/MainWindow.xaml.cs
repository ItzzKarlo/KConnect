using System.Windows;
using System.Windows.Controls;
using KConnect.Views;

namespace KConnect;

public partial class MainWindow : Window
{
    private static MainWindow? _instance;
    public static MainWindow? Instance => _instance;

    public MainWindow()
    {
        InitializeComponent();
        _instance = this;
        NavigateTo(new LoginView());
    }

    public void NavigateTo(UserControl view)
    {
        MainContent.Content = view;

        // Expand/contract window based on which view we're showing
        if (view is ChatView)
        {
            ResizeMode = ResizeMode.CanResize;
            Width      = 1000;
            Height     = 660;
            MinWidth   = 760;
            MinHeight  = 480;
        }
        else
        {
            ResizeMode = ResizeMode.NoResize;
            Width      = 460;
            Height     = 600;
        }

        // Re-centre after resize
        Left = (SystemParameters.PrimaryScreenWidth  - Width)  / 2;
        Top  = (SystemParameters.PrimaryScreenHeight - Height) / 2;
    }
}
