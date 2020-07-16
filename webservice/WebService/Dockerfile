# This docker file first creates a container to build the
# project, and then copies the built data to a runtime 
# container (which does not have .NET SDK). 

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY docker-compose* .
COPY webservice/WebService/*.csproj ./webservice/WebService/
COPY webservice/UnitTests/*.csproj ./webservice/UnitTests/
RUN dotnet restore TVQ.sln

# copy everything else and build app
COPY webservice/WebService/. ./webservice/WebService/
WORKDIR /app/webservice/WebService
RUN dotnet publish WebService.csproj -c Release -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS runtime

# The following commands are required because of breaking 
# changes introduced in .NET Core 3 and SQL docker container. 
RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app
COPY --from=build /app/webservice/WebService/out ./
ENTRYPOINT ["dotnet", "Genometric.TVQ.WebService.dll"]
