using NNTPNewsReader.Services;

namespace NNTPNewsReader.Tests;

public class Tests
{
    private NntpClient _client = null!;
    
    [SetUp]
    public void Setup()
    {
        _client = new NntpClient();
    }
    
    [TearDown]
    public void TearDown()
    {
        _client.Disconnect();
    }
    
    
    [Test]
    public async Task Connect_WhenServerAcceptsConnection_ReturnsTrue()
    {
        //Arrange:
        var host = "news.eternal-september.org";
        var port = 119;
        
        //Act:
        bool isAccepted = await _client.ConnectAsync(host, port);
        
        //Assert
        Assert.That(isAccepted, Is.True);
    }
    
    [Test]
    public void Disconnect_WhenNotConnected_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _client.Disconnect());
    }
    
    [Test]
    public async Task Disconnect_WhenConnected_DoesNotThrow()
    {
        //Arrange:
        var host = "news.eternal-september.org";
        var port = 119;
        
        //Act
        bool isAccepted = await _client.ConnectAsync(host, port) ;
        
        //Assert
        Assert.That(isAccepted, Is.True);
        Assert.DoesNotThrow(() => _client.Disconnect());
    }
    
    
    [Test]
    public async Task AuthenticateAsync_WithValidLogin_ReturnsTrue()
    {
        //Arrange:
        var host = "news.eternal-september.org";
        var port = 119;
        
        string? username = Environment.GetEnvironmentVariable("USERNAME");
        string? password = Environment.GetEnvironmentVariable("PASSWORD");
            
        bool isAccepted = await _client.ConnectAsync(host, port);
        Assert.That(isAccepted, Is.True);
        
        
        //Act
        var login = await _client.AuthenticateAsync(username!, password!);
       
        //Assert
        Assert.That(login.hasSucceded, Is.True);
        Assert.That(login.responseCode, Is.EqualTo("281"));
       
    }
    
    [Test]
    public async Task AuthenticateAsync_WithWrongPassword_ReturnsFalse()
    {
        //Im n
        
        //Arrange:
        var host = "news.eternal-september.org";
        var port = 119;

        string? username = Environment.GetEnvironmentVariable("USERNAME");
        string password = "tester man";
            
        bool isAccepted = await _client.ConnectAsync(host, port);
        Assert.That(isAccepted, Is.True);
        
        
        //Act
        var login = await _client.AuthenticateAsync(username!, password!);
        
        //Assert
        Assert.That(login.hasSucceded, Is.False);
        Assert.That(login.responseCode, Is.EqualTo("481"));
    }
    
    [Test]
    public async Task AuthenticateAsync_WithInvalidUser_ReturnsFalse()
    {
        //Arrange:
        var host = "news.eternal-september.org";
        var port = 119;

        string username = "Bobby";
        string password = "tester man";
            
        bool isAccepted = await _client.ConnectAsync(host, port);
        Assert.That(isAccepted, Is.True);
        
        
        //Act
        var login = await _client.AuthenticateAsync(username!, password!);
        
        //Assert
        Assert.That(login.hasSucceded, Is.False);
        Assert.That(login.responseCode, Is.EqualTo("481"));
    }

    [Test]
    public async Task ListGroupsAsync_WhenAuthenticated_ReturnsList()
    {
        // Arrange
        string? username = Environment.GetEnvironmentVariable("USERNAME");
        string? password = Environment.GetEnvironmentVariable("PASSWORD");

        Assert.That(username, Is.Not.Null.And.Not.Empty);
        Assert.That(password, Is.Not.Null.And.Not.Empty);

        var host = "news.eternal-september.org";
        var port = 119;

        bool isConnected = await _client.ConnectAsync(host, port);
        Assert.That(isConnected, Is.True);

        var login = await _client.AuthenticateAsync(username!, password!);
        Assert.That(login.hasSucceded, Is.True);

        // Act
        var group = await _client.ListGroupsAsync();

        // Assert
        Assert.That(group.Contains("free.chicken"),Is.True);
    }

}