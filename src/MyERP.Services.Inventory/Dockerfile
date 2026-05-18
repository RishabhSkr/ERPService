# ============================================
# SALES DOCKERFILE — Build Context: src/ (parent folder)
# Kyun? Kyunki Sales ka ProjectReference hai → MyERP.Shared
# docker-compose mein: context: ./src, dockerfile: MyERP.Services.Sales/Dockerfile
# ============================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Step 1: Pehle DONO projects ke .csproj copy karo (caching ke liye)
# Shared PEHLE — kyunki Sales iske upar depend karta hai
COPY ["MyERP.Shared/MyERP.Shared.csproj", "MyERP.Shared/"]
COPY ["MyERP.Services.Inventory/MyERP.Services.Inventory.csproj", "MyERP.Services.Inventory/"]

# Restore — ab dono csproj available hain, toh reference resolve hoga!
RUN dotnet restore "MyERP.Services.Inventory/MyERP.Services.Inventory.csproj"

# Step 2: Ab DONO projects ka source code copy karo
COPY ["MyERP.Shared/", "MyERP.Shared/"]
COPY ["MyERP.Services.Inventory/", "MyERP.Services.Inventory/"]

# Publish Inventory (Shared automatically build hoga as dependency)
RUN dotnet publish "MyERP.Services.Inventory/MyERP.Services.Inventory.csproj" \
    -c Release -o /app/publish --no-restore

# ============================================
# STAGE 2: RUNTIME
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyERP.Services.Inventory.dll"]
