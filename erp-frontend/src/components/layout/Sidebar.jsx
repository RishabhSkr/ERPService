import React, { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { 
    LayoutDashboard, Factory, Box, Layers, ChevronDown, 
    ChevronRight, PlayCircle, Clock, ChefHat, ShoppingCart, 
    ListOrdered, Cog, Building2, Wrench, ArrowUpDown, FolderTree, Ruler, Route, MapPin,
    Settings, Menu, X, ChevronLeft
} from 'lucide-react';

import {useAuth} from '../../context/AuthContext'
import UserProfileDropdown from './UserProfileDropdown';

const Sidebar = () => {
    const location = useLocation();
    const [openMenus, setOpenMenus] = useState({});
    const { isAdmin } = useAuth();

    // Collapsed state with localStorage persistence
    const [collapsed, setCollapsed] = useState(() => {
        const saved = localStorage.getItem('sidebar-collapsed');
        return saved === 'true';
    });

    const toggleCollapsed = () => {
        setCollapsed(prev => {
            const next = !prev;
            localStorage.setItem('sidebar-collapsed', String(next));
            return next;
        });
    };

    // Toggle logic for any level of menu
    const toggleMenu = (name, e) => {
        e.preventDefault();
        e.stopPropagation();
        setOpenMenus(prev => ({ ...prev, [name]: !prev[name] }));
    };

     const menuItems = [
        { type: 'header', label: 'SALES & CRM' },
        { path: '/app/sales/orders', name: 'Sales Orders', icon: ShoppingCart, type: 'link' },
        { path: '/app/sales/fulfillment', name: 'Fulfillment Dashboard', icon: ListOrdered, type: 'link' },
        
        // ═══════════════════════════════════
        // PRODUCTION MODULE
        // ═══════════════════════════════════
        { type: 'header', label: 'PRODUCTION' },
        {
            name: 'Planning & Orders',
            icon: Factory,
            type: 'sub',
            subItems: [
                { path: '/app', name: 'PO Planning', icon: LayoutDashboard, type: 'link' },
                { path: '/app/production-create-order', name: 'Create PO Order', icon: ChefHat },
                { path: '/app/production-plan', name: 'PO Management', icon: Clock },
                { path: '/app/production-orders', name: 'All Orders', icon: ListOrdered },
            ]
        },
        {
            name: 'WO Execution',
            icon: PlayCircle,
            type: 'sub',
            subItems: [
                { path: '/app/production/wo-dashboard', name: 'WO Dashboard', icon: PlayCircle },
                { path: '/app/production/work-orders', name: 'Work Orders', icon: PlayCircle },
            ]
        },
        {
            name: 'Production Setup',
            icon: Cog,
            type: 'sub',
            subItems: [
                { path: '/app/production/bom', name: 'Bill of Materials', icon: Layers },
                { path: '/app/production/processes', name: 'Processes', icon: Cog },
                { path: '/app/production/process-routes', name: 'Process Routes', icon: Route },
                { path: '/app/production/work-centers', name: 'Work Centers', icon: Building2 },
                { path: '/app/production/equipment', name: 'Equipment', icon: Wrench },
            ]
        },

        // ═══════════════════════════════════
        // INVENTORY MODULE
        // ═══════════════════════════════════
        { type: 'header', label: 'INVENTORY' },
        {
            name: 'Inventory Management',
            icon: ArrowUpDown,
            type: 'sub',
            subItems: [
                { path: '/app/inventory/raw-material', name: 'Raw Material Stock', icon: Box },
                { path: '/app/inventory/finished-goods', name: 'Finished Goods Stock', icon: Box },
                { path: '/app/inventory/stock-movements', name: 'Stock Movements', icon: ArrowUpDown },
            ]
        },
        {
            name: 'Warehouse Setup',
            icon: Building2,
            type: 'sub',
            subItems: [
                { path: '/app/masters/warehouses', name: 'Warehouses', icon: Building2 },
                { path: '/app/masters/storage-locations', name: 'Storage Locations', icon: MapPin },
                { path: '/app/masters/storage-location-types', name: 'Location Types', icon: Layers },
            ]
        },

        // ═══════════════════════════════════
        // MASTER DATA
        // ═══════════════════════════════════
        { type: 'header', label: 'MASTER DATA' },
        {
            name: 'Master Data Management',
            icon: Layers,
            type: 'sub',
            subItems: [
                { path: '/app/masters/raw-materials', name: 'Raw Materials', icon: Box },
                { path: '/app/masters/products', name: 'Products (FG)', icon: Box },
                { path: '/app/masters/categories', name: 'Categories', icon: FolderTree },
                { path: '/app/masters/units', name: 'Units', icon: Ruler },
            ]
        },
        
        // ═══════════════════════════════════
        // SETTINGS (Admin Only)
        // ═══════════════════════════════════
        ...(isAdmin ? [
            { type: 'header', label: 'SETTINGS' },
            {
                name: 'Admin Settings',
                icon: Settings,
                type: 'sub',
                subItems: [
                    { path: '/app/settings/users', name: 'User Management', icon: Settings, type: 'link' },
                    { path: '/app/settings/roles', name: 'Role Management', icon: Settings, type: 'link' },
                    { path: '/app/settings/permissions', name: 'Permissions', icon: Settings, type: 'link' },
                    { path: '/app/settings/modules', name: 'Modules', icon: Settings, type: 'link' },
                ]
            }
        ] : []),
    ];

    // Recursive Function to handle Infinite Nesting(Advanced Menu)
    const renderMenuItem = (item, index, depth = 0) => {
        const Icon = item.icon;
        
        // 1. Header Case
        if (item.type === 'header') {
            if (collapsed) return null; // Hide headers when collapsed
            return (
                <div key={index} className="px-4 pt-6 pb-2 text-xs font-bold text-slate-500 uppercase tracking-wider">
                    {item.label}
                </div>
            );
        }

        // 2. Dropdown (Sub-menu) Case
        if (item.type === 'sub') {
            const isMenuOpen = openMenus[item.name];
            
            const isChildActive = (items) => {
                return items.some(sub => 
                    sub.path === location.pathname || (sub.subItems && isChildActive(sub.subItems))
                );
            };
            const isActive = isChildActive(item.subItems);

            // When collapsed, show only icon with tooltip
            if (collapsed) {
                return (
                    <div key={index} className="relative group">
                        <button
                            onClick={(e) => toggleMenu(item.name, e)}
                            className={`w-full flex items-center justify-center py-3 hover:bg-slate-800 transition-colors ${
                                isActive ? 'text-blue-400' : 'text-slate-300'
                            }`}
                        >
                            {Icon && <Icon size={20} />}
                        </button>
                        {/* Tooltip */}
                        <div className="absolute left-full top-0 ml-2 hidden group-hover:block z-50">
                            <div className="bg-slate-800 text-white text-xs py-1 px-2 rounded whitespace-nowrap shadow-lg border border-slate-700">
                                {item.name}
                            </div>
                        </div>
                    </div>
                );
            }

            const paddingLeft = depth === 0 ? '1rem' : `${depth * 1.5 + 1}rem`;

            return (
                <div key={index}>
                    <button
                        onClick={(e) => toggleMenu(item.name, e)}
                        style={{ paddingLeft }}
                        className={`w-full flex items-center justify-between py-3 pr-4 hover:bg-slate-800 transition-colors ${
                            isActive ? 'text-blue-400' : 'text-slate-300'
                        }`}
                    >
                        <div className="flex items-center gap-3">
                            {Icon && <Icon size={18} />} 
                            <span className="text-sm font-medium">{item.name}</span>
                        </div>
                        {isMenuOpen ? <ChevronDown size={15} /> : <ChevronRight size={15} />}
                    </button>

                    {/* Render Children Recursively if Open */}
                    {isMenuOpen && (
                        <div className="bg-slate-950 border-l border-slate-800 ml-4">
                            {item.subItems.map((subItem, subIndex) => 
                                renderMenuItem(subItem, subIndex, depth + 1)
                            )}
                        </div>
                    )}
                </div>
            );
        }

        // 3. Standard Link Case
        const isActive = location.pathname === item.path;

        // When collapsed, show only icon with tooltip
        if (collapsed) {
            return (
                <Link
                    key={index}
                    to={item.path}
                    className={`flex items-center justify-center py-3 hover:bg-slate-800 transition-colors relative group ${
                        isActive ? 'bg-blue-600/20 text-blue-400 border-r-2 border-blue-400' : 'text-slate-400 hover:text-white'
                    }`}
                >
                    {Icon && <Icon size={20} />}
                    {/* Tooltip */}
                    <div className="absolute left-full top-0 ml-2 hidden group-hover:block z-50">
                        <div className="bg-slate-800 text-white text-xs py-1 px-2 rounded whitespace-nowrap shadow-lg border border-slate-700">
                            {item.name}
                        </div>
                    </div>
                </Link>
            );
        }

        const paddingLeft = depth === 0 ? '1rem' : `${depth * 1.5 + 1}rem`;

        return (
            <Link
                key={index}
                to={item.path}
                style={{ paddingLeft }}
                className={`flex items-center gap-3 py-3 pr-4 hover:bg-slate-800 transition-colors ${
                    isActive ? 'bg-blue-600/20 text-blue-400 border-r-2 border-blue-400' : 'text-slate-400 hover:text-white'
                }`}
            >
                {Icon && <Icon size={18} />}
                <span className="text-sm">{item.name}</span>
            </Link>
        );
    };

    return (
        <div className={`h-screen bg-slate-900 text-white fixed left-0 top-0 flex flex-col transition-all duration-300 z-40 ${
            collapsed ? 'w-16' : 'w-72'
        }`}>
            {/* Header with Toggle */}
            <div className="p-3 border-b border-slate-700 flex items-center shrink-0">
                {collapsed ? (
                    <button onClick={toggleCollapsed} className="w-full flex justify-center text-slate-400 hover:text-white transition-colors p-1">
                        <Menu size={22} />
                    </button>
                ) : (
                    <div className="flex items-center justify-between w-full">
                        <Link to="/" className="flex items-center gap-2 text-xl font-bold hover:text-blue-400 transition-colors">
                            <Factory className="text-blue-500" size={24} /> ERP System
                        </Link>
                        <button onClick={toggleCollapsed} className="text-slate-400 hover:text-white transition-colors p-1 rounded hover:bg-slate-800">
                            <ChevronLeft size={20} />
                        </button>
                    </div>
                )}
            </div>
            
            {/* Scrollable Navigation */}
            <nav className="mt-2 flex-1 overflow-y-auto pb-4 scrollbar-hide">
                {menuItems.map((item, index) => renderMenuItem(item, index))}
            </nav>

            {/* Fixed Footer (User Profile) */}
            {!collapsed && <UserProfileDropdown />}
            {collapsed && (
                <div className="p-2 border-t border-slate-700 flex justify-center">
                    <div className="w-8 h-8 rounded-full bg-blue-600 flex items-center justify-center text-xs font-bold">
                        U
                    </div>
                </div>
            )}
        </div>
    );
};

export default Sidebar;