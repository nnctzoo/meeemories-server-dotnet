namespace Meeemories
{
    public class Settings
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string TableName { get; set; }
        public string ImageQueueName { get; set; }
        public string VideoQueueName { get; set; }
        public string Password { get; set; }
        public int ExpiredMunites { get; set; }
    }
}
