using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

// Carregar Configurações
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Validar Argumentos via linha de comando
if (args.Length < 3) {
    Console.WriteLine("Uso: stock-quote-alert PETR4 22.67 22.59");
    return;
}

// Lendo o Token de forma segura
string token = config["Settings:BrapiToken"] ?? throw new Exception("BrapiToken não configurado no appsettings.json.");

var alert = new AlertConfig(args[0], decimal.Parse(args[1]), decimal.Parse(args[2]));
var emailService = new EmailService(config);
using var httpClient = new HttpClient();

Console.WriteLine($"Monitorando {alert.Symbol}... (Ctrl+C para sair)");

// Loop de Monitoramento
while (true) {
    try {
        var url = $"https://brapi.dev/api/quote/{alert.Symbol}?token={token}";
        var response = await httpClient.GetFromJsonAsync<BrapiResponse>(url);

        // Verificação de segurança (evita NullReferenceException)
        if (response != null && response.Results != null && response.Results.Count > 0) {
            var currentPrice = response.Results[0].RegularMarketPrice;

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {alert.Symbol}: R$ {currentPrice}");

            // Lógica de Alerta de compra ou venda
            if (currentPrice > alert.SellPrice) {
                Console.WriteLine($"O preço está acima e atingiu R$ {currentPrice}");
                // emailService.SendAlert($"Venda {alert.Symbol}", $"O preço atingiu R$ {currentPrice}. Sugestão de venda acima de R$ {alert.SellPrice}.");
                Console.WriteLine("Alerta de VENDA enviado!");
            } 
            else if (currentPrice < alert.BuyPrice) {
                Console.WriteLine($"O preço está abaixo e atingiu R$ {currentPrice}");
                // emailService.SendAlert($"Compra {alert.Symbol}", $"O preço caiu para R$ {currentPrice}. Sugestão de compra abaixo de R$ {alert.BuyPrice}.");
                Console.WriteLine("Alerta de COMPRA enviado!");
            }
        }
    }
    catch (Exception ex) {
        Console.WriteLine($"Erro: {ex.Message}");
    }

    await Task.Delay(TimeSpan.FromMinutes(2));
}