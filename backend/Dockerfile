FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Domain/EnviroWatch.Domain.csproj", "src/Domain/"]
COPY ["src/Application/EnviroWatch.Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/EnviroWatch.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/API/EnviroWatch.API.csproj", "src/API/"]

RUN dotnet restore "src/API/EnviroWatch.API.csproj"

COPY . .
RUN dotnet publish "src/API/EnviroWatch.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EnviroWatch.API.dll"]
