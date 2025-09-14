#!/bin/bash

# 🧪 Infinite Query Protection Demo
# This script simulates React infinite query scenarios

echo "🛡️ Testing Infinite Query Protection"
echo "======================================"

API_BASE="http://localhost:5000"

echo ""
echo "📊 Test 1: Normal browsing (should work fine)"
echo "---------------------------------------------"
for i in {1..5}; do
  echo "Request $i: $(curl -s -o /dev/null -w "%{http_code}" $API_BASE/api/rooms)"
  sleep 1
done

echo ""
echo ""
echo "🚨 Test 2: Burst attack simulation (React infinite loop)"
echo "-------------------------------------------------------"
echo "Sending 20 rapid requests (burst limit is 60)..."

for i in {1..20}; do
  response=$(curl -s -o /dev/null -w "%{http_code}" $API_BASE/api/rooms)
  echo "Request $i: HTTP $response"
  if [ $i -eq 10 ]; then
    echo "  ⚡ Still within burst limit..."
  fi
done

echo ""
echo "🔥 Test 3: Extreme burst (should trigger protection)"
echo "---------------------------------------------------"
echo "Sending 70 requests rapidly (will exceed burst limit)..."

success_count=0
blocked_count=0

for i in {1..70}; do    response=$(curl -s -o /dev/null -w "%{http_code}" $API_BASE/api/rooms)
  if [ "$response" = "200" ]; then
    success_count=$((success_count + 1))
  elif [ "$response" = "429" ]; then
    blocked_count=$((blocked_count + 1))
    if [ $blocked_count -eq 1 ]; then
      echo "  🛡️ Protection activated! First block at request $i"
    fi
  fi
  
  # Show progress every 10 requests
  if [ $((i % 10)) -eq 0 ]; then
    echo "  Progress: $i/70 - Success: $success_count, Blocked: $blocked_count"
  fi
done

echo ""
echo "📈 Results:"
echo "  ✅ Successful requests: $success_count"
echo "  🚫 Blocked requests: $blocked_count"
echo "  🎯 Protection effectiveness: $((blocked_count * 100 / 70))%"

echo ""
echo "🧪 Test 4: Login brute force protection"
echo "---------------------------------------"
echo "Testing auth endpoint (stricter limits)..."

auth_success=0
auth_blocked=0

for i in {1..10}; do
  response=$(curl -s -o /dev/null -w "%{http_code}" \
    -X POST \
    -H "Content-Type: application/json" \
    -d '{"username":"test","password":"wrong"}' \
    $API_BASE/api/auth/login)
    
  if [ "$response" = "401" ] || [ "$response" = "400" ]; then
    auth_success=$((auth_success + 1))
  elif [ "$response" = "429" ]; then
    auth_blocked=$((auth_blocked + 1))
    if [ $auth_blocked -eq 1 ]; then
      echo "  🛡️ Auth protection activated at request $i"
    fi
  fi
done

echo "  Auth Results:"
echo "    ✅ Processed: $auth_success"  
echo "    🚫 Blocked: $auth_blocked"

echo ""
echo "🎉 Protection Demo Complete!"
echo ""
echo "🔍 Check your server logs for detailed protection events."
echo "💡 Rate limit headers are included in successful responses."
echo "🛡️ Your server is now protected against infinite queries!"
