import React, { useState, useEffect } from 'react';
import { getAllModules, createModule } from '../../api/identityService';
import { Package, Plus } from 'lucide-react';
import TableSearchFilter from '../../components/common/TableSearchFilter';
import toast from 'react-hot-toast';

const ModuleManagement = () => {
    const [modules, setModules] = useState([]);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');

    // Add Module Form
    const [newModuleName, setNewModuleName] = useState('');
    const [newModuleCode, setNewModuleCode] = useState('');

    const fetchModules = async () => {
        try {
            const res = await getAllModules();
            console.log("Modules Data", res.data);
            setModules(res.data || []);
        } catch (err) {
            console.error("Failed to fetch modules", err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchModules(); }, []);

    const handleCreate = async (e) => {
        e.preventDefault();
        if (!newModuleName.trim() || !newModuleCode.trim()) {
            toast.error("Module Name and Code are required!");
            return;
        }
        try {
            await createModule({
                moduleName: newModuleName,
                moduleCode: newModuleCode.toUpperCase(),
                displayOrder: modules.length + 1
            });
            toast.success("Module created!");
            setNewModuleName('');
            setNewModuleCode('');
            fetchModules();
        } catch (err) {
            toast.error("Failed to create module");
        }
    };

    const filtered = modules.filter(m =>
        m.moduleName.toLowerCase().includes(search.toLowerCase()) ||
        m.moduleCode.toLowerCase().includes(search.toLowerCase())
    );

    if (loading) return <div className="p-6 text-slate-400">Loading modules...</div>;

    return (
        <div className="p-6 space-y-8">
            {/* Header */}
            <div className="flex items-center gap-3">
                <Package className="text-blue-500" size={28} />
                <h1 className="text-2xl font-bold text-black">Module Management</h1>
            </div>

            {/* ═══ Create Module Form ═══ */}
            <div className="bg-slate-900 border border-slate-800 rounded-xl p-6">
                <h2 className="text-lg font-semibold text-white mb-4">📦 Create New Module</h2>
                <form onSubmit={handleCreate} className="flex gap-4 items-end">
                    <div className="flex-1">
                        <label className="block text-xs text-slate-400 mb-1">Module Name</label>
                        <input type="text"
                            className="w-full bg-slate-800 text-white rounded px-3 py-2 border border-slate-700 outline-none focus:border-blue-500"
                            placeholder="e.g. Sales & CRM"
                            value={newModuleName} onChange={(e) => setNewModuleName(e.target.value)} />
                    </div>
                    <div className="flex-1">
                        <label className="block text-xs text-slate-400 mb-1">Module Code</label>
                        <input type="text"
                            className="w-full bg-slate-800 text-white rounded px-3 py-2 border border-slate-700 outline-none focus:border-blue-500"
                            placeholder="e.g. SALES"
                            value={newModuleCode} onChange={(e) => setNewModuleCode(e.target.value)} />
                    </div>
                    <button type="submit" className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded font-medium flex items-center gap-1">
                        <Plus size={16} /> Add Module
                    </button>
                </form>
            </div>

            {/* ═══ Modules List ═══ */}
            <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
                <div className="px-4 py-3 border-b border-slate-800 flex items-center justify-between">
                    <h2 className="text-white font-semibold">All Modules ({modules.length})</h2>
                    <TableSearchFilter
                        value={search}
                        onChange={setSearch}
                        placeholder="Search modules..."
                        resultCount={filtered.length}
                        totalCount={modules.length}
                    />
                </div>
                <table className="w-full text-sm text-slate-300">
                    <thead className="bg-slate-800/50 text-xs uppercase text-slate-400">
                        <tr>
                            <th className="px-4 py-3 text-left">#</th>
                            <th className="px-4 py-3 text-left">Module Name</th>
                            <th className="px-4 py-3 text-left">Module Code</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-800">
                        {filtered.length === 0 ? (
                            <tr>
                                <td colSpan="4" className="px-4 py-8 text-center text-slate-500">
                                    {search ? "No modules match your search." : "No modules created yet."}
                                </td>
                            </tr>
                        ) : (
                            filtered.map((m, idx) => (
                                <tr key={m.moduleId}>
                                    <td className="px-4 py-3 text-slate-500">{idx + 1}</td>
                                    <td className="px-4 py-3 text-white font-medium">{m.moduleName}</td>
                                    <td className="px-4 py-3">
                                        <span className="px-2 py-0.5 bg-blue-500/10 text-blue-400 rounded text-xs font-mono border border-blue-500/20">
                                            {m.moduleCode}
                                        </span>
                                    </td>
                                    {/* <td className="px-4 py-3 text-center text-slate-400">{m.displayOrder}</td> */}
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default ModuleManagement;
