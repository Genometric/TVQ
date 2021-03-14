# This docker file first creates a container to build the
# project, and then copies the built data to a runtime
# container (which does not have .NET SDK, hence creating
# a smaller image.)

# -----------------------
# | Building the Webapp |
# -----------------------

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY docker-compose* .
COPY webservice/WebService/*.csproj ./webservice/WebService/

# Two options to restore the project:
# Option 1: restore only the webservice project.
RUN dotnet restore webservice/WebService/WebService.csproj

# Option 2: restore the entire solutions. In this case,
# every project should be copied to the container for the
# solution to restore correctly; like:
#   COPY webservice/UnitTests/*.csproj ./webservice/UnitTests/
#   RUN dotnet restore TVQ.sln

# copy everything else and build app
COPY webservice/WebService/. ./webservice/WebService/
WORKDIR /app/webservice/WebService
RUN dotnet publish WebService.csproj -c Release -o out

# --------------------------------
# | Build the Webapp for runtime |
# --------------------------------

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime

# The following commands are required because of breaking
# changes introduced in .NET Core 3 and SQL docker container.
RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Bash is needed for the wait_for_it script.
RUN apk update
RUN apk upgrade
RUN apk add --no-cache bash

WORKDIR /app
COPY --from=build /app/webservice/WebService/out ./
COPY --from=build /app/webservice/WebService/entrypoint.sh ./
COPY --from=build /app/webservice/WebService/wait_for_it.sh ./

RUN chmod +x wait_for_it.sh
RUN chmod +x entrypoint.sh
