# This docker file first creates a container to build the
# project, and then copies the built data to a runtime 
# container (which does not have .NET SDK). 

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY docker-compose* .
COPY webapp/TVQ/*.csproj ./webapp/TVQ/
COPY webapp/UnitTests/*.csproj ./webapp/UnitTests/
RUN dotnet restore TVQ.sln

# copy everything else and build app
COPY webapp/TVQ/. ./webapp/TVQ/
WORKDIR /app/webapp/TVQ
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS runtime

# The following commands are required because of breaking 
# changes introduced in .NET Core 3 and SQL docker container. 
RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app
COPY --from=build /app/webapp/TVQ/out ./
ENTRYPOINT ["dotnet", "Genometric.TVQ.API.dll"]
