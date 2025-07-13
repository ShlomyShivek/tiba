#!/bin/bash

# Tiba Microservices Stop Script
# This script stops all Docker containers and optionally cleans up

set -e  # Exit on any error

echo "🛑 Stopping Tiba Microservices..."
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

# Parse command line arguments
CLEAN_UP=false
while [[ $# -gt 0 ]]; do
    case $1 in
        --clean|-c)
            CLEAN_UP=true
            shift
            ;;
        --help|-h)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  --clean, -c    Remove containers, networks, and images"
            echo "  --help, -h     Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Stop containers
print_step "🛑 Stopping containers..."
docker-compose down

if [ "$CLEAN_UP" = true ]; then
    print_step "🗑️  Cleaning up containers, networks, and images..."
    docker-compose down --rmi all --volumes --remove-orphans
    
    # Clean up any dangling images
    print_step "🧹 Removing dangling Docker images..."
    docker image prune -f
    
    print_success "✅ Cleanup completed"
else
    print_success "✅ Containers stopped"
fi

echo
print_step "📊 Current Docker status:"
docker-compose ps

echo
print_success "🎉 Tiba Microservices stopped successfully!"
echo
echo "📝 To restart services:"
echo "   ./start.sh"
echo