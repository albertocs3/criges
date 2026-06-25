using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CriGes.Desktop.ViewModels;

namespace CriGes.Desktop;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += ViewModelPropertyChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel.CheckApiCommand.CanExecute(null))
        {
            _viewModel.CheckApiCommand.Execute(null);
        }
    }

    private void AdminPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.AdminPassword = passwordBox.Password;
        }
    }

    private void AdminPasswordConfirmationChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.AdminPasswordConfirmation = passwordBox.Password;
        }
    }

    private void LoginPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.LoginPassword = passwordBox.Password;
        }
    }

    private void NewUserPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.NewUserPassword = passwordBox.Password;
        }
    }

    private void NewUserPasswordConfirmationChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.NewUserPasswordConfirmation = passwordBox.Password;
        }
    }

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainWindowViewModel.AdminPassword):
                ClearPasswordBoxIfViewModelCleared(AdminPasswordBox, _viewModel.AdminPassword);
                break;
            case nameof(MainWindowViewModel.AdminPasswordConfirmation):
                ClearPasswordBoxIfViewModelCleared(AdminPasswordConfirmationBox, _viewModel.AdminPasswordConfirmation);
                break;
            case nameof(MainWindowViewModel.LoginPassword):
                ClearPasswordBoxIfViewModelCleared(LoginPasswordBox, _viewModel.LoginPassword);
                break;
            case nameof(MainWindowViewModel.NewUserPassword):
                ClearPasswordBoxIfViewModelCleared(NewUserPasswordBox, _viewModel.NewUserPassword);
                break;
            case nameof(MainWindowViewModel.NewUserPasswordConfirmation):
                ClearPasswordBoxIfViewModelCleared(NewUserPasswordConfirmationBox, _viewModel.NewUserPasswordConfirmation);
                break;
        }
    }

    private static void ClearPasswordBoxIfViewModelCleared(PasswordBox passwordBox, string viewModelValue)
    {
        if (viewModelValue.Length == 0 && passwordBox.Password.Length > 0)
        {
            passwordBox.Clear();
        }
    }
}
