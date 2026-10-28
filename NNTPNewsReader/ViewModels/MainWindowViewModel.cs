using CommunityToolkit.Mvvm.ComponentModel;
using NNTPNewsReader.Services;

namespace NNTPNewsReader.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public NavigationService Navigation { get; }

    public MainWindowViewModel(NavigationService navigation)
    {
        Navigation = navigation;
    }  
}