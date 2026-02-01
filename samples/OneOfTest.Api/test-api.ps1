Write-Host "=== 🧪 Creating user ==="
$body = '{"name":"Test User","email":"test@example.com"}'
try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/users/result' -Method Post -Body $body -ContentType 'application/json'
    Write-Host "✅ User created:"
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "❌ Error creating user:"
    $_.Exception.Message
}

Write-Host "`n=== 🧪 Testing T1,T2 OneOf endpoint ==="
try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/users/1/result' -Method Get
    Write-Host "✅ API Response:"
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "❌ Error getting user:"
    $_.Exception.Message
}
