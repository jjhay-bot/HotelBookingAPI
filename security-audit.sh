#!/bin/bash

# Security Audit Script for .NET Projects
# This script performs various security checks on your .NET project

echo "🔐 .NET Project Security Audit"
echo "=============================="
echo ""

PROJECT_DIR=$(pwd)
echo "📂 Auditing project: $PROJECT_DIR"
echo ""

# Check if this is a .NET project
if ! ls *.csproj 1> /dev/null 2>&1 && ! ls *.sln 1> /dev/null 2>&1; then
    echo "❌ No .NET project files found in current directory"
    exit 1
fi

echo "1️⃣ Checking for vulnerable packages..."
echo "------------------------------------"
dotnet list package --vulnerable
if [ $? -eq 0 ]; then
    echo "✅ Vulnerability check completed"
else
    echo "⚠️  Could not check for vulnerabilities"
fi
echo ""

echo "2️⃣ Checking for deprecated packages..."
echo "-------------------------------------"
dotnet list package --deprecated
if [ $? -eq 0 ]; then
    echo "✅ Deprecated package check completed"
else
    echo "⚠️  Could not check for deprecated packages"
fi
echo ""

echo "3️⃣ Checking for outdated packages..."
echo "-----------------------------------"
dotnet list package --outdated
if [ $? -eq 0 ]; then
    echo "✅ Outdated package check completed"
else
    echo "⚠️  Could not check for outdated packages"
fi
echo ""

echo "4️⃣ Checking project file security settings..."
echo "--------------------------------------------"

# Check for floating version references
echo "🔍 Checking for floating version references..."
grep -r "Version=\"\*\"" *.csproj 2>/dev/null && echo "❌ Found floating version references (*)" || echo "✅ No floating version references found"
grep -r "Version=\".*\.\*\"" *.csproj 2>/dev/null && echo "❌ Found partial floating version references" || echo "✅ No partial floating version references found"
echo ""

# Check for security analysis settings
echo "🔍 Checking for security analysis settings..."
grep -r "EnableNETAnalyzers" *.csproj 2>/dev/null && echo "✅ Found EnableNETAnalyzers setting" || echo "⚠️  EnableNETAnalyzers not found - consider adding for security analysis"
grep -r "TreatWarningsAsErrors" *.csproj 2>/dev/null && echo "✅ Found TreatWarningsAsErrors setting" || echo "⚠️  TreatWarningsAsErrors not found - consider adding"
echo ""

echo "5️⃣ Checking for sensitive information in config files..."
echo "-------------------------------------------------------"

# Check for common sensitive patterns
echo "🔍 Checking for potential secrets..."

# Check for hardcoded passwords
grep -ri "password.*=" appsettings*.json 2>/dev/null && echo "⚠️  Found potential hardcoded passwords in config" || echo "✅ No hardcoded passwords found in config"

# Check for API keys
grep -ri "apikey\|api_key\|secret" appsettings*.json 2>/dev/null && echo "⚠️  Found potential API keys/secrets in config" || echo "✅ No obvious API keys found in config"

# Check for connection strings with passwords
grep -ri "password.*=" appsettings*.json 2>/dev/null && echo "⚠️  Found connection strings with passwords in config" || echo "✅ No connection string passwords found in config"
echo ""

echo "6️⃣ Checking NuGet configuration security..."
echo "------------------------------------------"

if [ -f "NuGet.Config" ]; then
    echo "🔍 Found NuGet.Config, checking settings..."
    
    # Check for HTTPS package sources
    grep -i "http://" NuGet.Config 2>/dev/null && echo "❌ Found HTTP package sources (insecure)" || echo "✅ No HTTP package sources found"
    
    # Check for signature validation
    grep -i "signatureValidationMode" NuGet.Config 2>/dev/null && echo "✅ Found signature validation settings" || echo "⚠️  No signature validation configured"
    
else
    echo "ℹ️  No NuGet.Config found - using default settings"
fi
echo ""

echo "7️⃣ Checking for packages.lock.json..."
echo "------------------------------------"
if [ -f "packages.lock.json" ]; then
    echo "✅ Found packages.lock.json - dependency versions are locked"
else
    echo "⚠️  No packages.lock.json found - consider enabling RestorePackagesWithLockFile"
fi
echo ""

echo "8️⃣ Checking SSL/TLS configuration..."
echo "-----------------------------------"
grep -ri "RequireHttpsMetadata.*false" . 2>/dev/null && echo "❌ Found RequireHttpsMetadata set to false" || echo "✅ No insecure HTTPS metadata settings found"
grep -ri "UseHttpsRedirection" Program.cs 2>/dev/null && echo "✅ Found HTTPS redirection" || echo "⚠️  HTTPS redirection not found in Program.cs"
echo ""

echo "9️⃣ Security recommendations..."
echo "-----------------------------"

echo "📋 Security Checklist:"
echo "[ ] All packages are using specific versions (no * or floating versions)"
echo "[ ] EnableNETAnalyzers is set to true"
echo "[ ] TreatWarningsAsErrors is set to true"
echo "[ ] RestorePackagesWithLockFile is enabled"
echo "[ ] signatureValidationMode is set to 'require' in NuGet.Config"
echo "[ ] All package sources use HTTPS"
echo "[ ] No secrets or passwords in appsettings.json"
echo "[ ] HTTPS redirection is enabled"
echo "[ ] Security middleware is implemented"
echo "[ ] Input validation is in place"
echo "[ ] Authentication and authorization are properly configured"
echo ""

echo "🔒 Additional security tools to consider:"
echo "[ ] Microsoft DevSkim CLI: dotnet tool install --global Microsoft.CST.DevSkim.CLI"
echo "[ ] Snyk: npm install -g snyk (for Node.js dependencies if any)"
echo "[ ] OWASP Dependency Check"
echo "[ ] SonarQube or SonarCloud integration"
echo "[ ] Application Insights for runtime monitoring"
echo ""

echo "✅ Security audit completed!"
echo "Remember to:"
echo "• Regularly update packages after security review"
echo "• Monitor for new vulnerabilities"
echo "• Keep documentation updated"
echo "• Train team members on security best practices"
echo ""

# Save audit results to file
AUDIT_FILE="security-audit-$(date +%Y%m%d-%H%M%S).log"
echo "📄 Saving audit results to: $AUDIT_FILE"

# Re-run the checks and save to file
{
    echo "Security Audit Report - $(date)"
    echo "=============================="
    echo ""
    echo "Vulnerable packages:"
    dotnet list package --vulnerable 2>/dev/null
    echo ""
    echo "Deprecated packages:"
    dotnet list package --deprecated 2>/dev/null
    echo ""
    echo "Outdated packages:"
    dotnet list package --outdated 2>/dev/null
} > "$AUDIT_FILE"

echo "📊 Audit log saved to: $AUDIT_FILE"
