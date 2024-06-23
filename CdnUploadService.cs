using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

public class CdnUploadService
{
    private readonly HttpClient _httpClient;
    private readonly CdnSettings _cdnSettings;

    public CdnUploadService(HttpClient httpClient, IOptions<CdnSettings> cdnSettings)
    {
        _httpClient = httpClient;
        _cdnSettings = cdnSettings.Value;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string resource)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is null or empty", nameof(file));

        using (var content = new MultipartFormDataContent())
        {
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.FileName);

            var requestUri = $"{_cdnSettings.CdnUrl}/cdn/upload?resource={resource}";

            var response = await _httpClient.PostAsync(requestUri, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            // Assuming the response is in the format {"message": "OK", "data": "URL", "ts": "timestamp"}
            var result = System.Text.Json.JsonSerializer.Deserialize<CdnUploadResponse>(responseContent);

            return result?.Data ?? throw new Exception("Upload failed, no URL returned");
        }
    }
}

public class CdnSettings
{
    public string CdnUrl { get; set; }
}

public class CdnUploadResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; }

    [JsonPropertyName("ts")]
    public string Ts { get; set; }
}
