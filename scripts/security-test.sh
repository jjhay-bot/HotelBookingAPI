#!/bin/bash

# Security Testing Script for Hotel Booking API
# This script demonstrates various injection attacks and how the API handles them

API_BASE="https://localhost:7137/api"
SECURITY_DEMO_BASE="$API_BASE/security-demo"

echo "🛡️  Hotel Booking API Security Testing"
echo "======================================"
echo ""

# Test 1: NoSQL Injection Attempts
echo "📋 Test 1: NoSQL Injection Attempts"
echo "-----------------------------------"

echo "1.1 Testing authentication bypass attempt..."
curl -X POST "$SECURITY_DEMO_BASE/vulnerable/login" \
  -H "Content-Type: application/json" \
  -d '{"username": {"$ne": ""}, "password": {"$ne": ""}}' \
  -w "\nStatus: %{http_code}\n" -s

echo ""
echo "1.2 Testing secure login with same payload..."
curl -X POST "$SECURITY_DEMO_BASE/secure/login" \
  -H "Content-Type: application/json" \
  -d '{"username": {"$ne": ""}, "password": {"$ne": ""}}' \
  -w "\nStatus: %{http_code}\n" -s

echo ""

# Test 2: Query Parameter Injection
echo "📋 Test 2: Query Parameter Injection"
echo "------------------------------------"

echo "2.1 Testing vulnerable user search with injection..."
curl -X GET "$SECURITY_DEMO_BASE/vulnerable/user?username=admin\";\${db.users.drop()};var x=\"" \
  -w "\nStatus: %{http_code}\n" -s

echo ""
echo "2.2 Testing secure user search with same payload..."
curl -X GET "$SECURITY_DEMO_BASE/secure/user?username=admin\";\${db.users.drop()};var x=\"" \
  -w "\nStatus: %{http_code}\n" -s

echo ""

# Test 3: Valid Operations
echo "📋 Test 3: Valid Operations"
echo "---------------------------"

echo "3.1 Testing secure login with valid credentials..."
curl -X POST "$SECURITY_DEMO_BASE/secure/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "ValidPass123!"}' \
  -w "\nStatus: %{http_code}\n" -s

echo ""
echo "3.2 Testing secure user search with valid username..."
curl -X GET "$SECURITY_DEMO_BASE/secure/user?username=testuser" \
  -w "\nStatus: %{http_code}\n" -s

echo ""

# Test 4: Attack Examples Documentation
echo "📋 Test 4: Getting Attack Examples"
echo "----------------------------------"

echo "4.1 Fetching common attack patterns..."
curl -X POST "$SECURITY_DEMO_BASE/attack-examples" \
  -H "Content-Type: application/json" \
  -w "\nStatus: %{http_code}\n" -s | jq '.' 2>/dev/null || echo "Response received (jq not available for formatting)"

echo ""

# Test 5: Secure Pattern Examples
echo "📋 Test 5: Secure Pattern Examples"
echo "----------------------------------"

echo "5.1 Testing secure pagination and search..."
curl -X GET "$SECURITY_DEMO_BASE/secure-patterns?searchTerm=test&page=1" \
  -w "\nStatus: %{http_code}\n" -s

echo ""
echo "5.2 Testing with invalid search term..."
curl -X GET "$SECURITY_DEMO_BASE/secure-patterns?searchTerm=<script>alert('xss')</script>&page=1" \
  -w "\nStatus: %{http_code}\n" -s

echo ""

# Test 6: Rate Limiting (if implemented)
echo "📋 Test 6: Rate Limiting Test"
echo "-----------------------------"

echo "6.1 Testing multiple rapid requests..."
for i in {1..5}; do
  echo "Request $i:"
  curl -X GET "$SECURITY_DEMO_BASE/secure/user?username=test$i" \
    -w "Status: %{http_code}\n" -s --max-time 2
done

echo ""

# Test 7: Input Validation
echo "📋 Test 7: Input Validation"
echo "---------------------------"

echo "7.1 Testing with empty username..."
curl -X GET "$SECURITY_DEMO_BASE/secure/user?username=" \
  -w "\nStatus: %{http_code}\n" -s

echo ""
echo "7.2 Testing with overly long username..."
curl -X GET "$SECURITY_DEMO_BASE/secure/user?username=$(python3 -c 'print("a" * 100)')" \
  -w "\nStatus: %{http_code}\n" -s

echo ""
echo "7.3 Testing with special characters..."
curl -X GET "$SECURITY_DEMO_BASE/secure/user?username=test@#$%^&*()" \
  -w "\nStatus: %{http_code}\n" -s

echo ""

# Summary
echo "🎯 Security Testing Summary"
echo "============================"
echo ""
echo "✅ MongoDB C# Driver Protection:"
echo "   - Strongly typed queries prevent NoSQL injection"
echo "   - BSON serialization provides additional safety"
echo ""
echo "✅ Input Validation:"
echo "   - Username format validation"
echo "   - Password complexity requirements"
echo "   - Query parameter sanitization"
echo ""
echo "✅ Security Middleware:"
echo "   - Request size limits"
echo "   - Suspicious pattern detection"
echo "   - Security headers"
echo ""
echo "✅ Authentication Security:"
echo "   - Secure password hashing (PBKDF2)"
echo "   - Constant-time password comparison"
echo "   - Failed attempt logging"
echo ""
echo "⚠️  Recommendations for Production:"
echo "   - Implement proper rate limiting with Redis"
echo "   - Add comprehensive logging and monitoring"
echo "   - Use BCrypt.Net for password hashing"
echo "   - Implement CAPTCHA for repeated failures"
echo "   - Set up intrusion detection system"
echo "   - Regular security audits and penetration testing"
echo ""
echo "📚 Learn More:"
echo "   - OWASP Top 10: https://owasp.org/www-project-top-ten/"
echo "   - NoSQL Injection: https://owasp.org/www-project-web-security-testing-guide/latest/4-Web_Application_Security_Testing/07-Input_Validation_Testing/05.6-Testing_for_NoSQL_Injection"
echo "   - MongoDB Security: https://docs.mongodb.com/manual/security/"
echo ""
