import axios from 'axios';

// ====================================================================
// Multi-Service Axios Instances
// Each microservice runs on its own port
// ====================================================================

const createApiInstance = (baseURL, serviceName) => {
    const instance = axios.create({
        baseURL,
        headers: { 'Content-Type': 'application/json' },
        timeout: 10000,
    });

    // Response Interceptor
    instance.interceptors.response.use(
        (response) => response,
        (error) => {
            const message = error.response?.data?.message || error.message;
            console.error(`[${serviceName}] API Error:`, message);
            return Promise.reject(error);
        }
    );

    return instance;
};

// Service instances
export const salesApi = createApiInstance('http://localhost:5002/api', 'Sales');
export const productionApi = createApiInstance('http://localhost:5006/api', 'Production');
export const inventoryApi = createApiInstance('http://localhost:5004/api', 'Inventory');

// Default export (backward compatibility — points to Production for now)
export default productionApi;