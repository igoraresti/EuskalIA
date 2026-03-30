#!/bin/bash

# EuskalLingo - SQL Server Container Manager
# This script ensures the SQL Server container is up and running.

GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}== EuskalIA - SQL Server Manager ==${NC}"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}Error: Docker is not running. Please start Docker and try again.${NC}"
    exit 1
fi

# Move to the server directory if needed
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
cd "$DIR"

CONTAINER_NAME="euskalia-sqlserver"

# Check if container exists
if [ "$(docker ps -aq -f name=$CONTAINER_NAME)" ]; then
    if [ "$(docker ps -q -f name=$CONTAINER_NAME)" ]; then
        echo -e "${GREEN}Container '$CONTAINER_NAME' is already running.${NC}"
    else
        echo -e "${BLUE}Starting existing container '$CONTAINER_NAME'...${NC}"
        docker start $CONTAINER_NAME
    fi
else
    echo -e "${BLUE}Creating and starting new SQL Server container...${NC}"
    docker-compose up -d
fi

# Wait for SQL Server to be ready
echo -e "${BLUE}Waiting for SQL Server to accept connections...${NC}"
LIMIT=60
COUNT=0
while [ $COUNT -lt $LIMIT ]; do
    if docker exec $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Pass123' -C -Q "SELECT 1" > /dev/null 2>&1; then
        echo -e "${GREEN}SQL Server is READY!${NC}"
        break
    fi
    echo -n "."
    sleep 2
    COUNT=$((COUNT+1))
done

if [ $COUNT -eq $LIMIT ]; then
    echo -e "${RED}\nError: SQL Server took too long to start.${NC}"
    exit 1
fi

echo -e "\n${GREEN}Database setup completed successfully.${NC}"
echo -e "Connection String: ${BLUE}Server=localhost,1433;Database=EuskalIA;User Id=sa;Password=YourStrong!Pass123;Encrypt=False${NC}"
