using System;
using System.Collections.Generic;
using Meeemories.Core.Models;

namespace Meeemories.Web.Models
{
    public class Media : IMedia
    {
        public string Id { get; set; }

        public MediaStatus Status { get; set; }

        public DateTimeOffset PostedAt { get; set; }
        
        public MediaType MediaType { get; set; }
        
        public string Url { get; set; }

        public IEnumerable<Source> Sources { get; set; }

        public string RoomId { get; set; } = "Default";

        public string DeleteToken { get; set; }
    }
}