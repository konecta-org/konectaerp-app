# NOTE TEMPALATE: Adjust the script as needed for your specific environment setup

# setup-dev-env.sh
#!/bin/bash
echo "Setting up KONECTAERP development environment..."

# Check prerequisites
dotnet --version
node --version
docker --version

# Setup local database
docker-compose up -d

# Install dependencies
cd src/frontend && npm install
cd ../backend && dotnet restore

echo "Dev environment ready!"