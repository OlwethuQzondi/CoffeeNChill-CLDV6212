CoffeeNChill Azure Functions APIA serverless RESTful API built with .NET 8 (Isolated Worker Process) and Azure Functions v4, integrated with Azure Storage (Azurite) for managing coffee shop operations including Menu Items (Table Storage) and Staff Documents (Blob Storage). Containerized with Docker for standalone execution.📌 Project Architecture OverviewCoffeeNChillFunctions/

├── docs/                                  # Exported Postman collection and artifacts
│   └── CoffeeNChill.postman_collection.json
├── DTOs/                                  # Data Transfer Objects with JsonPropertyName
│   └── MenuItemDto.cs
├── Models/                                # Table Storage Entity classes
│   └── MenuItemEntity.cs
├── MenuFunctions.cs                       # Azure Functions: Table Storage (Menu CRUD)
├── StaffDocumentFunctions.cs              # Azure Functions: Blob Storage (Staff Docs)
├── Dockerfile                             # Multi-stage Docker build setup
├── host.json                              # Azure Functions host runtime configuration
├── local.settings.json                    # Connection strings for local development
└── README.md                              # Main documentation

👥 Group Member ContributionsMember NameRole & ResponsibilitiesVerified CommitsOlwethu ZondiBackend Architecture, Azure Table Storage implementation (MenuFunctions.cs), DTO camelCase serialization setup, Dockerfile creation, Postman Collection setup & runner execution.5+ CommitsAphiwe NdlovuAzure Blob Storage implementation (StaffDocumentFunctions.cs), Docker containerization testing, Postman test verification, Demonstration Video production.5+ Commits🎥 Demonstration VideoYouTube Video Link: [https://youtu.be/CxVYQOPW9Ms?si=7fD0Cy85pcETkDFZ]The demonstration video covers:Running Azurite locally and executing standalone Docker container commands.Executing HTTP requests using Postman against local Azure Function triggers.Step-by-step walk-through of Table Storage (Menu Items) and Blob Storage (Staff Documents) APIs.

💻 Local Setup & Development InstructionsPrerequisites.NET 8.0 SDKAzure Functions Core Tools v4Azurite Storage Emulator (via VS Code Extension, npm, or Docker)Docker DesktopPostman

Step 1: Clone the Repositorygit clone https://github.com/YOUR_GITHUB_USERNAME/CoffeeNChill-CLDV6212.git
cd CoffeeNChill-CLDV6212/CoffeeNChillFunctions

Step 2: Configure Environment VariablesEnsure your local.settings.json file contains the Azurite connection string:{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
    }
}
Step 3: Start Azurite Storage Emulatorazurite --silent --location c:\azurite --debug c:\azurite\debug.log
Step 4: Run the Function App Locallydotnet build
func start --port 7111

The API endpoints will be accessible at: http://localhost:7111/api/🐳 Docker Execution Commands (Standalone)Option A: Build and Run Container LocallyBuild the Docker Image:docker build -t coffeenchill-functions:v1.0 .
Run the Container:docker run -d -p 7111:8080 --name coffeenchill-app coffeenchill-functions:v1.0
Verify Running Containers:docker ps

Option B: Pull Tagged Images from Docker HubAzure Functions Image:docker pull YOUR_DOCKERHUB_USERNAME/coffeenchill-functions:v1.0
docker run -d -p 7111:8080 YOUR_DOCKERHUB_USERNAME/coffeenchill-functions:v1.0
Azurite Storage Image:docker pull YOUR_DOCKERHUB_USERNAME/coffeenchill-azurite:v1.0
docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 YOUR_DOCKERHUB_USERNAME/coffeenchill-azurite:v1.0

🧪 API Endpoints & Postman Testing
All endpoints are exported in /docs/CoffeeNChill.postman_collection.json.1. Menu Items API (Azure Table Storage)POST /api/menu - Create a new menu itemGET /api/menu - Retrieve all menu itemsGET /api/menu/category/{category} - Retrieve items by categoryPUT /api/menu/{category}/{sku} - Update menu item price or availabilityDELETE /api/menu/{category}/{sku} - Delete a menu item2. Staff Documents API (Azure Blob Storage)POST /api/documents/upload?fileName={fileName} - Upload document streamGET /api/documents - List all uploaded staff documentsDELETE /api/documents/{fileName} - Delete a staff document

🖼️ Visual Evidence & Verification ScreenshotsDocker Build & Running Container (docker ps)Azure Table Storage - Profile/Menu API ExecutionAzure Blob Storage - Upload Staff Document (POST /api/documents/upload)Azure Blob Storage - List Documents (GET /api/documents)

📜 Repository Guidelines & Checklist Compliance
[x] Minimum 5 meaningful commits per group member.
[x] Exported Postman Collection saved in /docs directory.
[x] Dockerfile verified using multi-stage build pattern.
[x] Local environment running on port 7111 with Azurite emulators.
