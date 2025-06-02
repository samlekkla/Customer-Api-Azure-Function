# Customer Management System with Azure Functions & Cosmos DB

This solution contains a minimal ASP.NET Core Web API and an Azure Function that together handle customer data and responsible salespeople using Azure Cosmos DB and SendGrid.

## 🧱 Technologies Used

- ASP.NET Core (.NET 8)
- Azure Cosmos DB (SQL API)
- Azure Functions (.NET Isolated)
- SendGrid (Email Notifications)
- Swagger (OpenAPI)
- Git + GitHub

## 📦 Features

### Web API (`Customer_API`)
- Create, update, delete customers
- Each customer must have a responsible salesperson
- Search customers by name or salesperson
- Uses LINQ with Cosmos DB for safe querying
- Swagger UI for testing endpoints

### Azure Function (`CustomerNotifierFunction`)
- Listens for changes in the `Customers` container in Cosmos DB
- Sends an email to the responsible salesperson when a customer is created or updated
- Uses SendGrid for reliable delivery

## 🚀 Getting Started

### 1. Requirements
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator) (for local dev)
- [SendGrid account](https://sendgrid.com) with verified sender

### 2. Setup

#### Cosmos DB Emulator
- Start the emulator locally (usually runs on `https://localhost:8081`)

#### SendGrid
- Sign up and generate an API key
- Go to **Sender Authentication** and verify an email address (e.g. noreply@yourdomain.com)
- Use this verified address as your `from` email when sending

#### Configuration

Update your Azure Function's `local.settings.json` file:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDbConnection": "AccountEndpoint=https://localhost:8081/;AccountKey=...",
    "SendGridApiKey": "SG.your_actual_api_key"
  }
}
