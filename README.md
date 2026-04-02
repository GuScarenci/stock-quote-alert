# Stock Quote Alert

A .NET console application for real-time monitoring of financial assets. The system alerts investors via email whenever an asset's price crosses pre-defined buy or sell thresholds.

## Project Highlights

- **State Machine Logic:** Implementation of a custom state machine to prevent email spam. The system only triggers notifications upon state transitions (e.g., when the price enters or leaves an alert zone).
- **Multi-API Support:** Ready-to-use integration for both **Brapi** and **Yahoo Finance**, enabling the monitoring of B3 (Brazilian) assets as well as international markets.
- **Simulation Mode (Mock):** Allows full testing of alert logic and email dispatching without depending on external market quotes or API keys.
- **Clean Architecture:** Modularized code with a clear separation of concerns (Services, Models, and Enums).

## Tech Stack

- **.NET 10.0**
- **MailKit & MimeKit** (Reliable SMTP communication)
- **YahooFinanceApi** (Global market integration)
- **Microsoft.Extensions.Configuration** (JSON-based settings management)

## Prerequisites

- **.NET SDK 10.0** installed.
- A Google **App Password** (if using Gmail) for sending emails.
- A **Brapi Token** (if choosing the Brapi API provider).

## Configuration

1. Rename the `appsettings.example.json` file to `appsettings.json`.
2. Fill in your credentials as shown in the example below:

```json
{
  "Settings": {
    "StockApiProvider": "MockData",
    "TargetEmail": "destination@email.com",
    "BrapiToken": "YOUR_TOKEN_HERE",
    "Smtp": {
      "Server": "smtp.gmail.com",
      "Port": 587,
      "User": "your-email@gmail.com",
      "Pass": "your-app-password"
    }
  }
}
```
## How to Run

Open your terminal in the project's root folder and execute:

```
dotnet run -- <SYMBOL> <SELL_PRICE> <BUY_PRICE>
```

Example:
```
dotnet run -- PETR4.SA 22.67 22.59
```

**Note on Symbols:**

For Yahoo Finance, use the .SA suffix for Brazilian stocks (e.g., VALE3.SA).

For Brapi, use the standard ticker code (e.g., PETR4).

## Data Source Selection
In the appsettings.json file, you can change the StockApiProvider variable to switch between modes:

- MockData: Uses pre-defined prices for logic testing.

- Yahoo: Queries the global market (no token required).

- Brapi: Queries via the Brapi API (requires a valid token).

Developed by Gustavo Moura Scarenci de Carvalho Ferreira - Computer Engineering Student @ USP São Carlos.