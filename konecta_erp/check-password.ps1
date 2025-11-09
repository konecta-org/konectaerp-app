# Check Authentication Service logs for employee password
Write-Host "Checking logs for employee credentials..." -ForegroundColor Cyan
docker compose logs authentication-service --tail 100 | Select-String -Pattern "Password is:|Welcome email" -Context 1,0
