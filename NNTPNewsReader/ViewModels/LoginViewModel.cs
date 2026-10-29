
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
    private int _port = 119;
    private string _server = "news.eternal-september.org";
    

    public LoginViewModel(NavigationService navigation, INntpClient nntpClient)
    {
        _navigation = navigation;
        _nntpClient = nntpClient;
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
    
    //https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-10.0
    //https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socketexception?view=net-10.0
    [RelayCommand]
    public async Task CheckLoginAsync()
    {
        try
        {
            bool hasConnected = await _nntpClient.ConnectAsync(_server, _port);
            if (!hasConnected)
            {
                LoginMessage = "Could not connect to server";
            }


            var result = await _nntpClient.AuthenticateAsync(Username, Password);

            if (result.hasSucceded)
            {
                _navigation.NavigateTo<NntpMainViewModel>();
                Username = "";
                Password = "";
                LoginMessage = "";
            }
            else
            {
                _nntpClient.Disconnect();

                switch (result.responseCode)
                {
                    case "481":
                        LoginMessage = "Wrong Username or Password";
                        break;
                    default:
                        LoginMessage = "Login failed, Please try again";
                        break;
                }

            }
        }
        catch (SocketException)
        {
            //Exception for network error
            //fx no server on port 455 or a server with that hostname, or when connection is aborted
            LoginMessage = "Could not connect to server";
        }
        catch (IOException)
        {
            //Exception for NetworkStream read/write error
            LoginMessage = "Could not connect to server";
        }

        
    }
}