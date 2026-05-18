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
COPY ["MyERP.Services.Sales/MyERP.Services.Sales.csproj", "MyERP.Services.Sales/"]

# Restore — ab dono csproj available hain, toh reference resolve hoga!
RUN dotnet restore "MyERP.Services.Sales/MyERP.Services.Sales.csproj"

# Step 2: Ab DONO projects ka source code copy karo
COPY ["MyERP.Shared/", "MyERP.Shared/"]
COPY ["MyERP.Services.Sales/", "MyERP.Services.Sales/"]

# Publish Sales (Shared automatically build hoga as dependency)
RUN dotnet publish "MyERP.Services.Sales/MyERP.Services.Sales.csproj" \
    -c Release -o /app/publish --no-restore

# ============================================
# STAGE 2: RUNTIME
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyERP.Services.Sales.dll"]