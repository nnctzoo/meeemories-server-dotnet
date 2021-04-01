using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Meeemories.Functions.Models
{
    public class Media : ITableEntity
    {
        public const string ROW_KEY = "MEDIA";

        [IgnoreProperty]
        public string Id { get => PartitionKey; set => PartitionKey = value; }

        [JsonIgnore]
        public string PartitionKey { get; set; }

        [JsonIgnore]
        public string RowKey { get; set; } = ROW_KEY;

        [IgnoreProperty]
        [JsonIgnore]
        public MediaStatus Status { get => (MediaStatus)Enum.Parse(typeof(MediaStatus), StatusStr); set => StatusStr = value.ToString(); }

        [JsonProperty("status")]
        public string StatusStr { get; set; } = MediaStatus.Ready.ToString();

        public DateTimeOffset PostedAt { get; set; }

        [IgnoreProperty]
        [JsonIgnore]
        public MediaType MediaType { get => (MediaType)Enum.Parse(typeof(MediaType), MediaTypeStr); set => MediaTypeStr = value.ToString(); }

        [JsonProperty("type")]
        public string MediaTypeStr { get; set; } = MediaType.Image.ToString();

        [JsonIgnore]
        public string DeleteToken { get; set; }

        public string Url { get; set; }

        [IgnoreProperty]
        public IEnumerable<MediaSource> Sources
        {
            get => JsonConvert.DeserializeObject<IEnumerable<MediaSource>>(SourcesJSON ?? "[]");
            set => SourcesJSON = JsonConvert.SerializeObject(value);
        }

        [JsonIgnore]
        public string SourcesJSON { get; set; }

        [JsonIgnore]
        public DateTimeOffset Timestamp { get; set; }

        [JsonIgnore]
        public string ETag { get; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            foreach(var kv in properties)
            {
                switch(kv.Key)
                {
                    case nameof(PartitionKey):
                        PartitionKey = kv.Value.StringValue;
                        break;
                    case nameof(RowKey):
                        RowKey = kv.Value.StringValue;
                        break;
                    case nameof(Timestamp):
                        Timestamp = kv.Value.DateTimeOffsetValue ?? default;
                        break;
                    case nameof(ETag):
                        ETag = kv.Value.StringValue;
                        break;
                    case nameof(MediaTypeStr):
                        MediaTypeStr = kv.Value.StringValue;
                        break;
                    case nameof(StatusStr):
                        StatusStr = kv.Value.StringValue;
                        break;
                    case nameof(Url):
                        Url = kv.Value.StringValue;
                        break;
                    case nameof(SourcesJSON):
                        SourcesJSON = kv.Value.StringValue;
                        break;
                    case nameof(PostedAt):
                        PostedAt = kv.Value.DateTimeOffsetValue ?? default;
                        break;
                    case nameof(DeleteToken):
                        DeleteToken = kv.Value.StringValue;
                        break;
                    default:
                        break;
                }
            }
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return new Dictionary<string, EntityProperty>
            {
                [nameof(PartitionKey)] = new EntityProperty(PartitionKey),
                [nameof(RowKey)] = new EntityProperty(RowKey),
                [nameof(Timestamp)] = new EntityProperty(Timestamp),
                [nameof(ETag)] = new EntityProperty(ETag),
                [nameof(MediaTypeStr)] = new EntityProperty(MediaTypeStr),
                [nameof(StatusStr)] = new EntityProperty(StatusStr),
                [nameof(Url)] = new EntityProperty(Url),
                [nameof(SourcesJSON)] = new EntityProperty(SourcesJSON),
                [nameof(PostedAt)] = new EntityProperty(PostedAt),
                [nameof(DeleteToken)] = new EntityProperty(DeleteToken),
            };
        }
    }
}
