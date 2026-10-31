using NNTPNewsReader.Services;

namespace NNTPNewsReader.Tests;

public class Tests
{
    private NntpClient _client = null!;
    private const string Host = "reader80.eternal-september.org"; //news.eternal-september.org
    private const int Port = 80; //119


    private async Task ConnectToServerWithCredentials()
    {
        await _client.ConnectAsync(Host, Port);
        
        string? username = Environment.GetEnvironmentVariable("USERNAME");
        string? password = Environment.GetEnvironmentVariable("PASSWORD");
        
        Assert.That(username, Is.Not.Null.And.Not.Empty);
        Assert.That(password, Is.Not.Null.And.Not.Empty);
        
        var login = await _client.AuthenticateAsync(username!, password!);
        Assert.That(login, Is.True);
    }
    
    
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
        
        //Act:
        bool isAccepted = await _client.ConnectAsync(Host, Port);
        
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
        
        //Act
        bool isAccepted = await _client.ConnectAsync(Host, Port) ;
        
        //Assert
        Assert.That(isAccepted, Is.True);
        Assert.DoesNotThrow(() => _client.Disconnect());
    }
    
    
    [Test]
    public async Task AuthenticateAsync_WithValidLogin_ReturnsTrue()
    {
        //Arrange:
        string? username = Environment.GetEnvironmentVariable("USERNAME");
        string? password = Environment.GetEnvironmentVariable("PASSWORD");
            
        bool isAccepted = await _client.ConnectAsync(Host, Port);
        Assert.That(isAccepted, Is.True);
        
        
        //Act
        var login = await _client.AuthenticateAsync(username!, password!);
       
        //Assert
        Assert.That(login, Is.True);
    }
    
    
    [Test]
    public async Task AuthenticateAsync_WithInvalidUser_ReturnsFalse()
    {
        //Arrange:
        string username = "Bobby";
        string password = "tester man";
            
        bool isAccepted = await _client.ConnectAsync(Host, Port);
        Assert.That(isAccepted, Is.True);
        
        
        //Act
        var login = await _client.AuthenticateAsync(username!, password!);
        
        //Assert
        Assert.That(login, Is.False);
    }

    [Test]
    public async Task ListGroupsAsync_WhenAuthenticated_ReturnsList()
    {
        // Arrange
        await ConnectToServerWithCredentials();

        // Act
        var result = await _client.ListNewsGroupsAsync();

        // Assert
        Assert.That(result.hasSucceeded, Is.True);
        Assert.That(result.newsGroupEntries.Any(entry => entry.GroupName == "free.chicken"),Is.True);
    }


    [Test]
    public async Task SelectGroupAsync_WithExistingGroupChicken_ReturnTrue()
    {
        // Arrange
        await ConnectToServerWithCredentials();

        // Act
        var group = await _client.SelectNewsGroupAsync("free.chicken");

        // Assert
        Assert.That(group.hasSucceeded, Is.True);
        Assert.That(group.details, Is.Not.Null);
        Assert.That(group.details.GroupName == "free.chicken");
    }
    
    [Test]
    public async Task SelectGroupAsync_WithExistingGroupJunk_ReturnTrue()
    {
        // Arrange
        await ConnectToServerWithCredentials();
        
        var groups = await _client.ListNewsGroupsAsync();
        Assert.That(groups.hasSucceeded, Is.True);

        // Act
        var group = await _client.SelectNewsGroupAsync("junk");

        // Assert
        Assert.That(group.hasSucceeded, Is.True);
        Assert.That(group.details, Is.Not.Null);
        Assert.That(group.details.GroupName == "junk");
    }
    
    [Test]
    public async Task SelectGroupAsync_With2Groups_ReturnTrue()
    {
        // Arrange
        await ConnectToServerWithCredentials();
        
        var groups = await _client.ListNewsGroupsAsync();
        Assert.That(groups.hasSucceeded, Is.True);

        // Act
        var group = await _client.SelectNewsGroupAsync("control");

        // Assert
        Assert.That(group.hasSucceeded, Is.True);
        Assert.That(group.details, Is.Not.Null);
        Assert.That(group.details.GroupName == "control");
        
        var articles = await _client.ListArticlesAsync(group.details!);
        Assert.That(articles.hasSucceeded, Is.True);
        
        
        group = await _client.SelectNewsGroupAsync("junk");
        
        Assert.That(group.hasSucceeded, Is.True);
        Assert.That(group.details, Is.Not.Null);
        Assert.That(group.details.GroupName == "junk");
    }
    
    
    [Test]
    public async Task SelectGroupAsync_With2GroupsWith30secDelay_ReturnTrue()
    {
        // Arrange
        await ConnectToServerWithCredentials();
        
        var groups = await _client.ListNewsGroupsAsync();
        Assert.That(groups.hasSucceeded, Is.True);

        // Act
        var group = await _client.SelectNewsGroupAsync("control");

        // Assert
        Assert.That(group.hasSucceeded, Is.True);
        Assert.That(group.details, Is.Not.Null);
        Assert.That(group.details.GroupName == "control");
        
        var articles = await _client.ListArticlesAsync(group.details!);
        Assert.That(articles.hasSucceeded, Is.True);
        
        await Task.Delay(TimeSpan.FromSeconds(30));
        
        group = await _client.SelectNewsGroupAsync("junk");
        
        Assert.That(group.hasSucceeded, Is.True);
        Assert.That(group.details, Is.Not.Null);
        Assert.That(group.details.GroupName == "junk");
    }
    
    
    [Test]
    public async Task SelectGroupAsync_WithNonExistingGroup_ReturnFalse()
    {
        // Arrange
        await ConnectToServerWithCredentials();

        // Act
        var group = await _client.SelectNewsGroupAsync("worng.group.chicken");

        // Assert
        Assert.That(group.hasSucceeded, Is.False);
    }
    
    
    [Test]
    public async Task ListArticlesAsync_GroupWithArticles_ReturnTrue()
    {
        // Arrange
        await ConnectToServerWithCredentials();
        
        var selection = await _client.SelectNewsGroupAsync("free.chicken");

        Assert.That(selection.hasSucceeded, Is.True);
        Assert.That(selection.details, Is.Not.Null);
        Assert.That(selection.details!.EstimatedArticles, Is.GreaterThan(0));
        
        // Act
        var result = await _client.ListArticlesAsync(selection.details!);
        
        // Assert
        Assert.That(result.hasSucceeded, Is.True);
        Assert.That(result.articleEntries, Is.Not.Empty);
    }
    
    
    [Test]
    public async Task ListArticlesAsync_GroupWithNoArticles_ReturnFalse()
    {
        //need to find empty group
    }

}