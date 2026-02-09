using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace ETaxTracker.Models.Dtos
{

    public class ApiResponse<T>
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = default!;

        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;

        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }

    public class LoginResponseData
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = default!;

        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;

        [JsonPropertyName("data")]
        public UserData Data { get; set; } = default!;

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = default!;
    }

    public class UserData
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [JsonPropertyName("business_id")]
        public Guid BusinessId { get; set; }

        [JsonPropertyName("service_id")]
        public string ServiceId { get; set; } = default!;
    }



}