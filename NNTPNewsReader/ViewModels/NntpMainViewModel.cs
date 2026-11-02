using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Net.Mail;
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
    
    
    private bool _isPosting;

    public bool IsPosting
    {
        get => _isPosting;
        set => SetProperty(ref _isPosting, value);
    }
    
    private string _postAuthor = "";

    public string PostAuthor
    {
        get => _postAuthor;
        set => SetProperty(ref _postAuthor, value);
    }
    
    private string _postEmail = "";

    public string PostEmail
    {
        get => _postEmail;
        set => SetProperty(ref _postEmail, value);
    }
    
    
    private string _postSubject = "";

    public string PostSubject
    {
        get => _postSubject;
        set => SetProperty(ref _postSubject, value);
    }
    
    
    private string _postBody = "";

    public string PostBody
    {
        get => _postBody;
        set => SetProperty(ref _postBody, value);
    }
    
    private string _postGroupName = "";

    public string PostGroupName
    {
        get => _postGroupName;
        set => SetProperty(ref _postGroupName, value);
    }
    
    public bool CanPostToSelectedGroup
    {
        get
        {
            if (SelectedNewsGroupEntry == null)
            {
                return false;
            }

            return SelectedNewsGroupEntry.Status == "y" || SelectedNewsGroupEntry.Status == "m";
        }
    }
    
    
    private string _articleBody = "";

    public string ArticleBody
    {
        get => _articleBody;
        set => SetProperty(ref _articleBody, value);
    }
    
    private ArticleEntry? _selectedArticleEntry;

    public ArticleEntry? SelectedArticleEntry
    {
        get => _selectedArticleEntry;
        set
        {
            bool hasChanged = SetProperty(ref _selectedArticleEntry, value);
            
            if (hasChanged && value != null && LoadArticleBodyCommand.CanExecute(null))
            {
                LoadArticleBodyCommand.Execute(null);
            }
        }
    }
    
    private List<ArticleEntry> _allArticleEntries = new();
    private ObservableCollection<ArticleEntry> _articles = new();

    public ObservableCollection<ArticleEntry> Articles
    {
        get => _articles;
        set => SetProperty(ref _articles, value);
    }
    
    
    private NewsGroupEntry? _selectedNewsGroup;
    
    public NewsGroupEntry? SelectedNewsGroupEntry
    {
        get => _selectedNewsGroup;
        set
        {
            bool hasChanged = SetProperty(ref _selectedNewsGroup, value);
            
            if (hasChanged)
            {
                IsPosting = false;
                OnPropertyChanged(nameof(CanPostToSelectedGroup));
            }
            
            if (hasChanged && value != null && LoadArticlesCommand.CanExecute(null))
            {
                SelectedArticleEntry = null;
                ArticleBody = "";
                
                LoadArticlesCommand.Execute(null);
            }
        }
    }
    

    private List<NewsGroupEntry> _allNewsGroups = new();
    private ObservableCollection<NewsGroupEntry> _newsGroups = new();

    public ObservableCollection<NewsGroupEntry> NewsGroups
    {
        get => _newsGroups;
        set => SetProperty(ref _newsGroups, value);
    }
    
    private string _newsGroupSearchText = "";

    public string NewsGroupSearchText
    {
        get => _newsGroupSearchText;
        set
        {
            bool changed = SetProperty(ref _newsGroupSearchText, value);

            if (changed)
            {
                FilterNewsGroups();
            }
        }
    }
    
    
    private string _articleSearchText = "";

    public string ArticleSearchText
    {
        get => _articleSearchText;
        set
        {
            bool changed = SetProperty(ref _articleSearchText, value);

            if (changed)
            {
                FilterArticles();
            }
        }
    }
    
    
    
    public async Task LoadGroupsAsync()
    {
        try
        {
            NewsGroups.Clear();
            _allNewsGroups.Clear();
            ErrorMessage = "";
        
            var listNewsGroupResult = await _nntpClient.ListNewsGroupsAsync();

            if (!listNewsGroupResult.hasSucceeded)
            {
                ErrorMessage = "Failed to load groups";
                return;
            }
            
            _allNewsGroups = listNewsGroupResult.newsGroupEntries;
            NewsGroups = new ObservableCollection<NewsGroupEntry>(_allNewsGroups);
        }
        catch(Exception e)
        {
            Disconnect();
            Console.WriteLine(e);
        }
    }
    
    private void FilterNewsGroups()
    {
        var matches = new List<NewsGroupEntry>();

        foreach (var group in _allNewsGroups)
        {
            if (group.GroupName.Contains(NewsGroupSearchText, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(group);
            }
        }

        NewsGroups = new ObservableCollection<NewsGroupEntry>(matches);
    }
    
    private void FilterArticles()
    {
        var matches = new List<ArticleEntry>();

        foreach (var articles in _allArticleEntries)
        {
            if (articles.ArticleSubject.Contains(ArticleSearchText, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(articles);
            }
        }

        Articles = new ObservableCollection<ArticleEntry>(matches);
    }
    
    
    
    
    [RelayCommand]
    public async Task LoadArticlesAsync()
    {
        _allArticleEntries.Clear();
        Articles.Clear();
        ErrorMessage = "";
        
        try
        {
            if (SelectedNewsGroupEntry != null)
            {
                var selectNewsGroupResult = await _nntpClient.SelectNewsGroupAsync(SelectedNewsGroupEntry.GroupName);

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
                
                _allArticleEntries = listArticlesResult.articleEntries;
                Articles = new ObservableCollection<ArticleEntry>(_allArticleEntries);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Disconnect();
        }
    }
    
    [RelayCommand]
    public async Task LoadArticleBodyAsync()
    {
        ArticleBody = "";
        ErrorMessage = "";
        
        try
        {
            if (SelectedArticleEntry != null)
            {
                var articleBodyResult = await _nntpClient.ReadArticleBodyAsync(SelectedArticleEntry.ArticleNumber);

                if (!articleBodyResult.hasSucceeded)
                {
                    ErrorMessage = "Failed to get article body from server";
                    return;
                }

                ArticleBody = articleBodyResult.articleText;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Disconnect();
        }
    }
    
    [RelayCommand]
    public void MakePost()
    {
        if (SelectedNewsGroupEntry == null)
        {
            ErrorMessage = "Please select newsgroup";
            return;
        }
       
        PostGroupName = SelectedNewsGroupEntry.GroupName;
        ErrorMessage = "";
        IsPosting = true;
    }
    
    [RelayCommand]
    public void CancelPost()
    {
        IsPosting = false;
    }
    
    [RelayCommand]
    public async Task SendPostAsync()
    {
        if (string.IsNullOrWhiteSpace(PostGroupName) || string.IsNullOrWhiteSpace(PostEmail) ||
            string.IsNullOrWhiteSpace(PostSubject) || string.IsNullOrWhiteSpace(PostBody))
        {
            ErrorMessage = "Please fill in Email, Subject, and Body.";
            return;
        }
        ErrorMessage = "";

        MailAddress? email;
        bool isValidEmailFormat = MailAddress.TryCreate(PostEmail, out email); // checks that the mail is formated correctly
        
        if (!isValidEmailFormat)
        {
            ErrorMessage = "Enter a valid email address.";
            return;
        }
        
        var post = new ArticlePost
        {
            PostGroupName = PostGroupName,
            PostAuthor = $"{PostAuthor} <{PostEmail}>",
            PostSubject = PostSubject,
            PostBody = PostBody
        };
        
        try
        {
            bool hasSucceeded = await _nntpClient.PostArticleAsync(post);

            if (!hasSucceeded)
            {
                ErrorMessage = "The server did not accept the post or posting is not allowed.";
                return;
            }

            ErrorMessage = "Post sent successfully.";
            PostSubject = "";
            PostBody = "";
            IsPosting = false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ErrorMessage = "Could not confirm the post was sent. It may have been received.";
        }
        
        
    }
    
    

    [RelayCommand]
    public void Disconnect()
    {
        _nntpClient.Disconnect();
        _allArticleEntries.Clear();
        Articles.Clear();
        NewsGroups.Clear();
        _allNewsGroups.Clear();
        _navigation.NavigateTo<LoginViewModel>();
        if (_navigation.CurrentViewModel is LoginViewModel login)
        {
            login.IsConnected = false;
            login.Password = "";
            login.LoginMessage = "";
        }
    }
}