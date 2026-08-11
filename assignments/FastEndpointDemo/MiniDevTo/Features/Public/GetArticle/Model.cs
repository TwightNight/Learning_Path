using System.Text.Json.Serialization;

namespace MiniDevTo.Features.Public.GetArticle
{
    public class Request
    {
        public int Id { get; set; }
    }
    public class Response
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string CreationDate => CreatedOn.ToString("yyyy-MM-dd"); 

        [JsonIgnore]
        public DateTime CreatedOn { get; set; }

    }
    
}