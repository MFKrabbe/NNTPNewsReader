using System.Threading.Tasks;

namespace NNTPNewsReader.Services;

public interface INntpClient
{
     Task<bool> ConnectAsync(string host , int port);
     void StopClient();
     void ReadMessages();
     void WriteMessages();
     void ListArticles();
     void ListGroups();
}