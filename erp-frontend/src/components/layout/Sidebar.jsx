import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { 
    LayoutDashboard, Factory, Box, Layers, ChevronDown, 
    ChevronRight, PlayCircle, Clock, ChefHat, ShoppingCart, 
    ListOrdered, Cog, Building2, Wrench, ArrowUpDown, FolderTree, Ruler, Route, MapPin
} from 'lucide-react';

const Sidebar = () => {
    const location = useLocation();
    const [openMenus, setOpenMenus] = useState({});

    // Toggle logic for any level of menu
    const toggleMenu = (name, e) => {
        e.preventDefault(); // Prevent navigation if it has a path
        e.stopPropagation();
        setOpenMenus(prev => ({ ...prev, [name]: !prev[name] }));
    };
     const menuItems = [
        // ═══════════════════════════════════
        // SALES & CRM
        // ═══════════════════════════════════
        { type: 'header', label: 'SALES & CRM' },
        { path: '/sales/orders', name: 'Sales Orders', icon: ShoppingCart, type: 'link' },
        { path: '/sales/fulfillment', name: 'Fulfillment Dashboard', icon: ListOrdered, type: 'link' },
        
        // ═══════════════════════════════════
        // PRODUCTION MODULE
        // ═══════════════════════════════════
        { type: 'header', label: 'PRODUCTION' },
        {
            name: 'Planning & Orders',
            icon: Factory,
            type: 'sub',
            subItems: [
                { path: '/', name: 'PO Planning', icon: LayoutDashboard, type: 'link' },
                { path: '/production-create-order', name: 'Create PO Order', icon: ChefHat },
                { path: '/production-plan', name: 'PO Management', icon: Clock },
                { path: '/production-orders', name: 'All Orders', icon: ListOrdered },
            ]
        },
        {
            name: 'WO Execution',
            icon: PlayCircle,
            type: 'sub',
            subItems: [
                { path: '/production/wo-dashboard', name: 'WO Dashboard', icon: PlayCircle },
                { path: '/production/work-orders', name: 'Work Orders', icon: PlayCircle },
            ]
        },
        {
            name: 'Production Setup',
            icon: Cog,
            type: 'sub',
            subItems: [
                { path: '/production/bom', name: 'Bill of Materials', icon: Layers },
                { path: '/production/processes', name: 'Processes', icon: Cog },
                { path: '/production/process-routes', name: 'Process Routes', icon: Route },
                { path: '/production/work-centers', name: 'Work Centers', icon: Building2 },
                { path: '/production/equipment', name: 'Equipment', icon: Wrench },
            ]
        },

        // ═══════════════════════════════════
        // INVENTORY MODULE
        // ═══════════════════════════════════
        { type: 'header', label: 'INVENTORY' },
        {
            name: 'Inventory Management',
            icon: Box,
            type: 'sub',
            subItems: [
                { path: '/inventory/raw-material', name: 'Raw Material Stock', icon: Box },
                { path: '/inventory/finished-goods', name: 'Finished Goods', icon: Box },
                { path: '/inventory/stock-movements', name: 'Stock Movements', icon: ArrowUpDown },
            ]
        },
        {
            name: 'Warehouse Setup',
            icon: Building2,
            type: 'sub',
            subItems: [
                { path: '/masters/warehouses', name: 'Warehouses', icon: Building2 },
                { path: '/masters/storage-locations', name: 'Storage Locations', icon: MapPin },
                { path: '/masters/storage-location-types', name: 'Location Types', icon: Layers },
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
                { path: '/masters/raw-materials', name: 'Raw Materials', icon: Box },
                { path: '/masters/products', name: 'Products (FG)', icon: Box },
                { path: '/masters/categories', name: 'Categories', icon: FolderTree },
                { path: '/masters/units', name: 'Units', icon: Ruler },
            ]
        },
    ];

    // Recursive Function to handle Infinite Nesting(Advanced Menu)
    const renderMenuItem = (item, index, depth = 0) => {
        const Icon = item.icon;
        
        // 1. Header Case
        if (item.type === 'header') {
            return (
                <div key={index} className="px-4 pt-6 pb-2 text-xs font-bold text-slate-500 uppercase tracking-wider">
                    {item.label}
                </div>
            );
        }

        // 2. Dropdown (Sub-menu) Case
        if (item.type === 'sub') {
            const isMenuOpen = openMenus[item.name];
            
            // Check if any child inside this tree is active (for highlighting parent)
            const isChildActive = (items) => {
                return items.some(sub => 
                    sub.path === location.pathname || (sub.subItems && isChildActive(sub.subItems))
                );
            };
            const isActive = isChildActive(item.subItems);

            // Dynamic Padding for nesting indentation
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
        <div className="h-screen w-72 bg-slate-900 text-white fixed left-0 top-0 overflow-y-auto pb-10">
            <div className="p-4 text-2xl font-bold border-b border-slate-700 flex items-center gap-2">
                <Factory className="text-blue-500"/> ERP System
            </div>
            
            <nav className="mt-4">
                {menuItems.map((item, index) => renderMenuItem(item, index))}
            </nav>
        </div>
    );
};

export default Sidebar;