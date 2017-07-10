namespace WebApp.Models
{
    /// <summary>
    /// Many-to-many relationship beetween <see cref="WebApp.Models.UserCollection"/> and <see cref="WebApp.Models.Source"/>
    /// </summary>
    public class UserCollectionSource
    {
        public int UserCollectionId { get; set; }
        public UserCollection UserCollection { get; set; }
        public int SourceId { get; set; }
        public Source Source { get; set; }
    }
}
