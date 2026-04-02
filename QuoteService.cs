using System.Net.Http.Json;
using YahooFinanceApi;

public class QuoteService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;
    private readonly StockAPI _apiUsed;
    private readonly decimal[] _mockPrices = { 1, 1, 2, 2, 3, 3, 1, 2, 3, 2, 1 };
    private int _mockCounter = 0;

    public QuoteService(string token, StockAPI apiUsed)
    {
        _httpClient = new HttpClient();
        _token = token;
        _apiUsed = apiUsed;
    }

    public async Task<decimal?> GetPriceAsync(string symbol)
    {
        try 
        {
            if (_apiUsed == StockAPI.Brapi)
            {
                var response = await _httpClient.GetFromJsonAsync<BrapiResponse>(
                    $"https://brapi.dev/api/quote/{symbol}?token={_token}");
                return response?.Results?[0]?.RegularMarketPrice;
            }
            
            if (_apiUsed == StockAPI.Yahoo)
            {
                var response = await Yahoo.Symbols(symbol).Fields(Field.RegularMarketPrice).QueryAsync();
                if (response.ContainsKey(symbol))
                {
                    return Convert.ToDecimal(response[symbol].RegularMarketPrice);
                }
            }

            if (_apiUsed == StockAPI.MyMockData)
            {
                decimal price = _mockPrices[_mockCounter];
                _mockCounter = (_mockCounter + 1) % _mockPrices.Length;
                return price;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao consultar cotação: {ex.Message}");
            return null;
        }
    }
}