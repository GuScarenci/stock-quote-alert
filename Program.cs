using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using YahooFinanceApi;

// Loading configurations
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Define api used
StockAPI apiUsed = StockAPI.MyMockData;

// Validate console arguments
if (args.Length < 3)
{
    Console.WriteLine("Uso: stock-quote-alert PETR4 22.67 22.59");
    return;
}

// Safely reading Brapi Token
string token = "";
if (apiUsed == StockAPI.Brapi)
{
    token = config["Settings:BrapiToken"] ?? throw new Exception("BrapiToken não configurado no appsettings.json.");
}

var alert = new AlertConfig(args[0], decimal.Parse(args[1]), decimal.Parse(args[2]));
var quoteService = new QuoteService(token, apiUsed);
var emailService = new EmailService(config);
using var httpClient = new HttpClient();

Console.WriteLine($"Monitorando {alert.Symbol}... (Ctrl+C para sair)");

Level position = Level.BetweenPrices;
Level lastPosition = Level.BetweenPrices;

decimal[] mockPrices = { 1, 1, 2, 2, 3, 3, 1, 2, 3, 2, 1 };

// Monitoring Loop
int counter = 0;
while (true)
{
    try
    {
        decimal? currentPrice = await quoteService.GetPriceAsync(alert.Symbol);

        if (apiUsed == StockAPI.Brapi)
        {
            var url = $"https://brapi.dev/api/quote/{alert.Symbol}?token={token}";
            var response = await httpClient.GetFromJsonAsync<BrapiResponse>(url);

            if (response?.Results != null && response.Results.Count > 0)
            {
                currentPrice = response.Results[0].RegularMarketPrice;
            }
        }
        else if (apiUsed == StockAPI.Yahoo)
        {
            var response = await Yahoo.Symbols(alert.Symbol).Fields(Field.RegularMarketPrice).QueryAsync();

            if (response.ContainsKey(alert.Symbol))
            {
                currentPrice = Convert.ToDecimal(response[alert.Symbol].RegularMarketPrice);
            }
        }
        else if (apiUsed == StockAPI.MyMockData)
        {
            currentPrice = mockPrices[counter];
        }

        if (lastPosition != position)
        {
            lastPosition = position;
        }

        if (currentPrice.HasValue)
        {

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {alert.Symbol}: R$ {currentPrice}");


            // Lógica de Alerta de compra ou venda
            if (currentPrice > alert.SellPrice)
            {
                position = Level.SellPrice;
                if (lastPosition != position)
                {
                    emailService.SendAlert($"Venda {alert.Symbol}", $"O preço atingiu R$ {currentPrice}. Sugestão de venda acima de R$ {alert.SellPrice}.");
                    Console.WriteLine("Alerta de VENDA enviado!");
                }
            }
            else if (currentPrice < alert.BuyPrice)
            {
                position = Level.BuyPrice;
                if (lastPosition != position)
                {
                    emailService.SendAlert($"Compra {alert.Symbol}", $"O preço caiu para R$ {currentPrice}. Sugestão de compra abaixo de R$ {alert.BuyPrice}.");
                    Console.WriteLine("Alerta de COMPRA enviado!");
                }
            }
            else
            {
                position = Level.BetweenPrices;
                if (lastPosition != position)
                {
                    if (lastPosition == Level.BuyPrice)
                    {
                        Console.WriteLine("Alerta de NÃO COMPRA enviado!");
                    }
                    else if (lastPosition == Level.SellPrice)
                    {
                        Console.WriteLine("Alerta de NÃO VENDA enviado!");
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro: {ex.Message}");
    }
    counter++;
    if (counter > 10)
    {
        counter = 0;
    }

    await Task.Delay(TimeSpan.FromMinutes(1));
    //await Task.Delay(TimeSpan.FromSeconds(3));
}
