import React, { useState, useEffect } from 'react';
import { getAllRoles, getAllModules, getPermissionsByRole, grantPermission, revokePermission } from '../../api/identityService';
import { ShieldCheck, Plus } from 'lucide-react';
import TableSearchFilter from '../../components/common/TableSearchFilter';
import toast from 'react-hot-toast';

const PermissionManager = () => {
    // ─── STATES ───
    const [roles, setRoles] = useState([]);
    const [modules, setModules] = useState([]);
    const [selectedRoleId, setSelectedRoleId] = useState('');
    const [permissions, setPermissions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');

    // Add Permission Form
    const [permModuleId, setPermModuleId] = useState('');
    const [permName, setPermName] = useState('');
    const [permEndpoint, setPermEndpoint] = useState('');
    const [permMethod, setPermMethod] = useState('GET');

    // ─── FETCH DATA ───
    const fetchBaseData = async () => {
        try {
            const rolesRes = await getAllRoles();
            const modulesRes = await getAllModules();
            setRoles((rolesRes.data || []).filter(r => !r.isSystemRole));
            setModules(modulesRes.data || []);
        } catch (err) {
            console.error("Failed to fetch data", err);
        } finally {
            setLoading(false);
        }
    };

    const fetchPermissions = async (roleId) => {
        if (!roleId) { setPermissions([]); return; }
        try {
            const res = await getPermissionsByRole(roleId);
            setPermissions(res.data || []);
        } catch (err) {
            console.error("Failed to fetch permissions", err);
        }
    };

    useEffect(() => { fetchBaseData(); }, []);
    useEffect(() => { fetchPermissions(selectedRoleId); }, [selectedRoleId]);

    // ─── HANDLERS ───
    const handleGrantPermission = async (e) => {
        e.preventDefault();
        if (!selectedRoleId || !permModuleId || !permName.trim() || !permEndpoint.trim()) {
            toast.error("All fields are required!");
            return;
        }
        // Ensure endpoint starts with /api/
        let endpoint = permEndpoint.trim();
        if (!endpoint.startsWith('/')) endpoint = '/' + endpoint;

        try {
            await grantPermission({
                roleId: selectedRoleId,
                moduleId: permModuleId,
                permissionName: permName,
                apiEndpoint: endpoint,
                httpMethod: permMethod,
                isGranted: true
            });
            toast.success("Permission granted!");
            setPermName('');
            setPermEndpoint('');
            fetchPermissions(selectedRoleId);
        } catch (err) {
            toast.error("Failed to grant permission");
        }
    };

    const handleToggle = async (perm) => {
        try {
            if (perm.isGranted) {
                await revokePermission(perm.rolePermissionId);
                toast.success(`"${perm.permissionName}" revoked`);
            } else {
                await grantPermission({
                    roleId: selectedRoleId,
                    moduleId: perm.moduleId,
                    permissionName: perm.permissionName,
                    apiEndpoint: perm.apiEndpoint,
                    httpMethod: perm.httpMethod,
                    isGranted: true
                });
                toast.success(`"${perm.permissionName}" granted`);
            }
            fetchPermissions(selectedRoleId);
        } catch (err) {
            toast.error("Failed to toggle permission");
        }
    };

    // ─── HTTP METHOD BADGE ───
    const methodBadge = (method) => {
        const colors = {
            GET: 'bg-green-500/10 text-green-400 border-green-500/20',
            POST: 'bg-blue-500/10 text-blue-400 border-blue-500/20',
            PUT: 'bg-yellow-500/10 text-yellow-400 border-yellow-500/20',
            PATCH: 'bg-orange-500/10 text-orange-400 border-orange-500/20',
            DELETE: 'bg-red-500/10 text-red-400 border-red-500/20',
        };
        return <span className={`px-2 py-0.5 rounded text-xs font-mono border ${colors[method] || 'bg-slate-800 text-slate-400'}`}>{method}</span>;
    };

    if (loading) return <div className="p-6 text-slate-400">Loading...</div>;

    const selectedRoleName = roles.find(r => r.roleId === selectedRoleId)?.roleName || '';

    // Filter permissions by search
    const filtered = permissions.filter(p =>
        p.permissionName?.toLowerCase().includes(search.toLowerCase()) ||
        p.apiEndpoint?.toLowerCase().includes(search.toLowerCase()) ||
        p.moduleName?.toLowerCase().includes(search.toLowerCase())
    );

    // Count stats
    const activeCount = permissions.filter(p => p.isGranted).length;
    const revokedCount = permissions.filter(p => !p.isGranted).length;

    return (
        <div className="p-6 space-y-8">
            {/* Header */}
            <div className="flex items-center gap-3">
                <ShieldCheck className="text-emerald-500" size={28} />
                <h1 className="text-2xl font-bold text-black">Permission Manager</h1>
            </div>

            {/* ═══ SECTION 1: Select Role ═══ */}
            <div className="bg-slate-900 border border-slate-800 rounded-xl p-6">
                <h2 className="text-lg font-semibold text-white mb-4">🔐 Manage Permissions for Role</h2>
                <select
                    className="w-full bg-slate-800 text-white rounded px-3 py-2 border border-slate-700 outline-none focus:border-emerald-500"
                    value={selectedRoleId}
                    onChange={(e) => setSelectedRoleId(e.target.value)}
                >
                    <option value="">-- Select a Role --</option>
                    {roles.map(r => (
                        <option key={r.roleId} value={r.roleId}>{r.roleName}</option>
                    ))}
                </select>
            </div>

            {/* ═══ SECTION 2: Grant + Permissions Table ═══ */}
            {selectedRoleId && (
                <>
                    {/* Grant Permission Form */}
                    <div className="bg-slate-900 border border-slate-800 rounded-xl p-6">
                        <h2 className="text-lg font-semibold text-white mb-4">
                            <Plus size={18} className="inline text-emerald-400 mr-1" />
                            Grant Permission to <span className="text-emerald-400">{selectedRoleName}</span>
                        </h2>
                        <form onSubmit={handleGrantPermission} className="grid grid-cols-5 gap-4 items-end">
                            <div>
                                <label className="block text-xs text-slate-400 mb-1">Module</label>
                                <select className="w-full bg-slate-800 text-white rounded px-3 py-2 border border-slate-700 outline-none"
                                    value={permModuleId} onChange={(e) => setPermModuleId(e.target.value)}>
                                    <option value="">Select</option>
                                    {modules.map(m => <option key={m.moduleId} value={m.moduleId}>{m.moduleName}</option>)}
                                </select>
                            </div>
                            <div>
                                <label className="block text-xs text-slate-400 mb-1">Permission Name</label>
                                <input type="text" className="w-full bg-slate-800 text-white rounded px-3 py-2 border border-slate-700 outline-none"
                                    placeholder="e.g. View Orders" value={permName} onChange={(e) => setPermName(e.target.value)} />
                            </div>
                            <div>
                                <label className="block text-xs text-slate-400 mb-1">API Endpoint</label>
                                <input type="text" className="w-full bg-slate-800 text-white rounded px-3 py-2 border border-slate-700 outline-none"
                                    placeholder="e.g. /api/sales/orders" value={permEndpoint} onChange={(e) => setPermEndpoint(e.target.value)} />
                            </div>
                            <div>
                                <label className="block text-xs text-slate-400 mb-1">HTTP Method</label>
                                <select className="w-full bg-slate-800 text-white rounded px-3 py-2 border border-slate-700 outline-none"
                                    value={permMethod} onChange={(e) => setPermMethod(e.target.value)}>
                                    <option value="GET">GET</option>
                                    <option value="POST">POST</option>
                                    <option value="PUT">PUT</option>
                                    <option value="PATCH">PATCH</option>
                                    <option value="DELETE">DELETE</option>
                                </select>
                            </div>
                            <button type="submit" className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded font-medium">
                                Grant
                            </button>
                        </form>
                    </div>

                    {/* Permissions Table */}
                    <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
                        <div className="px-4 py-3 border-b border-slate-800 flex items-center justify-between">
                            <div className="flex items-center gap-4">
                                <h2 className="text-white font-semibold">
                                    Permissions for <span className="text-emerald-400">{selectedRoleName}</span>
                                </h2>
                                <div className="flex gap-2 text-xs">
                                    <span className="px-2 py-0.5 rounded-full bg-green-500/10 text-green-400 border border-green-500/20">{activeCount} Active</span>
                                    <span className="px-2 py-0.5 rounded-full bg-red-500/10 text-red-400 border border-red-500/20">{revokedCount} Revoked</span>
                                </div>
                            </div>
                            <TableSearchFilter
                                value={search}
                                onChange={setSearch}
                                placeholder="Search permissions..."
                                resultCount={filtered.length}
                                totalCount={permissions.length}
                            />
                        </div>
                        <table className="w-full text-sm text-slate-300">
                            <thead className="bg-slate-800/50 text-xs uppercase text-slate-400">
                                <tr>
                                    <th className="px-4 py-3 text-left">Module</th>
                                    <th className="px-4 py-3 text-left">Permission Name</th>
                                    <th className="px-4 py-3 text-left">API Endpoint</th>
                                    <th className="px-4 py-3 text-center">Method</th>
                                    <th className="px-4 py-3 text-center">Status</th>
                                    <th className="px-4 py-3 text-center">Access</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-slate-800">
                                {filtered.length === 0 ? (
                                    <tr>
                                        <td colSpan="6" className="px-4 py-8 text-center text-slate-500">
                                            {search ? "No permissions match your search." : "No permissions assigned yet. Use the form above to grant access."}
                                        </td>
                                    </tr>
                                ) : (
                                    filtered.map(p => (
                                        <tr key={p.rolePermissionId} className={!p.isGranted ? 'opacity-50' : ''}>
                                            <td className="px-4 py-3 text-white font-medium">{p.moduleName}</td>
                                            <td className="px-4 py-3">{p.permissionName}</td>
                                            <td className="px-4 py-3 font-mono text-xs text-slate-400">{p.apiEndpoint}</td>
                                            <td className="px-4 py-3 text-center">{methodBadge(p.httpMethod)}</td>
                                            <td className="px-4 py-3 text-center">
                                                {p.isGranted ? (
                                                    <span className="px-2 py-0.5 rounded-full text-xs bg-green-500/10 text-green-400 border border-green-500/20">Active</span>
                                                ) : (
                                                    <span className="px-2 py-0.5 rounded-full text-xs bg-red-500/10 text-red-400 border border-red-500/20">Revoked</span>
                                                )}
                                            </td>
                                            <td className="px-4 py-3 text-center">
                                                <button onClick={() => handleToggle(p)}
                                                    className={`px-3 py-1 rounded text-xs font-medium transition-colors ${
                                                        p.isGranted 
                                                        ? 'bg-red-500/10 text-red-400 hover:bg-red-500/20 border border-red-500/20' 
                                                        : 'bg-green-500/10 text-green-400 hover:bg-green-500/20 border border-green-500/20'
                                                    }`}>
                                                    {p.isGranted ? '🔒 Revoke' : '🔓 Grant'}
                                                </button>
                                            </td>
                                        </tr>
                                    ))
                                )}
                            </tbody>
                        </table>
                    </div>
                </>
            )}
        </div>
    );
};

export default PermissionManager;
