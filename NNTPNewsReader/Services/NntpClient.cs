using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NNTPNewsReader.Services;



//https://www.w3.org/Protocols/rfc977/rfc977 for return codes


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
            
            string? serverGreeting = await _reader.ReadLineAsync();

            
            bool isAccepted = serverGreeting != null && (serverGreeting.StartsWith("200 ") || serverGreeting.StartsWith("201 "));
            
            if (!isAccepted)
            {
                Disconnect(); //for at server greeting mismatch
                return false;
            }
            
            return true;
            
        }
        catch (Exception e)
        {
            Disconnect(); //for a failiure in creation of Objects or connection
            Console.WriteLine(e);
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

    public async Task<List<string>> ListGroupsAsync()
    {
        var command = Encoding.UTF8.GetBytes("list" + CRLF);
        await _client!.GetStream().WriteAsync(command); 
        
        string? serverResponse = await _reader!.ReadLineAsync();
        
        var newsgroups = new List<string>();

        if (serverResponse != null && serverResponse.StartsWith("215 "))
        {
            while (true)
            {
                string? groupName = await _reader!.ReadLineAsync();

                if (groupName == null)
                {
                    Console.WriteLine("groupnames is null");
                    break;
                }

                if (groupName == ".")
                {
                    break;
                }
                
                //gets the groupname only from the return of fx free.chicken 000000029 000000001
                //                                                   0           1         2
                newsgroups.Add(groupName.Split(" ")[0]);
            }
        }

        return newsgroups;
    }

    public Task<(bool hasSucceeded, string? responseCode)> SelectGroupAsync(string groupName)
    {
        throw new NotImplementedException();
    }


    //https://learn.microsoft.com/en-us/dotnet/api/system.string.split?view=net-10.0
    //https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-tuples
    //https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-10.0
    public async Task<(bool hasSucceded, string? responseCode)> AuthenticateAsync(string username, string password)
    {
        try
        {

            var command = Encoding.UTF8.GetBytes($"authinfo user {username}{CRLF}");

            await _client!.GetStream().WriteAsync(command);
            string? serverResponse = await _reader!.ReadLineAsync();

            if (serverResponse != null && serverResponse.StartsWith("281"))
            {
                //if a server only requires username
                return (true, "281");
            }

            if (serverResponse == null || !serverResponse.StartsWith("381"))
            {
                //catches everything this not the 381 request password code (481, 482 or 500 what?) or no response
                return (false, serverResponse?.Split(' ')[0]);
            }

            command = Encoding.UTF8.GetBytes($"authinfo pass {password}{CRLF}");

            await _client!.GetStream().WriteAsync(command);
            serverResponse = await _reader!.ReadLineAsync();

            bool succeded = serverResponse != null && serverResponse.StartsWith("281");
            string? responseCode = serverResponse?.Split(" ")[0];


            return (succeded, responseCode);
        }
        catch(Exception e)
        {
            Disconnect();
            Console.WriteLine(e);
            throw;
        }
    }
}