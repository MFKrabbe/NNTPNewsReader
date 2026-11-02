using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using NNTPNewsReader.ViewModels;

namespace NNTPNewsReader.Services;

public class NavigationService : ObservableObject
{
    private readonly IServiceProvider _services;

    private ViewModelBase? _currentViewModel;

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public void NavigateTo<T>()  //Er sat op til at bruge vores ServiceCollectionExtension klasse i stedet
        where T : ViewModelBase
    {
        CurrentViewModel = _services.GetRequiredService<T>();
    }
  
}