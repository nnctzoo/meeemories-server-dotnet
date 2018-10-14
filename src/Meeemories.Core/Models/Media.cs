using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Meeemories.Core.Models
{
    public class Media : TableEntity, IMedia
    {
        [IgnoreProperty]
        public string Id { get => RowKey; set { } }

        [IgnoreProperty]
        public string RoomId { get => PartitionKey; set { } }

        [IgnoreProperty]
        public MediaStatus Status { get => (MediaStatus)Enum.Parse(typeof(MediaStatus), StatusStr); set => StatusStr = value.ToString(); }
        public string StatusStr { get; set; } = MediaStatus.Ready.ToString();
        
        public DateTimeOffset PostedAt { get; set; }

        [IgnoreProperty]
        public MediaType MediaType { get => (MediaType)Enum.Parse(typeof(MediaType), MediaTypeStr); set => MediaTypeStr = value.ToString(); }
        public string MediaTypeStr { get; set; } = MediaType.Image.ToString();

        public string DeleteToken { get; set; }
        
        public string Url { get; set; }

        [IgnoreProperty]
        public IEnumerable<Source> Sources 
        { 
            get => JsonConvert.DeserializeObject<IEnumerable<Source>>(SourcesJSON);
            set => SourcesJSON = JsonConvert.SerializeObject(value);
        }
        public string SourcesJSON { get; set; }
    }
}