using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NNTPNewsReader.Models;

namespace NNTPNewsReader.Services;


/*
 *8.3.3.  Examples
 *
 * In the first four examples, TAB has been replaced by vertical bar and
 * some lines have been folded for readability.
 *
 * 3000234|I am just a test article|"Demo User"
 * <nobody@example.com>|6 Oct 1998 04:38:40 -0500|
 * <45223423@example.com>|<45454@example.net>|1234|
 * 17|Xref: news.example.com misc.test:3000363
 */


//https://www.w3.org/Protocols/rfc977/rfc977 for return codes
//https://www.rfc-editor.org/info/rfc6048 newer codes --> check Posting status


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
            Console.WriteLine($"ATTEMPTING CONNECTION with host: {host} on port: {port}");
        
            _reader = new StreamReader(_client.GetStream());
            
            string? serverGreeting = await _reader.ReadLineAsync();
            Console.WriteLine($"SERVER CONNECTION response: {serverGreeting}");
            

            
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

    public async Task<(bool hasSucceeded, string articleText)> ReadArticleBodyAsync(int articleNumber)
    {
        try
        {
            var command = Encoding.UTF8.GetBytes($"body {articleNumber}{CRLF}");
            Console.WriteLine($"Sending BODY with article number: {articleNumber}");
            
            await _client!.GetStream().WriteAsync(command);
            Console.WriteLine("BODY sent; waiting for response");

            string? serverResponse = await _reader!.ReadLineAsync();
            Console.WriteLine($"BODY response: {serverResponse}");

            var sBuilder = new StringBuilder();
            var articleBody = "";
                
            if (serverResponse != null && serverResponse.StartsWith("222 "))
            {
                while (true)
                {
                    string? bodyString = await _reader!.ReadLineAsync();
                   
                    if (bodyString == null)
                    {
                        Console.WriteLine("groupString is null");
                        throw new Exception("GroupString is null");
                    }
                    
                    if (bodyString == ".")
                    {
                        break;
                    }
                    
                    if (bodyString.StartsWith(".."))
                    {
                        bodyString = bodyString.Substring(1); //removes the extra dot
                    }
                    
                    sBuilder.AppendLine(bodyString);
                }
                articleBody = sBuilder.ToString();
                Console.WriteLine($"ArticleBody retrieved: \n{articleBody}");
                return (true, articleBody);
            }
            else
            {
                Console.WriteLine("failed to retrieve body");
                return (false, articleBody);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Disconnect();
            throw;
        }
    }

    public async Task<bool> PostArticleAsync(ArticlePost post)
    {
        try
        {
            var nntpFormat = ToNntpPostFormat(post);
            
            var command = Encoding.UTF8.GetBytes($"post{CRLF}");
            Console.WriteLine("Sending POST:");   
            
            await _client!.GetStream().WriteAsync(command);
            Console.WriteLine("POST sent; waiting for response");
            
            string? serverResponse = await _reader!.ReadLineAsync();
            Console.WriteLine($"POST response: {serverResponse}");
            
            if(serverResponse == null || !serverResponse.StartsWith("340 "))
            {
                return false;
            }
            
            command = Encoding.UTF8.GetBytes(nntpFormat);
            Console.WriteLine($"Sending Formatted POST: {nntpFormat}");
            
            await _client!.GetStream().WriteAsync(command);
            Console.WriteLine("POST sent; waiting for response");
            
            serverResponse = await _reader!.ReadLineAsync();
            
            return serverResponse != null && serverResponse!.StartsWith("240 ");
        }
        catch (Exception e)
        {
            Disconnect();
            Console.WriteLine(e);
            throw;
        }
    }
    

    public string ToNntpPostFormat(ArticlePost post)
    {
        var sBuilder = new StringBuilder();

        sBuilder.Append($"From: {post.PostAuthor}{CRLF}");
        sBuilder.Append($"Newsgroups: {post.PostGroupName}{CRLF}");
        sBuilder.Append($"Subject: {post.PostSubject}{CRLF}");
        sBuilder.Append(CRLF); //space between the Headesr and body

        using var reader = new StringReader(post.PostBody);
        
        string? line = reader.ReadLine();

        while (line != null)
        {
            if (line.StartsWith("."))
            {
                sBuilder.Append(".");  //adding extra a dot
            }
            sBuilder.Append(line);
            sBuilder.Append(CRLF);
            
            line = reader.ReadLine();
        }
        
        sBuilder.Append(".");
        sBuilder.Append(CRLF);
        
        return sBuilder.ToString();
        
        
        /*
         * Thus a single dot (".")
         * on a line indicates the end of the text, and lines starting with a
         * dot in the original text have that dot doubled during transmission.
         */
    }
    
    
    
    

    //https://www.rfc-editor.org/info/rfc3977/#section-8.3 
    public async Task<(bool hasSucceeded, List<ArticleEntry> articleEntries)> ListArticlesAsync(GroupDetails groupDetails)
    {
        try
        {
            int estimatedArticles = groupDetails.EstimatedArticles;
            int firstArticle = groupDetails.FirstArticle;
            int lastArticle = groupDetails.LastArticle;

            var articleList = new List<ArticleEntry>();
            
            if (estimatedArticles == 0 || lastArticle < firstArticle)
            {
                Console.WriteLine("no articles");
                return (true, articleList);
            }
            
            var command = Encoding.UTF8.GetBytes($"over {firstArticle}-{lastArticle}{CRLF}");
            await _client!.GetStream().WriteAsync(command);
            Console.WriteLine($"Sending OVER {firstArticle}-{lastArticle}");
        
            string? serverResponse = await _reader!.ReadLineAsync();
            Console.WriteLine($"OVER response: {serverResponse}");
            
            if (serverResponse != null && serverResponse.StartsWith("224 "))
            {
                while (true)
                {
                    string? articleString = await _reader!.ReadLineAsync();
                    
                    if (articleString == null)
                    {
                        throw new Exception("ArticleString is null");
                    }

                    if (articleString == ".")
                    {
                        break;
                    }
                    
                    var articleStringParts = articleString.Split('\t');
                    
                    if (articleStringParts.Length < 4)
                    {
                        Console.WriteLine(articleStringParts);
                        throw new Exception("Invalid group string");
                    }

                    var articleNumber = int.Parse(articleStringParts[0]);
                    var articleSubject = articleStringParts[1];
                    var articleAuther = articleStringParts[2];
                    var articleDate = articleStringParts[3];

                    var articleEntry = new ArticleEntry
                    {
                        ArticleNumber =  articleNumber,
                        ArticleSubject = articleSubject,
                        ArticleAuthor = articleAuther,
                        ArticleDate = articleDate
                    };

                    Console.WriteLine(articleEntry.ArticleSubject);
                    articleList.Add(articleEntry);
                }
                Console.WriteLine("OVER response fully read");
            }
            else
            {
                Console.WriteLine("no articles in range or null");
                return (false, articleList);
            }
            return (true, articleList);
        }
        catch (Exception e)
        {
            Disconnect();
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task<(bool hasSucceeded, List<NewsGroupEntry> newsGroupEntries)> ListNewsGroupsAsync()
    {
        try
        {
            var command = Encoding.UTF8.GetBytes($"list{CRLF}");
            Console.WriteLine("Sending LIST");
            
            await _client!.GetStream().WriteAsync(command); 
            Console.WriteLine("LIST sent; waiting for response");
        
            string? serverResponse = await _reader!.ReadLineAsync();
            Console.WriteLine($"LIST response: {serverResponse}");
        
            var newsgroups = new List<NewsGroupEntry>();

            if (serverResponse != null && serverResponse.StartsWith("215 "))
            {
                while (true)
                {
                    string? groupString = await _reader!.ReadLineAsync();

                    if (groupString == null)
                    {
                        Console.WriteLine("groupString is null");
                        throw new Exception("GroupString is null");
                    }

                    if (groupString == ".")
                    {
                        break;
                    }
                
                    //split the return msg  fx free.chicken 000000029 000000001 y
                    //                              0           1         2     3
                    var stringParts = groupString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (stringParts.Length != 4)
                    {
                        Console.WriteLine(stringParts);
                        throw new Exception("Invalid group string");
                    }
                
                    var name = stringParts[0];
                    var lastArticle = int.Parse(stringParts[1]);
                    var firstArticle =int.Parse(stringParts[2]);
                    var status = stringParts[3];

                    var newsGroup = new NewsGroupEntry
                    {
                        GroupName =  name,
                        LastArticle = lastArticle,
                        FirstArticle = firstArticle,
                        Status = status
                    };
                
                    newsgroups.Add(newsGroup);
                }
            }
            else
            {
                return  (false, newsgroups);
            }

            return (true, newsgroups);
        }
        catch (Exception e)
        {
            Disconnect();
            Console.WriteLine(e);
            throw;
        }
        
        
    }
    
    
    //GroupDetails? because selection can potentially fail with 411 and return no GroupDetails
    
    public async Task<(bool hasSucceeded, GroupDetails? details)> SelectNewsGroupAsync(string newsGroupEntryName)
    {
        try
        {
            var command = Encoding.UTF8.GetBytes($"group {newsGroupEntryName}{CRLF}");
            Console.WriteLine($"Sending GROUP: {newsGroupEntryName}");
            
            await _client!.GetStream().WriteAsync(command);
            Console.WriteLine("GROUP sent; waiting for response");
            
            string? serverResponse = await _reader!.ReadLineAsync();
            Console.WriteLine($"GROUP response: {serverResponse}");

            if (serverResponse != null && serverResponse.StartsWith("211 "))
            {
                var articleSummaryParts = serverResponse.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                
                //211 n f l s group selected
                // 0  1 2 3 4  5        6
                
                if (articleSummaryParts.Length < 5)
                {
                    Console.WriteLine(articleSummaryParts);
                    throw new Exception("Invalid group string");
                }
                
                var estimatedArticles = int.Parse(articleSummaryParts[1]); //n
                var firstArticle = int.Parse(articleSummaryParts[2]); //f
                var lastArticle = int.Parse(articleSummaryParts[3]); //l
                var groupName = articleSummaryParts[4]; //s

                var groupDetails = new GroupDetails
                {
                    FirstArticle = firstArticle,
                    LastArticle = lastArticle,
                    GroupName = groupName,
                    EstimatedArticles = estimatedArticles
                };
                
                return (true, groupDetails);
            }
            else
            {
                return (false, null);
            }
            
        }
        catch (Exception e)
        {
            Disconnect();
            Console.WriteLine(e);
            throw;
        }
    }


    //https://learn.microsoft.com/en-us/dotnet/api/system.string.split?view=net-10.0
    //https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-tuples
    //https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-10.0
    public async Task<bool> AuthenticateAsync(string username, string password)
    {
        try
        {
            var command = Encoding.UTF8.GetBytes($"authinfo user {username}{CRLF}");
            Console.WriteLine($"Sending AUTHINFO USER with name: {username}");

            await _client!.GetStream().WriteAsync(command);
            Console.WriteLine("AUTHINFO USER sent; waiting for response");
            
            string? serverResponse = await _reader!.ReadLineAsync();
            Console.WriteLine($"AUTHINFO response: {serverResponse}");

            if (serverResponse != null && serverResponse.StartsWith("281 "))
            {
                //if a server only requires username
                return true;
            }

            if (serverResponse == null || !serverResponse.StartsWith("381 "))
            {
                //catches everything this not the 381 request password code (481, 482 or 500 what?) or no response
                return false;
            }

            command = Encoding.UTF8.GetBytes($"authinfo pass {password}{CRLF}");

            await _client!.GetStream().WriteAsync(command);
            serverResponse = await _reader!.ReadLineAsync();
            
            return serverResponse != null && serverResponse.StartsWith("281 ");
        }
        catch(Exception e)
        {
            Disconnect();
            Console.WriteLine(e);
            throw;
        }
    }
}