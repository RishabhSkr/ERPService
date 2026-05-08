import { useState } from 'react';
import { Link } from 'react-router-dom';
import { LogOut, User, Settings, ChevronUp } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

const UserProfileDropdown = () => {
    const [isOpen, setIsOpen] = useState(false);
    const { user, logout } = useAuth();

    // If user is not loaded yet, don't crash
    if (!user) return null;

    return (
        <div className="relative mt-auto">
            {/* The Dropdown Menu (Appears above the button) */}
            {isOpen && (
                <div className="absolute bottom-full left-4 right-4 mb-2 bg-slate-800 border border-slate-700 rounded-xl shadow-xl overflow-hidden animate-in slide-in-from-bottom-2">
                    <div className="px-4 py-3 border-b border-slate-700">
                        <p className="text-sm text-white font-medium truncate">{user.username}</p>
                        <p className="text-xs text-slate-400 truncate">{user.roleName || 'No Role'}</p>
                    </div>
                    
                    <div className="p-1">
                        <Link 
                            to="/app/profile"
                            className="w-full flex items-center gap-2 px-3 py-2 text-sm text-slate-300 hover:bg-slate-700 hover:text-white rounded-lg transition-colors"
                            onClick={() => setIsOpen(false)}
                        >
                            <User size={16} />
                            <span>Edit Profile</span>
                        </Link>
                        
                        <button 
                            className="w-full flex items-center gap-2 px-3 py-2 text-sm text-red-400 hover:bg-red-500/10 hover:text-red-300 rounded-lg transition-colors mt-1"
                            onClick={() => { logout(); window.location.href = '/'; }}
                        >
                            <LogOut size={16} />
                            <span>Logout</span>
                        </button>
                    </div>
                </div>
            )}

            {/* The Trigger Button at the bottom of sidebar */}
            <button
                onClick={() => setIsOpen(!isOpen)}
                className={`w-full flex items-center justify-between p-4 border-t border-slate-800 hover:bg-slate-800/50 transition-colors ${isOpen ? 'bg-slate-800/50' : ''}`}
            >
                <div className="flex items-center gap-3 overflow-hidden">
                    <div className="w-8 h-8 rounded-full bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center shrink-0">
                        <span className="text-sm font-bold text-white">
                            {user.username?.charAt(0).toUpperCase()}
                        </span>
                    </div>
                    <div className="text-left overflow-hidden">
                        <p className="text-sm font-medium text-white truncate">{user.username}</p>
                        <p className="text-xs text-slate-400 truncate">{user.roleName || 'Pending'}</p>
                    </div>
                </div>
                <ChevronUp 
                    size={16} 
                    className={`text-slate-400 transition-transform duration-200 ${isOpen ? 'rotate-180' : ''}`} 
                />
            </button>
        </div>
    );
};

export default UserProfileDropdown;
