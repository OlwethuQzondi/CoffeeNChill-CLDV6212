# Stage 1: Base image for running .NET 8 Azure Functions
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

# Stage 2: SDK image for compiling the C# project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CoffeeNChillFunctions/CoffeeNChillFunctions.csproj", "CoffeeNChillFunctions/"]
RUN dotnet restore "CoffeeNChillFunctions/CoffeeNChillFunctions.csproj"
COPY . .
WORKDIR "/src/CoffeeNChillFunctions"
RUN dotnet build "CoffeeNChillFunctions.csproj" -c Release -o /app/build

# Stage 3: Publish compiled assets
FROM build AS publish
RUN dotnet publish "CoffeeNChillFunctions.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final stage running the functions app
FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true