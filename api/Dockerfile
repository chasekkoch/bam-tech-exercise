# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["api/StargateAPI.csproj", "api/"]
RUN dotnet restore "api/StargateAPI.csproj"

COPY . .
WORKDIR /src/api
RUN dotnet publish "StargateAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=https://+:8443
EXPOSE 8443

ENTRYPOINT ["dotnet", "StargateAPI.dll"]
