# Build script for Contact Messages Dashboard
Write-Host "Building Contact Messages Dashboard for Umbraco 16..." -ForegroundColor Green

# Check if Node.js is installed
if (!(Get-Command "npm" -ErrorAction SilentlyContinue)) {
    Write-Host "Error: npm is not installed. Please install Node.js first." -ForegroundColor Red
    exit 1
}

# Install dependencies
Write-Host "Installing dependencies..." -ForegroundColor Yellow
npm install

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to install dependencies." -ForegroundColor Red
    exit 1
}

# Build the project
Write-Host "Building TypeScript component..." -ForegroundColor Yellow
npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed." -ForegroundColor Red
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "The dashboard component is now ready for Umbraco 16." -ForegroundColor Green
Write-Host "Please restart your Umbraco application to see the changes." -ForegroundColor Yellow
