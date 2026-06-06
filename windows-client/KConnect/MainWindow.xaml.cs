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
    }
}
