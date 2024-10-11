import https from "https";
import axios from "axios";
import { errorLogger, requestLogger, responseLogger, setGlobalConfig } from "axios-logger";

const axiosInstance = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_BASE_URL,
    headers: {
        'Content-Type': 'application/json'
    }
});

setGlobalConfig({
    prefixText: 'api',
    dateFormat: 'HH:MM:ss',
    headers: true,
    params: true
});

axiosInstance.interceptors.request.use(requestLogger, errorLogger);
axiosInstance.interceptors.response.use(responseLogger, errorLogger);

if(process.env.NODE_ENV === 'development') {
    const httpsAgent = new https.Agent({
        rejectUnauthorized: false
    });
    axiosInstance.defaults.httpAgent = httpsAgent;
    console.log(process.env.NODE_ENV, 'RejectUnauthorized is disabled');
}

export default axiosInstance;