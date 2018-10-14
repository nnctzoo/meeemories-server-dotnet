namespace Meeemories.Core.Models
{
    public class Settings
    {
        public string StorageConnectionString { get; set; }
        public string TableName => "meeemories";
        public string BlobContainerName => "meeemories";
        public string ImageQueneName => "images";
        public string MovieQueneName => "movies";
    }
}