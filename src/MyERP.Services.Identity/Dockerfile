# ============================================
# IDENTITY DOCKERFILE — Build Context: src/ (parent folder)
# Consistent with other services
# docker-compose mein: context: ./src, dockerfile: MyERP.Services.Identity/Dockerfile
# ============================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Step 1: Sirf csproj copy karo (caching ke liye)
COPY ["MyERP.Services.Identity/MyERP.Services.Identity.csproj", "MyERP.Services.Identity/"]
RUN dotnet restore "MyERP.Services.Identity/MyERP.Services.Identity.csproj"

# Step 2: Source code copy karo
COPY ["MyERP.Services.Identity/", "MyERP.Services.Identity/"]

# Publish
RUN dotnet publish "MyERP.Services.Identity/MyERP.Services.Identity.csproj" \
    -c Release -o /app/publish --no-restore

# ============================================
# STAGE 2: RUNTIME
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyERP.Services.Identity.dll"]
