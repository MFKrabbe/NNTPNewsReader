using System;

namespace NNTPNewsReader.Models;

public class NewsGroupEntry
{
    //For the LIST command on the server

    public string GroupName { get; init; } = "";
    public int LastArticle { get; init; }
    public int FirstArticle { get; init; }
    public string Status { get; init; } = "";
    // y = Can post, n = cannot post, m = controlled by a moderator

    public int EstimatedArticles
    {
        get
        {
            if (FirstArticle <= 0 || LastArticle < FirstArticle)
            {
                return 0;
            }

            return LastArticle - FirstArticle + 1;
        }
       
    } 
}






