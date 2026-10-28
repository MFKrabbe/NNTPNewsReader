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
    public async Task ConnectAsync_WhenServerAcceptsConnection_ReturnsTrue()
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
        bool accepted = await _client.ConnectAsync(host, port);
        
        //Assert
        Assert.That(accepted, Is.True);
        Assert.DoesNotThrow(() => _client.Disconnect());
    }
    
    
    [Test]
    public async Task ListGroupsAsync_WhenConnected_ReturnsGroupNames()
    {
        //Arrange:
        var host = "news.eternal-september.org";
        var port = 119;
        bool accepted = await _client.ConnectAsync(host, port);
        
        //Act
        var groupNames = await _client.ListGroupsAsync();
        
        //Assert
        Assert.That(accepted, Is.True);
        Assert.That(groupNames, Is.Not.Empty);
        
    }
    
}