#!/bin/bash

# 🧹 Project Cleanup Script
# Organizes files into proper folder structure

echo "🧹 Organizing project files..."

# Create folder structure
mkdir -p docs/{security,deployment,guides,performance,architecture}
mkdir -p scripts
mkdir -p tests

# Move security documentation
echo "📁 Moving security docs..."
[ -f "SECURITY_IMPLEMENTATION_GUIDE.md" ] && mv SECURITY_IMPLEMENTATION_GUIDE.md docs/security/
[ -f "SECURITY_GUIDE.md" ] && mv SECURITY_GUIDE.md docs/security/
[ -f "SECURITY_CHECKLIST.md" ] && mv SECURITY_CHECKLIST.md docs/security/
[ -f "SECURITY_ACROSS_FRAMEWORKS.md" ] && mv SECURITY_ACROSS_FRAMEWORKS.md docs/security/
[ -f "README_SECURITY.md" ] && mv README_SECURITY.md docs/security/

# Move package security docs
[ -f "PACKAGE_RESEARCH_GUIDE.md" ] && mv PACKAGE_RESEARCH_GUIDE.md docs/security/
[ -f "PACKAGE_UPDATE_EVALUATION_EXAMPLE.md" ] && mv PACKAGE_UPDATE_EVALUATION_EXAMPLE.md docs/security/
[ -f "PACKAGE_SECURITY_VALIDATION.md" ] && mv PACKAGE_SECURITY_VALIDATION.md docs/security/

# Move deployment documentation
echo "🚀 Moving deployment docs..."
[ -f "DEPLOYMENT_GUIDE.md" ] && mv DEPLOYMENT_GUIDE.md docs/deployment/
[ -f "RENDER_DEPLOYMENT_GUIDE.md" ] && mv RENDER_DEPLOYMENT_GUIDE.md docs/deployment/
[ -f "RENDER_DASHBOARD_GUIDE.md" ] && mv RENDER_DASHBOARD_GUIDE.md docs/deployment/
[ -f "MONGODB_SETUP_GUIDE.md" ] && mv MONGODB_SETUP_GUIDE.md docs/deployment/
[ -f "ALTERNATIVE_DEPLOYMENT_OPTIONS.md" ] && mv ALTERNATIVE_DEPLOYMENT_OPTIONS.md docs/deployment/

# Move architecture guides
echo "🏗️ Moving architecture docs..."
[ -f "DOTNET_REACT_ARCHITECTURE_GUIDE.md" ] && mv DOTNET_REACT_ARCHITECTURE_GUIDE.md docs/architecture/
[ -f "DOTNET_OPENSOURCE_ECOSYSTEM.md" ] && mv DOTNET_OPENSOURCE_ECOSYSTEM.md docs/architecture/
[ -f "API_Structure_Guide.md" ] && mv API_Structure_Guide.md docs/architecture/
[ -f "REACT_FRONTEND_STARTER_GUIDE.md" ] && mv REACT_FRONTEND_STARTER_GUIDE.md docs/architecture/

# Move performance documentation
echo "⚡ Moving performance docs..."
[ -f "DATABASE_PERFORMANCE_ANALYSIS.md" ] && mv DATABASE_PERFORMANCE_ANALYSIS.md docs/performance/
[ -f "CACHE_CONFIGURATION_GUIDE.md" ] && mv CACHE_CONFIGURATION_GUIDE.md docs/performance/
[ -f "CACHE_TESTING_DEMO.md" ] && mv CACHE_TESTING_DEMO.md docs/performance/
[ -f "MEMORY_CACHE_VS_REDIS_GUIDE.md" ] && mv MEMORY_CACHE_VS_REDIS_GUIDE.md docs/performance/
[ -f "MEMORY_CACHE_AUTO_CLEANUP.md" ] && mv MEMORY_CACHE_AUTO_CLEANUP.md docs/performance/
[ -f "MEMORY_CACHE_CAPACITY_ANALYSIS.md" ] && mv MEMORY_CACHE_CAPACITY_ANALYSIS.md docs/performance/
[ -f "SERVER_SIDE_CACHING_EXPLAINED.md" ] && mv SERVER_SIDE_CACHING_EXPLAINED.md docs/performance/
[ -f "WHAT_WE_CACHE_ANALYSIS.md" ] && mv WHAT_WE_CACHE_ANALYSIS.md docs/performance/

# Move general guides
echo "📖 Moving guides..."
[ -f "TESTING_GUIDE.md" ] && mv TESTING_GUIDE.md docs/guides/
[ -f "IMPLEMENTATION_SUMMARY.md" ] && mv IMPLEMENTATION_SUMMARY.md docs/guides/
[ -f "CONFIGURATION_QUICK_REFERENCE.md" ] && mv CONFIGURATION_QUICK_REFERENCE.md docs/guides/
[ -f "USER_UPDATE_FIX.md" ] && mv USER_UPDATE_FIX.md docs/guides/
[ -f "ROOM_UPDATE_FIX.md" ] && mv ROOM_UPDATE_FIX.md docs/guides/
[ -f "DEACTIVATED_USER_SECURITY_FIX.md" ] && mv DEACTIVATED_USER_SECURITY_FIX.md docs/guides/
[ -f "ENVIRONMENT_SETUP.md" ] && mv ENVIRONMENT_SETUP.md docs/guides/
[ -f "PROJECT_COMPLETE.md" ] && mv PROJECT_COMPLETE.md docs/guides/

# Move scripts
echo "🔧 Moving scripts..."
[ -f "deploy.sh" ] && mv deploy.sh scripts/
[ -f "security-audit.sh" ] && mv security-audit.sh scripts/
[ -f "demo-protection.sh" ] && mv demo-protection.sh scripts/
[ -f "security-test.sh" ] && mv security-test.sh scripts/
[ -f "test-render-commands.sh" ] && mv test-render-commands.sh scripts/
[ -f "set-env.sh" ] && mv set-env.sh scripts/
[ -f "set-env.template.sh" ] && mv set-env.template.sh scripts/

# Move test files
echo "🧪 Moving test files..."
[ -f "test-security-demo.http" ] && mv test-security-demo.http tests/
[ -f "test-role-authorization.http" ] && mv test-role-authorization.http tests/
[ -f "test-user-update-fix.http" ] && mv test-user-update-fix.http tests/
[ -f "test-deactivated-user-security.http" ] && mv test-deactivated-user-security.http tests/
[ -f "test-query-protection.http" ] && mv test-query-protection.http tests/
[ -f "test-memory-cache-usage.http" ] && mv test-memory-cache-usage.http tests/

# Clean up temp files
echo "🗑️ Cleaning up..."
[ -f "RENDER_ENV_VARIABLES.txt" ] && rm RENDER_ENV_VARIABLES.txt
[ -d "out" ] && rm -rf out

echo "✅ Project organization complete!"
echo ""
echo "📁 New structure:"
echo "├── docs/"
echo "│   ├── security/     # Security guides and best practices"
echo "│   ├── deployment/   # Deployment guides for various platforms"
echo "│   ├── guides/       # Implementation and configuration guides"
echo "│   ├── performance/  # Performance optimization docs"
echo "│   └── architecture/ # Architecture and design docs"
echo "├── scripts/          # Utility scripts"
echo "├── tests/           # HTTP test files"
echo "└── (clean root)     # Only essential project files"
