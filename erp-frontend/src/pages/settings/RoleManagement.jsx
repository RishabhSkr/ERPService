import React, { useState, useEffect } from 'react';
import { getAllRoles, createRole, deleteRole } from '../../api/identityService';
import { ShieldAlert, Plus, Trash2, Lock } from 'lucide-react';
import toast from 'react-hot-toast';

const RoleManagement = () => {
    const [roles, setRoles] = useState([]);
    const [loading, setLoading] = useState(true);

    // Form states
    const [newRoleName, setNewRoleName] = useState('');
    const [newRoleDesc, setNewRoleDesc] = useState('');

    const fetchRoles = async () => {
        try {
            const res = await getAllRoles();
            console.log("Roles fetched successfully", res);
            setRoles(res.data);
        } catch (err) {
            console.error("Failed to fetch roles", err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchRoles();
    }, []);

    // ─── BLUE BELT TASK 1: Create Role ───
    const handleCreateRole = async (e) => {
        e.preventDefault();
        if (!newRoleName.trim()) {
            toast.error("Role Name is required!");
            return;
        }

        try {
            await createRole({ roleName: newRoleName, description: newRoleDesc });
            toast.success("Role created successfully!");
            setNewRoleName('');
            setNewRoleDesc('');
            fetchRoles();
        } catch (err) {
            console.error("Failed to create role", err);
            toast.error(err.response?.data?.message || "Failed to create role");
        }
    };

    // ─── BLUE BELT TASK 2: Delete Role ───
    const handleDeleteRole = async (role) => {
        if (role.isSystemRole) {
            toast.error("You cannot delete a System Role!");
            return;
        }

        const confirmDelete = window.confirm(`Are you sure you want to delete the '${role.roleName}' role?`);
        if (!confirmDelete) return;

        try {
            await deleteRole(role.roleId);
            toast.success("Role deleted successfully!");
            fetchRoles();
        } catch (err) {
            console.error("Failed to delete role", err);
            toast.error(err.response?.data?.message || "Failed to delete role");
        }
    };

    return (
        <div className="p-6">
            <div className="flex items-center gap-3 mb-6">
                <ShieldAlert className="text-purple-500" size={28} />
                <h1 className="text-2xl font-bold text-black">Role Management</h1>
            </div>

            {/* Create Role Form */}
            <div className="bg-slate-900 border border-slate-800 rounded-xl p-6 mb-8">
                <h2 className="text-lg font-semibold text-black mb-4">Create New Role</h2>
                <form onSubmit={handleCreateRole} className="flex gap-4 items-end">
                    <div className="flex-1">
                        <label className="block text-xs text-slate-400 mb-1">Role Name</label>
                        <input 
                            type="text" 
                            className="w-full bg-slate-800 text-white rounded px-3 py-2 border border-slate-700 outline-none focus:border-purple-500"
                            placeholder="e.g. SalesManager"
                            value={newRoleName}
                            onChange={(e) => setNewRoleName(e.target.value)}
                        />
                    </div>
                    <div className="flex-1">
                        <label className="block text-xs text-slate-400 mb-1">Description</label>
                        <input 
                            type="text" 
                            className="w-full bg-slate-800 text-white rounded px-3 py-2 border border-slate-700 outline-none focus:border-purple-500"
                            placeholder="e.g. Can manage all sales orders"
                            value={newRoleDesc}
                            onChange={(e) => setNewRoleDesc(e.target.value)}
                        />
                    </div>
                    <button 
                        type="submit"
                        className="bg-purple-600 hover:bg-purple-700 text-white px-4 py-2 rounded font-medium flex items-center gap-2"
                    >
                        <Plus size={18} /> Create Role
                    </button>
                </form>
            </div>

            {/* Roles Table */}
            <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
                <table className="w-full text-left text-sm text-slate-300">
                    <thead className="bg-slate-800/50 text-xs uppercase text-slate-400">
                        <tr>
                            <th className="px-4 py-3">Role Name</th>
                            <th className="px-4 py-3">Description</th>
                            <th className="px-4 py-3">Type</th>
                            <th className="px-4 py-3">UserCount</th>
                            <th className="px-4 py-3">PermissionsCount</th>
                            <th className="px-4 py-3 text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-800">
                        {/* ─── BLUE BELT TASK 3: Render Roles ─── */}
                        {/* TODO: Map over the `roles` array. */}
                        {roles.map(role => (
                            <tr key={role.roleId}>
                                <td className="px-4 py-3 font-medium text-white">{role.roleName}</td>
                                <td className="px-4 py-3">{role.description}</td>
                                <td className="px-4 py-3">
                                    {/* TODO: If role.isSystemRole is true, show a 🔒 System badge, otherwise show a normal badge */}
                             
                                    {role.isSystemRole ? (
                                        <span className="px-2 py-1 bg-purple-500/10 text-purple-400 rounded-full text-xs border border-purple-500/20 flex items-center w-max gap-1">
                                            <Lock size={12} /> System
                                        </span>
                                    ) : (
                                        <span className="px-2 py-1 bg-slate-800 text-slate-400 rounded-full text-xs">Custom</span>
                                    )}
                                </td>
                                <td className="px-4 py-3 text-center">{role.userCount}</td>
                                <td className="px-4 py-3 text-center">{role.permissionCount}</td>
                                <td className="px-4 py-3 text-right">
                                    {/* TODO: If role.isSystemRole is true, disable this button! */}
                                    <button 
                                        onClick={() => handleDeleteRole(role)}
                                        className={`px-2 py-1 text-white rounded-md flex items-center ml-auto gap-1 ${role.isSystemRole ? 'bg-slate-700 cursor-not-allowed text-slate-500' : 'bg-red-500 hover:bg-red-600'}`}
                                        disabled={role.isSystemRole}
                                    >
                                        <Trash2 size={14} /> Delete
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default RoleManagement;
