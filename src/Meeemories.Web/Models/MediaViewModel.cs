using System;
using System.Collections.Generic;
using Meeemories.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Meeemories.Web.Models
{
    public class MediaViewModel
    {
        public string Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MediaStatus Status { get; set; }

        public DateTimeOffset PostedAt { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public MediaType MediaType { get; set; }

        public string Url { get; set; }

        public IEnumerable<Source> Sources { get; set; }

    }
}