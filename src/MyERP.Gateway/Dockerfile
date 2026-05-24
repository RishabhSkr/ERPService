# ============================================
# GATEWAY DOCKERFILE — Build Context: src/ (parent folder)
# Kyun? Gateway ka ProjectReference hai → MyERP.Services.Identity
# Gateway Identity ka Middleware + Repository directly use karta hai
# docker-compose mein: context: ./src, dockerfile: MyERP.Gateway/Dockerfile
# ============================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Step 1: DONO projects ke .csproj copy karo
# Identity PEHLE — kyunki Gateway iske upar depend karta hai
COPY ["MyERP.Services.Identity/MyERP.Services.Identity.csproj", "MyERP.Services.Identity/"]
COPY ["MyERP.Gateway/MyERP.Gateway.csproj", "MyERP.Gateway/"]

# Restore — Identity ka csproj available hai toh reference resolve hoga
RUN dotnet restore "MyERP.Gateway/MyERP.Gateway.csproj"

# Step 2: DONO projects ka source code copy karo
COPY ["MyERP.Services.Identity/", "MyERP.Services.Identity/"]
COPY ["MyERP.Gateway/", "MyERP.Gateway/"]

# Publish Gateway
# -p:ErrorOnDuplicatePublishOutputFiles=false kyunki Gateway aur Identity
# DONO mein appsettings.json hai — conflict resolve karne ke liye
RUN dotnet publish "MyERP.Gateway/MyERP.Gateway.csproj" \
    -c Release -o /app/publish --no-restore \
    -p:ErrorOnDuplicatePublishOutputFiles=false

# ============================================
# STAGE 2: RUNTIME
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyERP.Gateway.dll"]
