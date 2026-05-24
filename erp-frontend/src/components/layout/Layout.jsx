import React, { useState, useEffect } from 'react';
import Sidebar from './Sidebar';
import { Toaster } from 'react-hot-toast'; 

const Layout = ({ children }) => {
    // Read sidebar state from localStorage to sync margin
    const [collapsed, setCollapsed] = useState(() => {
        return localStorage.getItem('sidebar-collapsed') === 'true';
    });

    // Listen for localStorage changes (when sidebar toggles)
    useEffect(() => {
        const handleStorage = () => {
            setCollapsed(localStorage.getItem('sidebar-collapsed') === 'true');
        };

        // Custom event listener for same-tab updates
        window.addEventListener('storage-sidebar', handleStorage);
        
        // Also poll for changes (backup for same-tab)
        const interval = setInterval(() => {
            const current = localStorage.getItem('sidebar-collapsed') === 'true';
            setCollapsed(prev => prev !== current ? current : prev);
        }, 100);

        return () => {
            window.removeEventListener('storage-sidebar', handleStorage);
            clearInterval(interval);
        };
    }, []);

    return (
        <div className="flex min-h-screen bg-gray-50">
            <Sidebar />
            <div className={`flex-1 p-8 transition-all duration-300 ${
                collapsed ? 'ml-16' : 'ml-72'
            }`}>
                {children}
            </div>
            <Toaster position="top-right" />
        </div>
    );
};

export default Layout;