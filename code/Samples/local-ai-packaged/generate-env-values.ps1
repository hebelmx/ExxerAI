#!/usr/bin/env pwsh

Write-Host "=== Generated .env Configuration Values ===" -ForegroundColor Green
Write-Host ""
Write-Host "Copy these values to your .env file:" -ForegroundColor Yellow
Write-Host ""

# Generate all the random values
$n8nKey = -join ((1..32) | ForEach {[char]((65..90) + (97..122) + (48..57) | Get-Random)})
$jwtSecret = -join ((1..32) | ForEach {[char]((65..90) + (97..122) + (48..57) | Get-Random)})
$postgresPassword = -join ((1..32) | ForEach {[char]((65..90) + (97..122) + (48..57) | Get-Random)})
$supabaseJwt = -join ((1..32) | ForEach {[char]((65..90) + (97..122) + (48..57) | Get-Random)})
$neo4jPassword = -join ((1..16) | ForEach {[char]((65..90) + (97..122) + (48..57) | Get-Random)})
$clickhousePassword = -join ((1..32) | ForEach {[char]((65..90) + (97..122) + (48..57) | Get-Random)})
$minioPassword = -join ((1..32) | ForEach {[char]((65..90) + (97..122) + (48..57) | Get-Random)})
$langfuseSalt = -join ((1..32) | ForEach {[char]((65..90) + (97..122) + (48..57) | Get-Random)})
$nextAuthSecret = -join ((1..32) | ForEach {[char]((65..90) + (97..122) + (48..57) | Get-Random)})
$encryptionKey = -join ((1..32) | ForEach {[char]((65..90) + (97..122) + (48..57) | Get-Random)})

Write-Host "############" -ForegroundColor Cyan
Write-Host "# N8N Configuration" -ForegroundColor Cyan
Write-Host "############" -ForegroundColor Cyan
Write-Host "N8N_ENCRYPTION_KEY=$n8nKey" -ForegroundColor White
Write-Host "N8N_USER_MANAGEMENT_JWT_SECRET=$jwtSecret" -ForegroundColor White
Write-Host ""

Write-Host "############" -ForegroundColor Cyan
Write-Host "# Supabase Secrets" -ForegroundColor Cyan
Write-Host "############" -ForegroundColor Cyan
Write-Host "POSTGRES_PASSWORD=$postgresPassword" -ForegroundColor White
Write-Host "JWT_SECRET=$supabaseJwt" -ForegroundColor White
Write-Host "ANON_KEY=your-supabase-anon-key-here" -ForegroundColor Red
Write-Host "SERVICE_ROLE_KEY=your-supabase-service-role-key-here" -ForegroundColor Red
Write-Host "DASHBOARD_USERNAME=admin" -ForegroundColor White
Write-Host "DASHBOARD_PASSWORD=AdminPassword123" -ForegroundColor White
Write-Host "POOLER_TENANT_ID=local-ai-instance" -ForegroundColor White
Write-Host "POOLER_DB_POOL_SIZE=5" -ForegroundColor White
Write-Host ""

Write-Host "############" -ForegroundColor Cyan
Write-Host "# Neo4j Secrets" -ForegroundColor Cyan
Write-Host "############" -ForegroundColor Cyan
Write-Host "NEO4J_AUTH=neo4j/$neo4jPassword" -ForegroundColor White
Write-Host ""

Write-Host "############" -ForegroundColor Cyan
Write-Host "# Langfuse credentials" -ForegroundColor Cyan
Write-Host "############" -ForegroundColor Cyan
Write-Host "CLICKHOUSE_PASSWORD=$clickhousePassword" -ForegroundColor White
Write-Host "MINIO_ROOT_PASSWORD=$minioPassword" -ForegroundColor White
Write-Host "LANGFUSE_SALT=$langfuseSalt" -ForegroundColor White
Write-Host "NEXTAUTH_SECRET=$nextAuthSecret" -ForegroundColor White
Write-Host "ENCRYPTION_KEY=$encryptionKey" -ForegroundColor White
Write-Host ""

Write-Host "############" -ForegroundColor Cyan
Write-Host "# Optional configurations" -ForegroundColor Cyan
Write-Host "############" -ForegroundColor Cyan
Write-Host "FLOWISE_USERNAME=" -ForegroundColor White
Write-Host "FLOWISE_PASSWORD=" -ForegroundColor White
Write-Host "SEARXNG_UWSGI_WORKERS=4" -ForegroundColor White
Write-Host "SEARXNG_UWSGI_THREADS=4" -ForegroundColor White
Write-Host "REDIS_HOST=redis" -ForegroundColor White
Write-Host "REDIS_PORT=6379" -ForegroundColor White
Write-Host "REDIS_AUTH=LOCALONLYREDIS" -ForegroundColor White
Write-Host "REDIS_TLS_ENABLED=false" -ForegroundColor White
Write-Host ""

Write-Host "=== IMPORTANT NOTES ===" -ForegroundColor Red
Write-Host "1. For ANON_KEY and SERVICE_ROLE_KEY, visit: https://supabase.com/docs/guides/self-hosting/docker#api-keys" -ForegroundColor Yellow
Write-Host "2. Use the JWT_SECRET above to generate these keys" -ForegroundColor Yellow
Write-Host "3. Create your .env file and paste all these values" -ForegroundColor Yellow
Write-Host "4. For production deployment, uncomment and configure the Caddy section" -ForegroundColor Yellow 