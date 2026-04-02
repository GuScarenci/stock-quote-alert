using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using YahooFinanceApi;

// Carregar Configurações
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Validar Argumentos via linha de comando
if (args.Length < 3)
{
    Console.WriteLine("Uso: stock-quote-alert PETR4 22.67 22.59");
    return;
}

// Lendo o Token de forma segura
string token = config["Settings:BrapiToken"] ?? throw new Exception("BrapiToken não configurado no appsettings.json.");

var alert = new AlertConfig(args[0], decimal.Parse(args[1]), decimal.Parse(args[2]));
var emailService = new EmailService(config);
using var httpClient = new HttpClient();

Console.WriteLine($"Monitorando {alert.Symbol}... (Ctrl+C para sair)");

Level position = Level.BetweenPrices;
Level lastPosition = Level.BetweenPrices;

// decimal[] placeHolderPrices = { 1, 1, 2, 2, 3, 3, 1, 2, 3, 2, 1 };

// Loop de Monitoramento~
int counter = 0;
while (true)
{
    try
    {
        var url = $"https://brapi.dev/api/quote/{alert.Symbol}?token={token}";
        // var response = await httpClient.GetFromJsonAsync<BrapiResponse>(url);

        // var symbols = new[] { "PETR4.SA", "AAPL" }; 
        var response = await Yahoo.Symbols(alert.Symbol).Fields(Field.RegularMarketPrice).QueryAsync();
        // var stockPrice = response[alert.Symbol].RegularMarketPrice;

        if (lastPosition != position)
        {
            lastPosition = position;
        }

        // Verificação de segurança (evita NullReferenceException)
        // if (response != null && response.Results != null && response.Results.Count > 0)
        // {

        if (response.ContainsKey(alert.Symbol))
        {
            //Yahoo
            var stockData = response[alert.Symbol];
            decimal currentPrice = Convert.ToDecimal(stockData.RegularMarketPrice);

            //Brapi
            //var currentPrice = response.Results[0].RegularMarketPrice;

            //MockPrices
            // var currentPrice = placeHolderPrices[counter];

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

enum Level
{
    BuyPrice,
    BetweenPrices,
    SellPrice,
};