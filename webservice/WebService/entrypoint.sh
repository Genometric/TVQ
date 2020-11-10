#!/bin/sh

# Expected args: 
# 1. Database endpoint in the following format: {Service Name}:{Port Number}
# 2. Maximum wait time for the database to be ready.

# Exit immediately if a command exits with a non-zero status.
set -e

DB_ENDPOINT="$1"
DB_WAIT_TIME="$2"

./wait_for_it.sh $DB_ENDPOINT -t $DB_WAIT_TIME -- dotnet Genometric.TVQ.WebService.dll
