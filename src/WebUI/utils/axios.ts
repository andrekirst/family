import https from "https";
import axios from "axios";

const axiosInstance = axios.create({
    baseURL: process.env.API_BASE_URL,
    headers: {
        'Content-Type': 'application/json'
    }
});

if(process.env.NODE_ENV === 'development') {
    const httpsAgent = new https.Agent({
        rejectUnauthorized: false
    });
    axios.defaults.httpAgent = httpsAgent;
    console.log(process.env.NODE_ENV, 'RejectUnauthorized is disabled');
}

export default axiosInstance;