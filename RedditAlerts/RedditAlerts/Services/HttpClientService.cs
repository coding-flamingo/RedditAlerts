using Polly;
using Polly.Retry;
using RedditAlerts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RedditAlerts.Services;

public class HttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    public HttpClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        HttpStatusCode[] httpStatusCodesWorthRetrying = {
               HttpStatusCode.RequestTimeout, // 408
               HttpStatusCode.InternalServerError, // 500
               HttpStatusCode.BadGateway, // 502
               HttpStatusCode.ServiceUnavailable, // 503
               HttpStatusCode.GatewayTimeout // 504
            };
        httpClient.Timeout = TimeSpan.FromMinutes(15);
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrInner<TaskCanceledException>()
            .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
              .WaitAndRetryAsync(new[]
              {
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(15)
              });
    }

    public async Task<APIResultModel> CallGetApiAsync(string url, string? token = null)
    {
        APIResultModel apiResult = new APIResultModel();
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentNullException("URL is empty or null", nameof(url));
        }
        try
        {
            HttpRequestMessage requestMessage = new (HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(token))
            {
                requestMessage.Headers.Authorization = new
                    System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            HttpResponseMessage response;
            response = await _retryPolicy.ExecuteAsync(async () =>
                     await SendMessageAsync(requestMessage)
                );
            apiResult.Success = response.IsSuccessStatusCode;
            apiResult.Message = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            apiResult.Success = false;
            if (ex.Message.Contains("One or more errors"))
            {
                apiResult.Message = ex.InnerException?.Message ?? string.Empty;
            }
            else
            {
                apiResult.Message = ex.Message;
            }
        }
        return apiResult;
    }

    public async Task<APIResultModel> CallPost(string url, string jsonPayload, string token = "")
    {
        APIResultModel apiResult = new APIResultModel();
        HttpResponseMessage responseMessage;
        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        if (!string.IsNullOrWhiteSpace(token))
        {
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        try
        {
            requestMessage.Content = new StringContent(jsonPayload,
                Encoding.UTF8, "application/json");
            responseMessage = await _retryPolicy.ExecuteAsync(async () =>
                      await SendMessageAsync(requestMessage));
            apiResult.Message = await responseMessage.Content.ReadAsStringAsync();
            apiResult.Success = responseMessage.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            apiResult.Success = false;
            if (ex.Message.Contains("One or more errors"))
            {
                apiResult.Message = ex.InnerException?.Message ?? string.Empty;
            }
            else
            {
                apiResult.Message = ex.Message;
            }
        }
        return apiResult;
    }

    public async Task<byte[]> GetBytesAsync(string url)
    {
        HttpResponseMessage responseMessage;
        HttpRequestMessage requestMessage = new(HttpMethod.Get, url);
        responseMessage = await _retryPolicy.ExecuteAsync(async () =>
                    await SendMessageAsync(requestMessage));
        if (responseMessage.IsSuccessStatusCode)
        {
            return await responseMessage.Content.ReadAsByteArrayAsync();
        }
        else
        {
            return new byte[0];
        }
    }

    private async Task<HttpResponseMessage> SendMessageAsync(HttpRequestMessage requestMessage)
    {
        HttpResponseMessage response;
        response = await _httpClient.SendAsync(requestMessage);
        return response;
    }
}