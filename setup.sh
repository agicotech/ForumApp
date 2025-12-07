#!/bin/bash

# ForumApp Setup Script for Ubuntu
# This script installs all necessary dependencies and sets up the application

set -e  # Exit on error

echo "=========================================="
echo "ForumApp Setup Script for Ubuntu"
echo "=========================================="
echo ""

# Check if running as root
if [ "$EUID" -eq 0 ]; then 
    echo "Please do not run this script as root"
    exit 1
fi

# Update package list
echo "Updating package list..."
sudo apt-get update

# Install .NET SDK 9.0
echo ""
echo "Installing .NET SDK 9.0..."
if ! command -v dotnet &> /dev/null; then
    # Add Microsoft package repository
    wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    
    sudo apt-get update
    sudo apt-get install -y dotnet-sdk-9.0
else
    echo ".NET SDK is already installed"
fi

# Verify .NET installation
dotnet --version

# Install SQLite (if not already installed)
echo ""
echo "Installing SQLite..."
sudo apt-get install -y sqlite3 libsqlite3-dev

# Install nginx (optional, for reverse proxy)
echo ""
echo "Installing nginx..."
sudo apt-get install -y nginx

# Get the current directory
APP_DIR=$(pwd)
APP_NAME="ForumApp"
SERVICE_NAME="forumapp"

echo ""
echo "Application directory: $APP_DIR"

# Restore NuGet packages
echo ""
echo "Restoring NuGet packages..."
dotnet restore

# Build the application
echo ""
echo "Building the application..."
dotnet build --configuration Release

# Create publish directory
echo ""
echo "Publishing the application..."
dotnet publish --configuration Release --output ./publish

# Create systemd service file
echo ""
echo "Creating systemd service..."

sudo tee /etc/systemd/system/${SERVICE_NAME}.service > /dev/null <<EOF
[Unit]
Description=ForumApp ASP.NET Core Web Application
After=network.target

[Service]
Type=notify
WorkingDirectory=${APP_DIR}/publish
ExecStart=/usr/bin/dotnet ${APP_DIR}/publish/${APP_NAME}.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=${SERVICE_NAME}
User=${USER}
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOF

# Reload systemd
echo ""
echo "Reloading systemd..."
sudo systemctl daemon-reload

# Enable the service
echo ""
echo "Enabling ${SERVICE_NAME} service..."
sudo systemctl enable ${SERVICE_NAME}

# Configure nginx (optional)
echo ""
read -p "Do you want to configure nginx as reverse proxy? (y/n): " configure_nginx

if [ "$configure_nginx" = "y" ] || [ "$configure_nginx" = "Y" ]; then
    read -p "Enter your domain name (or IP address): " domain_name
    
    sudo tee /etc/nginx/sites-available/${SERVICE_NAME} > /dev/null <<EOF
server {
    listen 80;
    server_name ${domain_name};

    location / {
        proxy_pass http://localhost:5238;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
EOF

    # Enable the site
    sudo ln -sf /etc/nginx/sites-available/${SERVICE_NAME} /etc/nginx/sites-enabled/
    
    # Test nginx configuration
    sudo nginx -t
    
    # Reload nginx
    sudo systemctl reload nginx
    
    echo "Nginx configured successfully!"
fi

# Configure firewall (if ufw is active)
if sudo ufw status | grep -q "Status: active"; then
    echo ""
    echo "Configuring firewall..."
    sudo ufw allow 5238/tcp
    if [ "$configure_nginx" = "y" ] || [ "$configure_nginx" = "Y" ]; then
        sudo ufw allow 'Nginx Full'
    fi
fi

echo ""
echo "=========================================="
echo "Setup completed successfully!"
echo "=========================================="
echo ""
echo "To start the application:"
echo "  sudo systemctl start ${SERVICE_NAME}"
echo ""
echo "To check status:"
echo "  sudo systemctl status ${SERVICE_NAME}"
echo ""
echo "To view logs:"
echo "  sudo journalctl -u ${SERVICE_NAME} -f"
echo ""
echo "To stop the application:"
echo "  sudo systemctl stop ${SERVICE_NAME}"
echo ""
if [ "$configure_nginx" = "y" ] || [ "$configure_nginx" = "Y" ]; then
    echo "Application will be available at: http://${domain_name}"
else
    echo "Application will be available at: http://localhost:5238"
fi
echo ""
