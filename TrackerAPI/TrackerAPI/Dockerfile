FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TrackerAPI/TrackerAPI.csproj", "TrackerAPI/"]
RUN dotnet restore "TrackerAPI/TrackerAPI.csproj"

COPY . .
WORKDIR "/src/TrackerAPI"
RUN dotnet build "TrackerAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrackerAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Punto de entrada
ENTRYPOINT ["dotnet", "TrackerAPI.dll"]