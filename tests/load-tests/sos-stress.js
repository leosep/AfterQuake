import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '1m', target: 50 },
        { duration: '3m', target: 50 },
        { duration: '1m', target: 0 },
    ],
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
    const payload = JSON.stringify({
        emergencyType: Math.floor(Math.random() * 8),
        severity: Math.floor(Math.random() * 4),
        latitude: -33.45 + (Math.random() - 0.5) * 2,
        longitude: -70.67 + (Math.random() - 0.5) * 2,
        description: 'Test emergency load - system stress test simulating concurrent reports during aftershock',
        affectedPeople: Math.floor(Math.random() * 10) + 1,
    });

    const res = http.post(`${BASE_URL}/Emergency/Report`, payload, {
        headers: { 'Content-Type': 'application/json' },
    });

    check(res, {
        'status is 200 or 429': (r) => r.status === 200 || r.status === 429,
    });

    sleep(Math.random() * 3 + 1);
}
