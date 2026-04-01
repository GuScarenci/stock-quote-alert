public class BrapiResponse {
    public List<StockResult>? Results { get; set; }
}

public class StockResult {
    public string Symbol { get; set; } = string.Empty;
    public decimal RegularMarketPrice { get; set; }
}

public record AlertConfig(string Symbol, decimal SellPrice, decimal BuyPrice);