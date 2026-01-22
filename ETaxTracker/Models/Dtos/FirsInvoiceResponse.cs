using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class FirsInvoiceResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public FirsDataWrapper Data { get; set; }
}

public class FirsDataWrapper
{
    [JsonPropertyName("data")]
    public InvoiceData Data { get; set; }

    [JsonPropertyName("metadata")]
    public List<InvoiceMetadata> Metadata { get; set; }
}

public class InvoiceData
{
    [JsonPropertyName("invoice_number")]
    public string InvoiceNumber { get; set; }

    [JsonPropertyName("irn")]
    public string Irn { get; set; }

    [JsonPropertyName("qr_code")]
    public string QrCodeBase64 { get; set; }
}

public class InvoiceMetadata
{
    [JsonPropertyName("step")]
    public string Step { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
