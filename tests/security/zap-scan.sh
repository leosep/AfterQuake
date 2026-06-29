#!/bin/bash
# OWASP ZAP Security Scan
# Run: bash tests/security/zap-scan.sh

TARGET_URL="${1:-http://localhost:5000}"

docker run -v $(pwd)/tests/security:/zap/wrk:rw \
  -t ghcr.io/zaproxy/zaproxy:stable \
  zap-full-scan.py \
  -t $TARGET_URL \
  -r zap-report.html \
  -w zap-report.md \
  -x zap-report.xml \
  -z "-config globalexcludeurl.url_list.url\(0\).regex='.*/health/.*'"
