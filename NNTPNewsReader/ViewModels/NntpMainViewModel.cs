using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

    private ObservableCollection<string> _newsGroups = new();

    public ObservableCollection<string> NewsGroups
    {
        get => _newsGroups;
        set => SetProperty(ref _newsGroups, value);
    }
    
    public async Task LoadGroupsAsync()
    {
        var groupsNames = await _nntpClient.ListGroupsAsync();
        
        NewsGroups = new ObservableCollection<string>(groupsNames);
        
    }

    [RelayCommand]
    public void Disconnect()
    {
        _nntpClient.Disconnect();
        NewsGroups.Clear();
        _navigation.NavigateTo<LoginViewModel>();
    }
}