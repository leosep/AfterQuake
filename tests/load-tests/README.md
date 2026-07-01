# Load Tests - AfterQuake

## Prerequisites
- Install k6: https://k6.io/docs/getting-started/installation/

## Run tests

### Emergency page load test:
```bash
k6 run tests/load-tests/emergency.js
```

### SOS stress test (simulates post-earthquake surge):
```bash
k6 run tests/load-tests/sos-stress.js
```

### Custom URL:
```bash
k6 run -e BASE_URL=https://staging.afterquake.com.do tests/load-tests/emergency.js
```

## Thresholds
- 95% of requests must complete under 2 seconds
- Error rate must be below 10%
