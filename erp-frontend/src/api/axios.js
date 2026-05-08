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

    instance.interceptors.request.use((config) => {
        const token = localStorage.getItem("erp_token");
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    });

    // Response Interceptor
    instance.interceptors.response.use(
        (response) => response,
        (error) => {
            if(error.response?.status === 401){
                console.log('🔴 401 from:', error.config?.url);
                localStorage.removeItem('erp_token');
                localStorage.removeItem('erp_user');
                window.location.href = '/login'
            }
            const message = error.response?.data?.message || error.message;
            console.error(`[${serviceName}] API Error:`, message);
            return Promise.reject(error);
        }
    );

    return instance;
};

// Service instances
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

export const salesApi = createApiInstance(API_BASE_URL, 'Sales');
export const productionApi = createApiInstance(API_BASE_URL, 'Production');
export const inventoryApi = createApiInstance(API_BASE_URL, 'Inventory');

// Default export (backward compatibility — points to Production for now)
export default productionApi;