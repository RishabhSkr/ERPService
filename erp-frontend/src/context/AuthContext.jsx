
import { createContext, useContext, useState, useEffect, Children } from 'react';
import { loginUser } from '../api/authService';

const AuthContext = createContext(null);

export const AuthProvider = ({children}) => {
    const [user,setUser] = useState(null);
    const [token,setToken] = useState(localStorage.getItem("erp_token"));
    const[loading,setLoading] = useState(true);

    // on mount 
    useEffect(() => {
        const savedToken = localStorage.getItem("erp_token");
        const savedUser = localStorage.getItem("erp_user");

        if (savedUser && savedToken) {
            setUser(JSON.parse(savedUser));
            setToken(savedToken);
        }
        setLoading(false);
    }, []);
    
    // login 
    const login = async (username, password) => {
        try {
            const res = await loginUser(username, password);
            console.log(res);
            const data = res?.data;

            // save the state 
            const userInfo = {
                id: data.userId,
                username: data.username,
                roleName: data.roleName
            }

            // 1. Save to localStorage
            localStorage.setItem("erp_token", data.accessToken);
            localStorage.setItem("erp_user", JSON.stringify(userInfo));
            // 2. Update React state
            setToken(data.accessToken);
            setUser(userInfo);
            return data;

        } catch (error) {
            console.error(error);
            throw error;
        }
    }

    const logout = () => {
        localStorage.removeItem("erp_token");
        localStorage.removeItem("erp_user");
        setToken(null);
        setUser(null);
    }

    // ─── Computed Properties ───
    const isAdmin = user?.roleName === 'SuperAdmin' || user?.roleName === 'Admin';
    
    return (
        <AuthContext.Provider value={{ 
            user, 
            token, 
            login, 
            logout, 
            loading, 
            isAuthenticated: !!token,
            isAdmin 
        }}>
            {children}
        </AuthContext.Provider>
    );
}

export const useAuth = () => useContext(AuthContext);
