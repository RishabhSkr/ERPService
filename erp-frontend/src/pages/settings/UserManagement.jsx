import React, { useState, useEffect } from 'react';
import { getAllUsers, approveUser, suspendUser,getAllRoles } from '../../api/identityService';
import { Shield, CheckCircle, XCircle, Clock } from 'lucide-react';
import toast from 'react-hot-toast';
const UserManagement = () => {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [roles,setRoles] = useState([]);
    const [selectedRoles, setSelectedRoles] = useState({}); // Kiska kya role select kiya
    
    // ─── GREEN BELT TASK 1: Fetch Users ───
    // TODO: Use useEffect to call getAllUsers() and set the users state.
    
    const fetchUsers = async () => {
        try {
            const usersData = await getAllUsers();
            const rolesResp = await getAllRoles();
            const rolesData = rolesResp.data || [];
            
            // Map RequestedRoleName manually!
            const mappedUsers = usersData.map(user => {
                const requestedRole = rolesData.find(r => r.roleId === user.requestedRoleId);
                return {
                    ...user,
                    requestedRoleName: requestedRole ? requestedRole.roleName : 'None'
                };
            });

            console.log("Roles Data", rolesData);
            setUsers(mappedUsers);
            setRoles(rolesData);
        } catch (err) {
            console.error("Failed to fetch users", err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(()=>{
        fetchUsers();
    },[])

    // Make sure to handle the loading state properly!
    
    // ─── GREEN BELT TASK 2: Render Badges ───
    const getStatusBadge = (status) => {
        // TODO: Return different JSX spans based on status (Pending, Active, Suspended)
        switch(status){
            case "Pending":
                return <span className="px-2 py-1 bg-yellow-500/10 text-yellow-500">{status}</span>;
            case "Active":
                return <span className="px-2 py-1 bg-green-500/10 text-green-500">{status}</span>;
            case "Suspended":
                return <span className="px-2 py-1 bg-red-500/10 text-red-500">{status}</span>;
            default:
                return <span className="px-2 py-1 bg-slate-800 rounded-full text-xs">{status}</span>;
        }
    };

    // ─── GREEN BELT TASK 4: Handle Actions ───
    const handleApprove = async (user) => {
        try{
            if(user.roleName === 'SuperAdmin' || user.isSystemRole === 'true'){
                toast.error("System Admin cannot be approved/modified");
                return;
            }
           
            const roleToAssign = selectedRoles[user.userId] || user.requestedRoleId;
            if (!roleToAssign) {
                toast.error("Please select a role from the dropdown first!");
                return;
            }
            await approveUser(user.userId, roleToAssign);
            toast.success("User approved successfully!");
            fetchUsers();
        }
        catch(err){
            console.error("Failed to approve user", err);
            toast.error("Failed to approve user");
        }
    };
    
    const handleSuspend = async (user) => {
        try{
            if(user.roleName === 'SuperAdmin' || user.isSystemRole === 'true'){
                toast.error("System Admin cannot be suspended");
                return;
            }
            await suspendUser(user.userId);
            toast.success("User suspended successfully!");
            fetchUsers();
        }
        catch(err){
            console.error("Failed to suspend user", err);
            toast.error("Failed to suspend user");
        }
    };
    
    return (
        <div className="p-6">
            <div className="flex items-center gap-3 mb-6">
                <Shield className="text-blue-500" size={28} />
                <h1 className="text-2xl font-bold text-black">User Management</h1>
            </div>

            <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
                <table className="w-full text-left text-sm text-slate-300">
                    <thead className="bg-slate-800/50 text-xs uppercase text-slate-400">
                        <tr>
                            <th className="px-4 py-3">Username</th>
                            <th className="px-4 py-3">Email</th>
                            <th className="px-4 py-3">Role</th>
                            <th className="px-4 py-3">Requested Role</th>
                            <th className="px-4 py-3">Status</th>
                            <th className="px-4 py-3">Select Role</th>
                            <th className="px-4 py-3 text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-800">
                        {/* ─── GREEN BELT TASK 3: Render Users ─── */}
                        {/* TODO: Map over the `users` array and render a <tr> for each user. */}
                        {users.map((user) => (
                            <tr key={user.id}>
                                <td className="px-4 py-3">{user.username}</td>
                                <td className="px-4 py-3">{user.email}</td>
                                <td className="px-4 py-3">{user.roleName}</td>
                                <td className="px-4 py-3">{user.requestedRoleName}</td>
                                <td className="px-4 py-3">{getStatusBadge(user.status)}</td>
                                <td className="px-4 py-3">
                                    {user.status === 'Pending' ? (
                                        <select 
                                            className="bg-slate-800 text-slate-300 px-2 py-1 rounded border border-slate-700 outline-none"
                                            value={selectedRoles[user.userId] || user.requestedRoleId || ''}
                                            onChange={(e) => setSelectedRoles({...selectedRoles, [user.userId]: e.target.value})}
                                        >
                                            <option value="">-- Select Role --</option>
                                            {roles.filter(r => !r.isSystemRole).map(role => (
                                                <option key={role.roleId} value={role.roleId}>{role.roleName}</option>
                                            ))}
                                        </select>
                                    ) : (
                                        user.roleName // Agar pehle se approved hai toh role ka naam dikhao
                                    )}
                                </td>

                                <td className="px-4 py-3 text-right">
                                    <button 
                                        className={`px-2 py-1 text-white rounded-md mr-1 ${user.roleName === 'SuperAdmin' || user.status === 'Active' ? 'bg-slate-700 cursor-not-allowed text-slate-500' : 'bg-green-500 hover:bg-green-600'}`}
                                        onClick={() => handleApprove(user)}
                                        disabled={user.roleName === 'SuperAdmin' || user.status === 'Active'}
                                    >
                                        Approve
                                    </button>
                                    <button 
                                        className={`px-2 py-1 text-white rounded-md ${user.roleName === 'SuperAdmin' || user.status === 'Suspended' ? 'bg-slate-700 cursor-not-allowed text-slate-500' : 'bg-red-500 hover:bg-red-600'}`}
                                        onClick={() => handleSuspend(user)}
                                        disabled={user.roleName === 'SuperAdmin' || user.status === 'Suspended'}
                                    >
                                        Suspend
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

export default UserManagement;
