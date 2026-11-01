using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NNTPNewsReader.Models;

namespace NNTPNewsReader.Services;

public interface INntpClient
{
     Task<bool> ConnectAsync(string host , int port);
     void Disconnect();
     Task<(bool hasSucceeded, string articleText)> ReadArticleBodyAsync(int articleNumber);
     Task<bool> PostArticleAsync(ArticlePost post);
     Task<(bool hasSucceeded, List<ArticleEntry> articleEntries)> ListArticlesAsync(GroupDetails groupDetails);
     Task<bool> AuthenticateAsync(string username, string password);
     Task<(bool hasSucceeded, List<NewsGroupEntry> newsGroupEntries)> ListNewsGroupsAsync();
     Task<(bool hasSucceeded, GroupDetails? details)> SelectNewsGroupAsync(string newsGroupEntryName);
}