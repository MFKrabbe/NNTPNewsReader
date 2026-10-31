namespace NNTPNewsReader.Models;

public class GroupDetails
{
    //For the Group command on the server
    
    public int EstimatedArticles { get; init; }
    public int FirstArticle { get; init; }
    public int LastArticle { get; init; }
    public string GroupName { get; init; } = "";

}