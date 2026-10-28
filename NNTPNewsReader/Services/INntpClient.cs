using System.Collections.Generic;
using System.Threading.Tasks;

namespace NNTPNewsReader.Services;

public interface INntpClient
{
     Task<bool> ConnectAsync(string host , int port);
     void Disconnect();
     void ReadMessages();
     void WriteMessages();
     void ListArticles();
     Task<List<string>> ListGroupsAsync();
}