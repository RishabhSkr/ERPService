import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { updateUser } from '../../api/identityService';
import { getMyProfile, updateMyProfile,getPublicRoles} from '../../api/authService';
import { User, Mail, Shield, Clock, Save, CheckCircle, AlertCircle } from 'lucide-react';
import toast from 'react-hot-toast';

const UserProfile = () => {
    const { user } = useAuth();
    const [profile, setProfile] = useState(null);
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    // Editable fields
    const [email, setEmail] = useState('');

    const fetchProfile = async () => {
        if (!user?.id) return;
        try {
            const res = await getMyProfile(localStorage.getItem('erp_token'));
            const data = res.data || res;
            const rolesRes = await getPublicRoles();
            const roles = rolesRes.data || [];
            console.log(roles); 
            const role = roles.find(r => r.roleId === data.requestedRoleId);
            if (role) {
                data.requestedRoleName = role.roleName;
            }
            setProfile(data);
            setEmail(data.email || '');
        } catch (err) {
            console.error("Failed to fetch profile", err);
            toast.error("Failed to load profile");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchProfile(); }, [user?.id]);

    const handleSave = async (e) => {
        e.preventDefault();
        if (!email.trim()) {
            toast.error("Email is required!");
            return;
        }
        setSaving(true);
        try {
            await updateMyProfile(localStorage.getItem('erp_token'), { email });
            toast.success("Profile updated successfully!");
            fetchProfile();
        } catch (err) {
            toast.error("Failed to update profile");
        } finally {
            setSaving(false);
        }
    };

    if (loading) return <div className="p-6 text-slate-400">Loading profile...</div>;

    const statusBadge = (isActive) => {
        if (isActive) return <span className="px-2 py-0.5 rounded-full text-xs bg-green-500/10 text-green-400 border border-green-500/20">Active</span>;
        return <span className="px-2 py-0.5 rounded-full text-xs bg-red-500/10 text-red-400 border border-red-500/20">Suspended</span>;
    };

    return (
        <div className="p-6 max-w-3xl mx-auto space-y-8">
            {/* Header */}
            <div className="flex items-center gap-3">
                <User className="text-blue-500" size={28} />
                <h1 className="text-2xl font-bold text-black">My Profile</h1>
            </div>

            {/* ═══ Profile Card ═══ */}
            <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
                {/* Profile Header */}
                <div className="bg-gradient-to-r from-blue-600/20 to-purple-600/20 px-6 py-8 flex items-center gap-5">
                    <div className="w-20 h-20 rounded-full bg-blue-600 flex items-center justify-center text-3xl font-bold text-white shadow-lg">
                        {profile?.username?.charAt(0).toUpperCase() || 'U'}
                    </div>
                    <div>
                        <h2 className="text-2xl font-bold text-white">{profile?.username}</h2>
                        <div className="flex items-center gap-3 mt-1">
                            <span className="px-2 py-0.5 bg-blue-500/10 text-blue-400 rounded text-xs font-medium border border-blue-500/20">
                                {profile?.roleName || user?.roleName || 'User'}
                            </span>
                            {statusBadge(profile?.isActive)}
                        </div>
                    </div>
                </div>

                {/* Profile Info */}
                <div className="px-6 py-6 space-y-6">
                    {/* Read-only Info */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="flex items-start gap-3">
                            <User size={18} className="text-slate-500 mt-0.5" />
                            <div>
                                <div className="text-xs text-slate-500 uppercase tracking-wider">Username</div>
                                <div className="text-white font-medium">{profile?.username}</div>
                            </div>
                        </div>
                        <div className="flex items-start gap-3">
                            <Shield size={18} className="text-slate-500 mt-0.5" />
                            <div>
                                <div className="text-xs text-slate-500 uppercase tracking-wider">Role</div>
                                <div className="text-white font-medium">{profile?.roleName || user?.roleName}</div>
                            </div>
                        </div>
                        <div className="flex items-start gap-3">
                            <CheckCircle size={18} className="text-slate-500 mt-0.5" />
                            <div>
                                <div className="text-xs text-slate-500 uppercase tracking-wider">Status</div>
                                <div className="mt-0.5">{statusBadge(profile?.isActive)}</div>
                            </div>
                        </div>
                        <div className="flex items-start gap-3">
                            <Clock size={18} className="text-slate-500 mt-0.5" />
                            <div>
                                <div className="text-xs text-slate-500 uppercase tracking-wider">User ID</div>
                                <div className="text-slate-400 font-mono text-xs">{profile?.userId?.slice(0, 8) || user?.id?.slice(0, 8)}...</div>
                            </div>
                        </div>
                        <div className="flex items-start gap-3">
                            <Clock size={18} className="text-slate-500 mt-0.5" />
                            <div>
                                <div className="text-xs text-slate-500 uppercase tracking-wider">Requested Role</div>
                                <div className="text-slate-400 text-sm">{profile?.requestedRoleName || 'No Role'}</div>
                            </div>
                        </div>
                        <div className="flex items-start gap-3">
                            <Clock size={18} className="text-slate-500 mt-0.5" />
                            <div>
                                <div className="text-xs text-slate-500 uppercase tracking-wider">Created Date</div>
                                <div className="text-slate-400 text-sm">{profile?.createdAt ? new Date(profile.createdAt).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }) : '-'}</div>
                            </div>
                        </div>
                    </div>

                    {/* Divider */}
                    <div className="border-t border-slate-800" />

                    {/* Editable Fields */}
                    <form onSubmit={handleSave} className="space-y-4">
                        <h3 className="text-white font-semibold flex items-center gap-2">
                            <Mail size={16} className="text-blue-400" /> Edit Details
                        </h3>
                        <div>
                            <label className="block text-xs text-slate-400 mb-1">Email Address</label>
                            <input 
                                type="email"
                                className="w-full bg-slate-800 text-white rounded-lg px-3 py-2.5 border border-slate-700 outline-none focus:border-blue-500 transition-colors"
                                placeholder="your@email.com"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                            />
                        </div>
                        <div className="flex justify-end">
                            <button 
                                type="submit" 
                                disabled={saving}
                                className="bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white px-5 py-2 rounded-lg font-medium flex items-center gap-2 transition-colors"
                            >
                                <Save size={16} /> {saving ? 'Saving...' : 'Save Changes'}
                            </button>
                        </div>
                    </form>
                </div>
            </div>

            {/* ═══ Account Info Card ═══ */}
            <div className="bg-slate-900 border border-slate-800 rounded-xl p-6">
                <h3 className="text-white font-semibold mb-4 flex items-center gap-2">
                    <AlertCircle size={16} className="text-yellow-400" /> Account Information
                </h3>
                <div className="text-sm text-slate-400 space-y-2">
                    <p>• Your username and role can only be changed by an administrator.</p>
                    <p>• Contact your system administrator to request role changes or account modifications.</p>
                    <p>• For security reasons, password changes require admin approval.</p>
                </div>
            </div>
        </div>
    );
};

export default UserProfile;
