
using CommunityToolkit.Mvvm.Input;
using NNTPNewsReader.Services;

namespace NNTPNewsReader.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly NavigationService _navigation;
    private readonly ILoginService _dbFacade; // skal nok ikke bruge dette og skal gå over NntpServerClienten
    private readonly INntpClient _nntpClient;

    private string _username = "MFKrabbe";
    private string _password = "wmggezwgn";

    public LoginViewModel(NavigationService navigation, ILoginService dbFacade, INntpClient nntpClient)
    {
        _navigation = navigation;
        _dbFacade = dbFacade;
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
    
    
    [RelayCommand]
    public void CheckLogin()
    {
        if (_dbFacade.Login(Username, Password))
        {
            _navigation.NavigateTo<NntpMainViewModel>();
            Username = "";
            Password = "";
            LoginMessage = "Logged in";
        }
        else
        {
            LoginMessage = "Wrong UserName or Password";
        }
    }
}