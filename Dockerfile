FROM mcr.microsoft.com/dotnet/sdk:8.0.101 AS build
WORKDIR /src
COPY ["MoodTracker-back.csproj", "./"]
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0.1
WORKDIR /app
COPY --from=build /app/publish .

RUN useradd -M --uid 1001 dotnetuser && \
    chown -R dotnetuser:dotnetuser /app
USER dotnetuser

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "MoodTracker-back.dll"]