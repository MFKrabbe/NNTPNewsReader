using CommunityToolkit.Mvvm.Input;
using NNTPNewsReader.Services;

namespace NNTPNewsReader.ViewModels;

public partial class NntpMainViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;
    private readonly INntpClient _nntpClient;

    public NntpMainViewModel(NavigationService navigation, INntpClient nntpClient)
    {
        _navigation = navigation;
        _nntpClient = nntpClient;
    }

    [RelayCommand]
    public void testConnection()
    {
        //_nntpClient.Connect(null, 0);
    }
}