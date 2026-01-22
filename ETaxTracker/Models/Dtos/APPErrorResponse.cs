using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
namespace ETaxTracker.Models.Dtos
{    
    public class APPErrorResponse
    {
        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("extra")]
        public string? Extra { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("pagination")]
        public string? Pagination { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }
    }

}
