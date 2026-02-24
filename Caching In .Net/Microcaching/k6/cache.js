import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
	stages: [
		{ duration: '2s', target: 100 },      // Ramp up
		{ duration: '30s', target: 1000 },    // Ramp up
		{ duration: '2s', target: 0 },        // Ramp down
	],
};

export default function () {
	let res = http.get('https://localhost:7185/cache');
	check(res, {
		'status is 200': (r) => r.status === 200,
	});
	sleep(1);  // Add a 1s pause between iterations
}
