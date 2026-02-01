Write-Host "=== 🧪 Testing error case (404) ==="
try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/users/999/result' -Method Get
    Write-Host "✅ Unexpected success:"
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "❌ Expected 404 Error:"
    Write-Host "Status Code:" $_.Exception.Response.StatusCode.value__
    Write-Host "Message:" $_.Exception.Message
}
