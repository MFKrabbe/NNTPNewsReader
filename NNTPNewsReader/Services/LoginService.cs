namespace NNTPNewsReader.Services;

public class LoginService : ILoginService
{
    public bool Login(string username, string password)
    {
        
        // skal tjekke om der kommer 200 fra putty serveren og ikke MFKrabbe
        return username == "MFKrabbe" || password == "wmggezwgn";
    }
}