#!/bin/bash

# Tiba Microservices Startup Script
# This script builds the .NET solution and runs all services via Docker Compose

set -e  # Exit on any error

echo "🚀 Starting Tiba Microservices..."
echo

# Function to print colored output
print_step() {
    echo -e "\033[1;34m$1\033[0m"
}

print_success() {
    echo -e "\033[1;32m$1\033[0m"
}

print_error() {
    echo -e "\033[1;31m$1\033[0m"
}

# Check if Docker is running
print_step "📋 Checking Docker status..."
if ! docker info > /dev/null 2>&1; then
    print_error "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi
print_success "✅ Docker is running"

# Check if docker-compose is available
if ! command -v docker-compose > /dev/null 2>&1; then
    print_error "❌ docker-compose is not installed. Please install docker-compose and try again."
    exit 1
fi

# Stop any existing containers
print_step "🛑 Stopping existing containers..."
docker-compose down --remove-orphans

# Clean up any existing images (optional - uncomment if you want to force rebuild)
# print_step "🗑️  Removing existing images..."
# docker-compose down --rmi all

# Build .NET solution
print_step "🔨 Building .NET solution..."
dotnet clean
dotnet restore
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    print_error "❌ .NET build failed. Please fix build errors and try again."
    exit 1
fi
print_success "✅ .NET solution built successfully"

# Build and start containers
print_step "🐳 Building Docker images and starting containers..."
docker-compose up --build -d

if [ $? -ne 0 ]; then
    print_error "❌ Docker Compose failed to start containers."
    exit 1
fi

# Wait a moment for services to start
echo
print_step "⏳ Waiting for services to start..."
sleep 5

# Check container status
print_step "📊 Checking container status..."
docker-compose ps

# Display service URLs
echo
print_success "🎉 Tiba Microservices started successfully!"
echo
echo "📋 Service URLs:"
echo "   🌐 REST API:        http://localhost:5000"
echo "   🐰 RabbitMQ UI:     http://localhost:15672 (guest/guest)"
echo "   📖 API Docs:       http://localhost:5000/swagger"
echo
echo "📝 Useful commands:"
echo "   View logs:         docker-compose logs -f"
echo "   Stop services:     docker-compose down"
echo "   Restart service:   docker-compose restart <service-name>"
echo
echo "🧪 Test the API using the app.http file or curl:"
echo "   curl -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwidXNlcklkIjoiMSIsIm5hbWUiOiJKb2huIERvZSIsImlhdCI6MTUxNjIzOTAyMn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c' http://localhost:5000/todos"
echo