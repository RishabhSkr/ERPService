import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const identityApi = axios.create({
    baseURL: API_BASE_URL, // API Gateway URL
    headers: {
        "Content-Type": "application/json",
        "ngrok-skip-browser-warning": "69420"
    }
});

// Interceptor to attach token
identityApi.interceptors.request.use((config) => {
    const token = localStorage.getItem("erp_token");
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// ─── USER MANAGEMENT ───
export const getAllUsers = async () => {
    const response = await identityApi.get('/users');
    return response.data;
};

export const approveUser = async (id, roleId) => {
    // Expected body: { roleId: "guid" }
    const response = await identityApi.put(`/users/${id}/approve`, { roleId });
    return response.data;
};

export const suspendUser = async (id) => {
    const response = await identityApi.put(`/users/${id}/suspend`);
    return response.data;
};

export const getUserById = async (id) => {
    const response = await identityApi.get(`/users/${id}`);
    return response.data;
};

export const updateUser = async (id, data) => {
    const response = await identityApi.put(`/users/${id}`, { userId: id, ...data });
    return response.data;
};

// ─── ROLE MANAGEMENT ───
export const getAllRoles = async () => {
    const response = await identityApi.get('/roles');
    return response.data;
};

export const createRole = async (data) => {
    const response = await identityApi.post('/roles', data);
    return response.data;
};

export const deleteRole = async (id) => {
    const response = await identityApi.delete(`/roles/${id}`);
    return response.data;
};


// ─── PERMISSION MANAGEMENT ───
export const getAllModules = async () => {
    const response = await identityApi.get('/permissions/modules');
    return response.data;
};

export const getPermissionsByRole = async (roleId) => {
    const response = await identityApi.get(`/permissions/role/${roleId}`);
    return response.data;
};

export const grantPermission = async (data) => {
    const response = await identityApi.post('/permissions/grant', data);
    return response.data;
};

export const revokePermission = async (id) => {
    const response = await identityApi.delete(`/permissions/revoke/${id}`);
    return response.data;
};

export const createModule = async (data) => {
    const response = await identityApi.post('/permissions/modules', data);
    return response.data;
};
