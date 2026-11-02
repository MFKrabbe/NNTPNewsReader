namespace NNTPNewsReader.Models;

public class ArticlePost
{
    //For the Post command on the server
    public string PostGroupName { get; init; } = "";
    public string PostAuthor { get; init; } = "";
    public string PostSubject { get; init; } = "";
    public string PostBody { get; init; } = "";


    public override string ToString()
    {
        return $"PostGroupName: {PostGroupName}\n" +
               $"PostAuther: {PostAuthor}\n" +
               $"PostSubject: {PostSubject}\n" +
               $"\n" +
               $"PostBody:\n {PostAuthor}";
    }
}




/*
 *   Example of a successful posting:

      [C] POST
      [S] 340 Input article; end with <CR-LF>.<CR-LF>
      [C] From: "Demo User" <nobody@example.net>
      [C] Newsgroups: misc.test
      [C] Subject: I am just a test article
      [C] Organization: An Example Net
      [C]
      [C] This is just a test article.
      [C] .
      [S] 240 Article received OK
 */