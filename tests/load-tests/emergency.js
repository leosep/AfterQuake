import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');

export const options = {
    stages: [
        { duration: '2m', target: 100 },
        { duration: '5m', target: 100 },
        { duration: '2m', target: 500 },
        { duration: '5m', target: 500 },
        { duration: '2m', target: 1000 },
        { duration: '5m', target: 1000 },
        { duration: '2m', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<2000'],
        errors: ['rate<0.1'],
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
    const responses = http.batch([
        ['GET', `${BASE_URL}/`],
        ['GET', `${BASE_URL}/Emergency`],
        ['GET', `${BASE_URL}/Person`],
        ['GET', `${BASE_URL}/Shelter/Map`],
        ['GET', `${BASE_URL}/HelpRequest`],
        ['GET', `${BASE_URL}/Guide`],
        ['GET', `${BASE_URL}/Directory`],
        ['GET', `${BASE_URL}/Donation`],
        ['GET', `${BASE_URL}/api/map/shelters`],
        ['GET', `${BASE_URL}/api/map/health-centers`],
    ]);

    for (const res of responses) {
        check(res, {
            'status is 200': (r) => r.status === 200,
            'response time < 2s': (r) => r.timings.duration < 2000,
        });
        errorRate.add(res.status !== 200);
    }

    sleep(1);
}
