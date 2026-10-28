using System.Net;
using System.Net.Sockets;
using NNTPNewsReader.Services;

namespace NNTPNewsReader.Tests;

public class Tests
{
    [Test]
    public async Task Connect_WhenServerAcceptsConnection_ReturnsTrue()
    {
        //Arrange:
        var client = new NntpClient();
        var host = "news.eternal-september.org";
        var port = 119;
        
        //Act:
        bool accepted = await client.ConnectAsync(host, port);
        
        //Assert
        Assert.That(accepted, Is.True);
    }
}