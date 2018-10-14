using System;
using System.Collections.Generic;

namespace Meeemories.Core.Models
{
    public interface IMedia
    {
        string Id { get; set;  } 
        string RoomId { get; set; }

        MediaStatus Status { get; set; }
        DateTimeOffset PostedAt { get; set; }
        MediaType MediaType { get; set; }
        string DeleteToken { get; set; }
        string Url { get; set; }
        IEnumerable<Source> Sources { get; set; }
    }
}