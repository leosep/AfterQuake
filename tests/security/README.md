# Security Testing

## OWASP ZAP Scan
```bash
bash tests/security/zap-scan.sh https://staging.afterquake.com.do
```

## Manual Checklist
- [ ] SQL Injection in all search fields
- [ ] XSS in all form inputs
- [ ] CSRF on all POST endpoints (verify anti-forgery)
- [ ] JWT token expiration and refresh
- [ ] Rate limiting effectiveness
- [ ] Path traversal in file upload
- [ ] IDOR (Insecure Direct Object Reference)
- [ ] Mass assignment on entity binding
- [ ] Session fixation
- [ ] Open redirect
- [ ] CORS misconfiguration
- [ ] Security headers presence
