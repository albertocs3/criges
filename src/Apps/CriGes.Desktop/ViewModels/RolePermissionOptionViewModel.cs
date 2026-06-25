namespace CriGes.Desktop.ViewModels;

public sealed class RolePermissionOptionViewModel : ObservableObject
{
    private bool _isGranted;

    public RolePermissionOptionViewModel(string name, string description, bool isGranted)
    {
        Name = name;
        Description = description;
        _isGranted = isGranted;
    }

    public string Name { get; }

    public string Description { get; }

    public bool IsGranted
    {
        get => _isGranted;
        set => SetProperty(ref _isGranted, value);
    }
}
