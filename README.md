# SKF Product Assistant Mini

## Overview
The SKF Product Assistant Mini is an Azure Function-based application that allows users to query product attributes from SKF datasheets using natural language. It integrates with Azure OpenAI for natural language processing and Application Insights for monitoring and logging.

## Features
- **Natural Language Query**: Users can ask questions like "What is the width of 6205?".
- **Datasheet Lookup**: Reads product attributes from JSON files in the `datasheets` folder.
- **Azure OpenAI Integration**: Extracts product names and attributes from user queries.
- **Application Insights**: Tracks events, logs, and exceptions for monitoring.
- **Error Handling**: Gracefully handles invalid inputs and missing data.

## Folder Structure
```
SKFProductAssistant/
│
├── src/                       # Source code for the Azure Function
│   ├── AzureFunctions/        # Azure Function entry points
│   ├── Services/              # Business logic and integrations
│   ├── Models/                # Data models
│   ├── Utils/                 # Utility classes
│   ├── appsettings.json       # Configuration file
├── datasheets/                # Datasheets (JSON files)
├── tests/                     # Unit and integration tests
├── README.md                  # Project documentation
└── SKFProductAssistant.sln    # Solution file
```

## Prerequisites
- .NET 6.0 SDK or later
- Azure Function Core Tools
- Azure OpenAI Service
- Application Insights

## Setup
1. Clone the repository:
   ```powershell
   git clone <repository-url>
   cd SKFProductAssistant
   ```

2. Add your configuration:
   - Update `src/appsettings.json` with your Application Insights instrumentation key:
     ```json
     {
       "ApplicationInsights": {
         "InstrumentationKey": "<Your-Instrumentation-Key>"
       }
     }
     ```

3. Place the JSON datasheets in the `datasheets` folder.

4. Restore dependencies:
   ```powershell
   cd src
   dotnet restore
   ```

## Running Locally
1. Start the Azure Function locally:
   ```powershell
   func start
   ```

2. Use a tool like Postman or cURL to send a POST request to the function:
   ```powershell
   curl -X POST http://localhost:7071/api/HandleQuery -H "Content-Type: application/json" -d "{\"query\": \"What is the width of 6205?\"}"
   ```

## How to Run the Azure Function for Testing

1. **Ensure Prerequisites are Installed**:
   - .NET 6.0 SDK or later
   - Azure Functions Core Tools

2. **Navigate to the Project Directory**:
   ```powershell
   cd src
   ```

3. **Start the Azure Function**:
   ```powershell
   func start
   ```

4. **Test the Function**:
   - Use tools like Postman or curl to send HTTP requests to the function's endpoint.
   - Example endpoint: `http://localhost:7071/api/HandleQuery`

5. **Monitor Logs**:
   - Check the terminal for logs to verify the function's execution.

6. **Stop the Function**:
   - Press `Ctrl+C` in the terminal to stop the Azure Functions runtime.

## Deployment
### Deploying to Azure
1. Login to Azure:
   ```powershell
   az login
   ```

2. Create a new Azure Function App (if not already created):
   ```powershell
   az functionapp create --resource-group <Resource-Group-Name> --consumption-plan-location <Location> --runtime dotnet --functions-version 4 --name <Function-App-Name> --storage-account <Storage-Account-Name>
   ```

3. Publish the Azure Function:
   ```powershell
   func azure functionapp publish <Function-App-Name>
   ```

4. Verify the deployment by sending a request to the Azure Function URL:
   ```powershell
   curl -X POST https://<Function-App-Name>.azurewebsites.net/api/HandleQuery -H "Content-Type: application/json" -d "{\"query\": \"What is the width of 6205?\"}"
   ```

## Testing
- Unit tests are located in the `tests` folder.
- Run tests using:
  ```powershell
  dotnet test SKFProductAssistant.sln
  ```

## Notes
- Ensure the datasheets are placed in the `datasheets` folder.
- Use environment variables or `appsettings.json` for sensitive data.


