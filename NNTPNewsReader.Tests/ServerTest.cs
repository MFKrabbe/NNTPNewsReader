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
        bool accepted = await _client.ConnectAsync(host, port);
        
        //Assert
        Assert.That(accepted, Is.True);
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
        bool accepted = await _client.ConnectAsync(host, port) ;
        
        //Assert
        Assert.That(accepted, Is.True);
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
            
        bool accepted = await _client.ConnectAsync(host, port);
        Assert.That(accepted, Is.True);
        
        
        //Act
        var result = await _client.AuthenticateAsync(username!, password!);
       
        //Assert
        Assert.That(result.hasSucceded, Is.True);
        Assert.That(result.responseCode, Is.EqualTo("281"));
       
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
            
        bool accepted = await _client.ConnectAsync(host, port);
        Assert.That(accepted, Is.True);
        
        
        //Act
        var result = await _client.AuthenticateAsync(username!, password!);
        
        //Assert
        Assert.That(result.hasSucceded, Is.False);
        Assert.That(result.responseCode, Is.EqualTo("481"));
    }
    
    [Test]
    public async Task AuthenticateAsync_WithInvalidUser_ReturnsFalse()
    {
        //Im n
        
        //Arrange:
        var host = "news.eternal-september.org";
        var port = 119;

        string username = "Bobby";
        string password = "tester man";
            
        bool accepted = await _client.ConnectAsync(host, port);
        Assert.That(accepted, Is.True);
        
        
        //Act
        var result = await _client.AuthenticateAsync(username!, password!);
        
        //Assert
        Assert.That(result.hasSucceded, Is.False);
        Assert.That(result.responseCode, Is.EqualTo("481"));
    }
    
}