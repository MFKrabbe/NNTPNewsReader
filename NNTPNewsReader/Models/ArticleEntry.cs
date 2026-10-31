namespace NNTPNewsReader.Models;

//https://www.rfc-editor.org/info/rfc3977/
public class ArticleEntry
{ 
    public int ArticleNumber { get; init; }
    public string ArticleSubject { get; init; } = "";
    public string ArticleAuthor { get; init; } = "";
    public string ArticleDate { get; init; } = "";
}

/*
 * The first 8 fields MUST be the following, in order:

      "0" or article number (see below)
      Subject header content
      From header content
      Date header content
      Message-ID header content
      References header content
      :bytes metadata item
      :lines metadata item
 */