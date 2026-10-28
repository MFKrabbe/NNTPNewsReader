using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NNTPNewsReader.Services;

public class NntpClient : INntpClient
{
    private const string CRLF = "\r\n";
    
    public async Task<bool> ConnectAsync(string host, int port)
    {
        //const string host = "news.eternal-september.org";
        //const int port = 119;
        
        using var client =  new TcpClient();
        await client.ConnectAsync(host, port);
        
        var ns = client.GetStream();
        using var reader = new StreamReader(ns);
        //StreamReader har allerede default som Encoding.UTF-8 og buffersize på 1024, så der er ingen grund til at sætte det som Tommy
        
        string? greeting = await reader.ReadLineAsync();

        return greeting != null && (greeting.StartsWith("200 ") || greeting.StartsWith("201 "));
        
    }
    
    public void StopClient()
    {
        throw new System.NotImplementedException();
    }

    public void ReadMessages()
    {
        throw new System.NotImplementedException();
    }

    public void WriteMessages()
    {
        throw new System.NotImplementedException();
    }

    public void ListArticles()
    {
        throw new System.NotImplementedException();
    }

    public void ListGroups()
    {
        throw new System.NotImplementedException();
    }
}