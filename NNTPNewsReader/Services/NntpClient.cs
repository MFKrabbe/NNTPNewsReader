using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NNTPNewsReader.Services;

public class NntpClient : INntpClient
{
    private const string CRLF = "\r\n";
    
    private TcpClient? _client;
    private  StreamReader? _reader;
    
    //Sets up TcpClient and StreamReader and connects to the given host and port
    public async Task<bool> ConnectAsync(string host, int port)
    {
        //const string host = "news.eternal-september.org";
        //const int port = 119;
        
        try
        {
            _client =  new TcpClient();
            await _client.ConnectAsync(host, port);
        
            _reader = new StreamReader(_client.GetStream());
            //StreamReader har allerede default som Encoding.UTF-8 og buffersize på 1024, så der er ingen grund til at sætte det som Tommy
            
            string? serverGreeting = await _reader.ReadLineAsync();

            
            bool isAccepted = serverGreeting != null && (serverGreeting.StartsWith("200 ") || serverGreeting.StartsWith("201 "));
            
            if (!isAccepted)
            {
                Disconnect();
                return false;
            }
            
            return true;
            
        }
        catch (Exception e)
        {
            Disconnect();
            Console.WriteLine("Error: " + e.Message);
            throw;
        }
        
    }
    
    public void Disconnect()
    {
        _reader?.Dispose();
        _client?.Dispose();
        
        _reader = null;
        _client = null;
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