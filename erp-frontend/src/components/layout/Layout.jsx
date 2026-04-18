import React from 'react';
import Sidebar from './Sidebar';
import { Toaster } from 'react-hot-toast'; 
const Layout = ({ children }) => {
    return (
        <div className="flex min-h-screen bg-gray-50">
            <Sidebar />
            <div className="ml-64 flex-1 p-8">
                {children}
            </div>
            <Toaster position="top-right" />
        </div>
    );
};

export default Layout;