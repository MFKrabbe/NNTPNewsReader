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

    private ObservableCollection<string> _test;

    public ObservableCollection<string> Test
    {
        get => _test;
        set => SetProperty(ref _test, value);
    }
    
    
    public List<string> _groups;

    [RelayCommand]
    public async Task testConnection()
    {
        _groups =  await _nntpClient.ListGroupsAsync();
        
        Test = new ObservableCollection<string>(_groups);
        


        //_nntpClient.Connect(null, 0);
    }
}