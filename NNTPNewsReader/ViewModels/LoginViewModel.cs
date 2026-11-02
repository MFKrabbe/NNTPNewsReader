
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NNTPNewsReader.Services;

namespace NNTPNewsReader.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;
    private readonly INntpClient _nntpClient;

    //husk at ændre password
    
    private string _username = "";
    private string _password = "";
    private int? _port; //80,119
    private string _server = ""; //"reader80.eternal-september.org"; //news.eternal-september.org
    

    public LoginViewModel(NavigationService navigation, INntpClient nntpClient)
    {
        _navigation = navigation;
        _nntpClient = nntpClient;
    }
    
    public string Server
    {
        get => _server;
        set => SetProperty(ref _server, value);
    }
    
    public int? Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }
    
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _loginMessage = "";

    public string LoginMessage
    {
        get => _loginMessage;
        set => SetProperty(ref _loginMessage, value);
    }
    
    private bool _isConnected = false;

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }
    
    
    
    
    //https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-10.0
    //https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socketexception?view=net-10.0
    [RelayCommand]
    public async Task CheckLoginAsync()
    {
        LoginMessage = "";

        if (!IsConnected)
        {
            return;
        }
        
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            LoginMessage = "Enter your username and password.";
            return;
        }

        try
        {
            var hasSucceeded = await _nntpClient.AuthenticateAsync(Username, Password);

            if (hasSucceeded)
            {
                _navigation.NavigateTo<NntpMainViewModel>();
                Username = "";
                Password = "";
                LoginMessage = "";
                
            }
            else
            {
                LoginMessage = "Wrong Username or Password, try again";
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            LoginMessage = "Something went wrong, try again";
            _nntpClient.Disconnect();
            IsConnected =  false;
        }
    }
    
    
    [RelayCommand]
    public async Task CheckServerCredentialsAsync()
    {
        
        if (string.IsNullOrWhiteSpace(Server))
        {
            LoginMessage = "Enter a server name";
            return;
        }
        
        if (!Port.HasValue)
        {
            LoginMessage = "Enter a port";
            return;
        }

        if (Port < 1 || Port > 65535)
        {
            LoginMessage = "Enter a port between 1 and 65535";
            return;
        }
        LoginMessage = "";
        
        try
        {
            bool hasConnected = await _nntpClient.ConnectAsync(Server.Trim(), Port.Value);
            if (!hasConnected)
            {
                IsConnected = false;
                LoginMessage = "Could not connect to server";
                return;
            }

            IsConnected = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _nntpClient.Disconnect();
            IsConnected = false;
            LoginMessage = "Something went wrong, try again";
        }
        
    }
    
}