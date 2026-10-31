using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NNTPNewsReader.Models;
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
    
    
    private string _errorMessage = "";

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }
    
    private NewsGroupEntry? _selectedNewsGroup;

    public NewsGroupEntry? SelectedNewsGroup
    {
        get => _selectedNewsGroup;
        set
        {
            bool hasChanged = SetProperty(ref _selectedNewsGroup, value);
            
            if (hasChanged && value != null && LoadArticlesCommand.CanExecute(null))
            {
                LoadArticlesCommand.Execute(null);
            }
        }
    }


    private ObservableCollection<NewsGroupEntry> _newsGroups = new();

    public ObservableCollection<NewsGroupEntry> NewsGroups
    {
        get => _newsGroups;
        set => SetProperty(ref _newsGroups, value);
    }
    
    
    private ObservableCollection<ArticleEntry> _articles = new();

    public ObservableCollection<ArticleEntry> Articles
    {
        get => _articles;
        set => SetProperty(ref _articles, value);
    }
    
    
    
    public async Task LoadGroupsAsync()
    {
        try
        {
            NewsGroups.Clear();
            ErrorMessage = "";
        
            var listNewsGroupResult = await _nntpClient.ListNewsGroupsAsync();

            if (!listNewsGroupResult.hasSucceeded)
            {
                ErrorMessage = "Failed to load groups";
                return;
            }
            //Need ta add Articles number in view
            NewsGroups = new ObservableCollection<NewsGroupEntry>(listNewsGroupResult.newsGroupEntries);
        }
        catch(Exception e)
        {
            Disconnect();
            Console.WriteLine(e);
        }
    }
    
    
    [RelayCommand]
    public async Task LoadArticlesAsync()
    {
        try
        {
            Articles.Clear();
            ErrorMessage = "";
        
            if (SelectedNewsGroup != null)
            {
                var selectNewsGroupResult = await _nntpClient.SelectNewsGroupAsync(SelectedNewsGroup.GroupName);

                if (!selectNewsGroupResult.hasSucceeded || selectNewsGroupResult.details == null)
                {
                    ErrorMessage = "Failed to selection of group from server";
                    return;
                }

                var listArticlesResult = await _nntpClient.ListArticlesAsync(selectNewsGroupResult.details);
            
                if (!listArticlesResult.hasSucceeded || selectNewsGroupResult.details == null)
                {
                    ErrorMessage = "Failed to get articles from server";
                    return;
                }
                //Need ta add Articles number in view
                Articles = new ObservableCollection<ArticleEntry>(listArticlesResult.articleEntries);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Disconnect();
        }
    }
    
    
    

    [RelayCommand]
    public void Disconnect()
    {
        _nntpClient.Disconnect();
        NewsGroups.Clear();
        _navigation.NavigateTo<LoginViewModel>();
    }
}