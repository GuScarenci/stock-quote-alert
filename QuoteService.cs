using System.Net.Http.Json;
public class QuoteService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;

    public QuoteService(string token)
    {
        _httpClient = new HttpClient();
        _token = token;
    }

    public async Task<decimal?> GetPriceAsync(string symbol)
    {
        try 
        {
            var response = await _httpClient.GetFromJsonAsync<BrapiResponse>(
                $"https://brapi.dev/api/quote/{symbol}?token={_token}");
            
            return response?.Results?[0]?.RegularMarketPrice;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao consultar API: {ex.Message}");
            return null;
        }
    }
}