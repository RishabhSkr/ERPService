import axios from "axios"

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const authApi = axios.create({
    baseURL: `${API_BASE_URL}/auth/`,
    headers: {
        "Content-Type": "application/json"
    }
}); 

const loginUser = async (username, password) => {
    try {
        const response = await authApi.post("/login", { username, password });
        return response.data;
    } catch (error) {
        throw error.response.data;
    }
}

const registerUser = async (username, email, password, roleId) => {
    try {
        const response = await authApi.post("/register", { username, email, password, roleId });
        return response.data;
    } catch (error) {
        throw error.response.data;
    }
}

const refresh = async (token) => {
    try {
        const response = await authApi.post("/refresh", { token });
        return response.data;
    } catch (error) {
        throw error.response.data;
    }
}

const forgetPassword = async (email) => {
    try {
        const response = await authApi.post("/forget-password", { email });
        return response.data;
    } catch (error) {
        throw error.response.data;
    }
}

const resetPassword = async (token, password) => {
    try {
        const response = await authApi.post("/reset-password", { token, password });
        return response.data;
    } catch (error) {
        throw error.response.data;
    }
}

const changePassword = async (currentPassword, newPassword) => {
    try {
        const response = await authApi.post("/change-password", { currentPassword, newPassword });
        return response.data;
    } catch (error) {
        throw error.response.data;
    }
}


const getPublicRoles = async () => {
    try {
        const response = await authApi.get("/roles/public");
        return response.data;
    } catch (error) {
        throw error.response?.data || error.message;
    }
}

const getMyProfile = async (token) => {
    try {
        const response = await authApi.get("/me", {
            headers: { Authorization: `Bearer ${token}` }
        });
        return response.data;
    } catch (error) {
        throw error.response?.data || error.message;
    }
}

const updateMyProfile = async (token, data) => {
    try {
        const response = await authApi.put("/me", data, {
            headers: { Authorization: `Bearer ${token}` }
        });
        return response.data;
    } catch (error) {
        throw error.response?.data || error.message;
    }
}

export { 
    loginUser, 
    registerUser, 
    refresh, 
    forgetPassword, 
    resetPassword, 
    changePassword, 
    getPublicRoles, 
    getMyProfile,
    updateMyProfile 
};