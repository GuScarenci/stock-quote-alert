using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using YahooFinanceApi;

// Loading configurations
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Define api used
string apiProviderStr = config["Settings:StockApiProvider"] ?? "Yahoo";
if (!Enum.TryParse(apiProviderStr, true, out StockAPI apiUsed)) 
{
    apiUsed = StockAPI.Yahoo; // Default seguro
}

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

Console.WriteLine($"Monitorando {alert.Symbol}... (Ctrl+C para sair)");

Level position = Level.BetweenPrices;
Level lastPosition = Level.BetweenPrices;


// Monitoring Loop
while (true)
{
    try
    {
        decimal? currentPrice = await quoteService.GetPriceAsync(alert.Symbol);

        if (currentPrice.HasValue)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {alert.Symbol}: R$ {currentPrice}");

            // Alert logic
            if (currentPrice > alert.SellPrice)
            {
                position = Level.SellPrice;
                if (lastPosition != position)
                {
                    emailService.SendAlert($"SELL {alert.Symbol}", $"The price is at R$ {currentPrice}. Sell suggestion above R$ {alert.SellPrice}.");
                    Console.WriteLine("SELL alert sent!");
                }
            }
            else if (currentPrice < alert.BuyPrice)
            {
                position = Level.BuyPrice;
                if (lastPosition != position)
                {
                    emailService.SendAlert($"BUY {alert.Symbol}", $"The price is at R$ {currentPrice}. Buy suggestion below R$ {alert.BuyPrice}.");
                    Console.WriteLine("BUY alert sent!");
                }
            }
            else
            {
                position = Level.BetweenPrices;
                if (lastPosition != position)
                {
                    if (lastPosition == Level.BuyPrice)
                        Console.WriteLine("DON'T BUY alert sent!");
                    else if (lastPosition == Level.SellPrice)
                        Console.WriteLine("DON'T SELL alert sent!");
                }
            }

            lastPosition = position;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    // await Task.Delay(TimeSpan.FromMinutes(1));
    await Task.Delay(TimeSpan.FromSeconds(3));
}
