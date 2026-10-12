
<h1>CoffeeNChill Azure Functions API</h1>

<p>A serverless RESTful API built with .NET 8 (Isolated Worker Process) and Azure Functions v4, integrated with Azure Table Storage, Azure Queue Storage, and Azure Blob Storage to manage coffee shop operations, including menu items, order lifecycles, and staff documents. The application is containerized with Docker and Docker Compose for standalone execution.</p>

<hr>

<h2>📌 Project Architecture Overview</h2>

<pre><code>CoffeeNChillFunctions/
├── docs/                                  # Exported Postman collection and artifacts
│   └── CoffeeNChill.postman_collection.json
├── DTOs/                                  # Data Transfer Objects with JsonPropertyName
│   ├── MenuItemDto.cs
│   └── OrderMessageDto.cs                 # Part 2: Order payload DTO with JSON attributes
├── Models/                                # Table Storage Entity classes
│   ├── MenuItemEntity.cs
│   └── OrderEntity.cs                     # Part 2: Order Table entity mapping
├── MenuFunctions.cs                       # Azure Functions: Table Storage (Menu CRUD)
├── OrderQueueFunctions.cs                 # Part 2: Table Storage & Base64 Queue Integration
├── StaffDocumentFunctions.cs              # Azure Functions: Blob Storage (Staff Docs)
├── Dockerfile                             # Multi-stage Docker build setup
├── docker-compose.yml                     # Multi-container orchestration (App + Azurite)
├── host.json                              # Azure Functions host runtime configuration
├── local.settings.json                    # Connection strings for local development
└── README.md                              # Main documentation</code></pre>

<h2>👥 Group Member Contributions</h2>

<table>
  <thead>
    <tr>
      <th>Member Name</th>
      <th>Role &amp; Responsibilities</th>
      <th>Verified Commits</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><strong>Olwethu Zondi</strong></td>
      <td>Backend architecture, Azure Table Storage and Base64 Queue implementation (<code>OrderQueueFunctions.cs</code>), case-sensitive DTO serialization, Dockerfile and Docker Compose configuration, Postman collection structure and test automation.</td>
      <td>5+ commits</td>
    </tr>
    <tr>
      <td><strong>Aphiwe Ndlovu</strong></td>
      <td>Azure Blob Storage implementation (<code>StaffDocumentFunctions.cs</code>), Docker containerization testing, Postman test verification, repository documentation and final commits.</td>
      <td>5+ commits</td>
    </tr>
  </tbody>
</table>

<h2>🎥 Demonstration Video</h2>

<ul>
  <li><strong>YouTube Video:</strong> <a href="https://https://youtu.be/JRBGDOSC4jA?si=yoLj0CWVoULwKA56">Watch the CoffeeNChill API Demonstration</a></li>
</ul>

<p><strong>The demonstration video covers:</strong></p>

<ul>
  <li>Running Azurite locally and executing multi-container setups using Docker Compose.</li>
  <li>Executing HTTP requests using Postman against local Azure Function triggers.</li>
  <li>A step-by-step walkthrough of Table Storage (Menu Items), Queue and Table Order Processing (<code>v2.0</code>), and Blob Storage (Staff Documents) APIs.</li>
</ul>

<h2>💻 Local Setup &amp; Development Instructions</h2>

<h3>Prerequisites</h3>

<ul>
  <li>.NET 8.0 SDK</li>
  <li>Azure Functions Core Tools v4</li>
  <li>Azurite Storage Emulator (via VS Code extension, npm, or Docker)</li>
  <li>Docker Desktop</li>
  <li>Postman</li>
</ul>

<h3>Step 1: Clone the Repository</h3>

<p>Replace <code>YOUR_GITHUB_USERNAME</code> with the appropriate GitHub username if necessary.</p>

<pre><code>git clone https://github.com/YOUR_GITHUB_USERNAME/CoffeeNChill-CLDV6212.git
cd CoffeeNChill-CLDV6212/CoffeeNChillFunctions</code></pre>

<h3>Step 2: Configure Environment Variables</h3>

<p>Ensure your <code>local.settings.json</code> file contains the following Azurite connection settings for local development:</p>

<pre><code>{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}</code></pre>

<p><strong>Note:</strong> Do not commit real credentials, secrets, or production connection strings to GitHub.</p>

<h3>Step 3: Run Using Docker Compose (Recommended)</h3>

<p>Use Docker Compose to start both the Azurite storage emulator and the Azure Functions application:</p>

<pre><code>docker-compose up --build</code></pre>

<p>The API endpoints will be accessible at:</p>

<p><code>http://localhost:7111/api/</code></p>

<h2>🐳 Docker Execution Commands (Standalone &amp; Docker Hub)</h2>

<h3>Option A: Build and Run the Container Locally (<code>v2.0</code>)</h3>

<p><strong>Build the Docker image:</strong></p>

<pre><code>docker build -t zondirite/coffeenchill-functions:v2.0 .</code></pre>

<p><strong>Run the container:</strong></p>

<pre><code>docker run -d -p 7111:8080 --name coffeenchill-app zondirite/coffeenchill-functions:v2.0</code></pre>

<p><strong>Verify running containers:</strong></p>

<pre><code>docker ps</code></pre>

<h3>Option B: Pull the Tagged Image from Docker Hub (<code>v2.0</code>)</h3>

<p><strong>Pull and run the Azure Functions image:</strong></p>

<pre><code>docker pull zondirite/coffeenchill-functions:v2.0
docker run -d -p 7111:8080 --name coffeenchill-app zondirite/coffeenchill-functions:v2.0</code></pre>

<p><strong>Note:</strong> The standalone container commands assume the image is configured to listen on port <code>8080</code>. Use Docker Compose when the application requires Azurite to be running alongside it.</p>

<h2>🧪 API Endpoints &amp; Postman Testing</h2>

<p>All endpoints are organized in <code>/docs/CoffeeNChill.postman_collection.json</code> under <strong>Part 1</strong> and <strong>Part 2 - Table &amp; Queue</strong>.</p>

<h3>1. Menu Items API (Azure Table Storage)</h3>

<table>
  <thead>
    <tr>
      <th>Method</th>
      <th>Endpoint</th>
      <th>Description</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><code>POST</code></td>
      <td><code>/api/menu</code></td>
      <td>Create a new menu item.</td>
    </tr>
    <tr>
      <td><code>GET</code></td>
      <td><code>/api/menu</code></td>
      <td>Retrieve all menu items.</td>
    </tr>
    <tr>
      <td><code>GET</code></td>
      <td><code>/api/menu/category/{category}</code></td>
      <td>Retrieve menu items by category.</td>
    </tr>
    <tr>
      <td><code>PUT</code></td>
      <td><code>/api/menu/{category}/{sku}</code></td>
      <td>Update a menu item's price or availability.</td>
    </tr>
    <tr>
      <td><code>DELETE</code></td>
      <td><code>/api/menu/{category}/{sku}</code></td>
      <td>Delete a menu item.</td>
    </tr>
  </tbody>
</table>

<h3>2. Orders, Table &amp; Queue API (Part 2)</h3>

<table>
  <thead>
    <tr>
      <th>Method</th>
      <th>Endpoint</th>
      <th>Description</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><code>POST</code></td>
      <td><code>/api/orders</code></td>
      <td>Accept an order payload, write it to <code>CoffeeOrders</code> Table Storage, and send a Base64-encoded message to <code>order-queue</code>.</td>
    </tr>
    <tr>
      <td><code>PUT</code></td>
      <td><code>/api/orders/{orderId}/status</code></td>
      <td>Update an order's fulfillment status in Table Storage using an idempotent point lookup.</td>
    </tr>
  </tbody>
</table>

<h3>3. Staff Documents API (Azure Blob Storage)</h3>

<table>
  <thead>
    <tr>
      <th>Method</th>
      <th>Endpoint</th>
      <th>Description</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><code>POST</code></td>
      <td><code>/api/documents/upload?fileName={fileName}</code></td>
      <td>Upload a document stream.</td>
    </tr>
    <tr>
      <td><code>GET</code></td>
      <td><code>/api/documents</code></td>
      <td>List all uploaded staff documents.</td>
    </tr>
    <tr>
      <td><code>DELETE</code></td>
      <td><code>/api/documents/{fileName}</code></td>
      <td>Delete a staff document.</td>
    </tr>
  </tbody>
</table>

<h2>🖼️ Visual Evidence &amp; Verification Screenshots</h2>

<p>The following screenshots document the implementation and testing process:</p>

<ul>
  <li><code>01_Postman_POST_Order_Passing.png</code> – Postman execution of <code>POST /api/orders</code>, returning <code>200 OK</code> with passing test assertions.</li>
  <li><code>02_Postman_PUT_Status_Passing.png</code> – Postman execution of <code>PUT /api/orders/{orderId}/status</code>, successfully updating the order status.</li>
  <li><code>03_Docker_v2_Build_And_Push.png</code> – Successful build and push of <code>zondirite/coffeenchill-functions:v2.0</code> to Docker Hub.</li>
  <li><code>04_Docker_Compose_Running.png</code> – Successful execution of <code>docker-compose up</code>, verifying that the application and Azurite containers run concurrently.</li>
</ul>

<h2>📜 Repository Guidelines &amp; Checklist Compliance</h2>

<ul>
  <li>[x] Minimum of five meaningful commits per group member.</li>
  <li>[x] Exported Postman collection with Part 2 folders saved in <code>/docs</code>.</li>
  <li>[x] Dockerfile verified using a multi-stage build pattern and tagged as <code>v2.0</code>.</li>
  <li>[x] Local environment configured to run on port <code>7111</code> with Azurite emulation.</li>
</ul>

<h2>📚 References</h2>

<ol>
  <li>
    Microsoft Corporation. (2024).
    <em>Azure Functions .NET isolated process guide.</em>
    Microsoft Learn.
    Available at:
    <a href="https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide">https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide</a>
    (Accessed 12 October 2026).
  </li>
  <li>
    Microsoft Corporation. (2024).
    <em>Get started with Azure Table storage and the Azure Storage Table SDK for .NET.</em>
    Microsoft Learn.
    Available at:
    <a href="https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-quickstart-dotnet">https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-quickstart-dotnet</a>
    (Accessed 12 October 2026).
  </li>
  <li>
    Microsoft Corporation. (2024).
    <em>How to use Azure Queue storage from .NET.</em>
    Microsoft Learn.
    Available at:
    <a href="https://learn.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues">https://learn.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues</a>
    (Accessed 12 October 2026).
  </li>
  <li>
    Docker Inc. (2024).
    <em>Docker Compose Overview.</em>
    Docker Documentation.
    Available at:
    <a href="https://docs.docker.com/compose/">https://docs.docker.com/compose/</a>
    (Accessed 12 October 2026).
  </li>
  <li>
    Postman Inc. (2024).
    <em>Writing tests in Postman.</em>
    Postman Learning Center.
    Available at:
    <a href="https://learning.postman.com/docs/writing-scripts/test-scripts/">https://learning.postman.com/docs/writing-scripts/test-scripts/</a>
    (Accessed 12 October 2026).
  </li>
</ol>
