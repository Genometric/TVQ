FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["TVQ.API/TVQ.API.csproj", "TVQ.API/"]
RUN dotnet restore "TVQ.API/TVQ.API.csproj"
COPY . .
WORKDIR "/src/TVQ"
RUN dotnet build "TVQ.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "TVQ.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TVQ.API.dll"]
