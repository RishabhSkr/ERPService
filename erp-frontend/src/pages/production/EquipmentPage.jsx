import React, { useState, useEffect, useCallback } from 'react';
import { Wrench, Plus, Edit2, Trash2, X, RefreshCw, Save, Link2, Loader } from 'lucide-react';
import toast from 'react-hot-toast';
import {
    getEquipment, createEquipment, updateEquipment, deleteEquipment,
    getWorkCenters, getProcesses, linkProcesses, getLinkedProcesses
} from '../../api/productionService';
import SearchSelect from '../../components/common/SearchSelect';

/**
 * Equipment CRUD + Link Processes
 * 
 * CreateEquipmentDto: { equipmentCode, equipmentName, workCenterId, manufacturer, model, costPerHour }
 * EquipmentDto: { equipmentId, equipmentCode, equipmentName, workCenterId, workCenterCode, workCenterName,
 *                 manufacturer, model, status, costPerHour, isActive, linkedProcesses[] }
 * LinkProcessesDto: { processIds: [guid] }
 */
const EquipmentPage = () => {
    const [items, setItems] = useState([]);
    const [workCenters, setWorkCenters] = useState([]);
    const [processes, setProcesses] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showForm, setShowForm] = useState(false);
    const [editItem, setEditItem] = useState(null);
    const [formData, setFormData] = useState({});
    // Link Processes modal
    const [linkEquipment, setLinkEquipment] = useState(null); // equipment being linked
    const [selectedProcessIds, setSelectedProcessIds] = useState([]);
    const [linkLoading, setLinkLoading] = useState(false);

    const extractData = (res) => {
        const d = res.data?.data;
        return Array.isArray(d) ? d : (d?.data || d || []);
    };

    const load = useCallback(async () => {
        setLoading(true);
        try {
            const [eqRes, wcRes, prRes] = await Promise.all([
                getEquipment(), getWorkCenters(), getProcesses()
            ]);
            setItems(extractData(eqRes));
            setWorkCenters(extractData(wcRes));
            setProcesses(extractData(prRes));
        } catch (err) {
            toast.error('Failed to load equipment');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { load(); }, [load]);

    const emptyForm = () => ({
        equipmentCode: '', equipmentName: '', workCenterId: '',
        manufacturer: '', model: '', costPerHour: '',
    });

    const openCreate = () => { setEditItem(null); setFormData(emptyForm()); setShowForm(true); };
    const openEdit = (item) => {
        setEditItem(item);
        setFormData({
            equipmentCode: item.equipmentCode || '',
            equipmentName: item.equipmentName || '',
            workCenterId: item.workCenterId || '',
            manufacturer: item.manufacturer || '',
            model: item.model || '',
            costPerHour: item.costPerHour || '',
        });
        setShowForm(true);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const payload = { ...formData, costPerHour: parseFloat(formData.costPerHour) || 0 };
            if (editItem) {
                await updateEquipment(editItem.equipmentId, payload);
                toast.success('Equipment updated!');
            } else {
                await createEquipment(payload);
                toast.success('Equipment created!');
            }
            setShowForm(false);
            load();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Failed');
        }
    };

    const handleDelete = async (item) => {
        if (!confirm(`Deactivate "${item.equipmentName}"?`)) return;
        try {
            await deleteEquipment(item.equipmentId);
            toast.success('Equipment deactivated!');
            load();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Failed to deactivate');
        }
    };

    // ─── Link Processes ───
    const openLinkProcesses = async (item) => {
        setLinkEquipment(item);
        setLinkLoading(true);
        try {
            const res = await getLinkedProcesses(item.equipmentId);
            const linked = extractData(res);
            setSelectedProcessIds(linked.map(p => p.processId));
        } catch {
            setSelectedProcessIds((item.linkedProcesses || []).map(p => p.processId));
        }
        setLinkLoading(false);
    };

    const toggleProcess = (pid) => {
        setSelectedProcessIds(prev =>
            prev.includes(pid) ? prev.filter(id => id !== pid) : [...prev, pid]
        );
    };

    const handleLinkSubmit = async () => {
        try {
            await linkProcesses(linkEquipment.equipmentId, { processIds: selectedProcessIds });
            toast.success('Processes linked!');
            setLinkEquipment(null);
            load();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Failed to link');
        }
    };

    const getStatusColor = (s) => {
        switch (s) {
            case 'Active': case 'Available': return 'bg-green-100 text-green-700';
            case 'InUse': return 'bg-blue-100 text-blue-700';
            case 'Maintenance': return 'bg-orange-100 text-orange-700';
            default: return 'bg-gray-100 text-gray-700';
        }
    };

    return (
        <div className="p-6">
            {/* Header */}
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <Wrench className="text-blue-500" size={28} />
                    <h1 className="text-2xl font-bold text-slate-800">Equipment</h1>
                    <span className="text-sm text-slate-400">({items.length})</span>
                </div>
                <div className="flex gap-3">
                    <button onClick={load} className="flex items-center gap-2 px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">
                        <RefreshCw size={16} /> Refresh
                    </button>
                    <button onClick={openCreate} className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium">
                        <Plus size={16} /> Add Equipment
                    </button>
                </div>
            </div>

            {/* Table */}
            <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
                <table className="w-full">
                    <thead className="bg-slate-50 border-b border-slate-200">
                        <tr>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Code</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Name</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Work Center</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Manufacturer</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Model</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Cost/Hr</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Status</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Linked Processes</th>
                            <th className="text-center px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                        {loading ? (
                            <tr><td colSpan="9" className="text-center py-10 text-slate-400">Loading...</td></tr>
                        ) : items.length === 0 ? (
                            <tr><td colSpan="9" className="text-center py-10 text-slate-400">No equipment found</td></tr>
                        ) : items.map(item => (
                            <tr key={item.equipmentId} className="hover:bg-slate-50 transition-colors">
                                <td className="px-4 py-3 text-sm font-mono font-semibold text-slate-700">{item.equipmentCode}</td>
                                <td className="px-4 py-3 text-sm font-medium text-slate-700">{item.equipmentName}</td>
                                <td className="px-4 py-3 text-sm text-slate-600">
                                    {item.workCenterName}
                                    <div className="text-xs text-slate-400">{item.workCenterCode}</div>
                                </td>
                                <td className="px-4 py-3 text-sm text-slate-600">{item.manufacturer || '-'}</td>
                                <td className="px-4 py-3 text-sm text-slate-600">{item.model || '-'}</td>
                                <td className="px-4 py-3 text-sm text-slate-600">₹{item.costPerHour?.toLocaleString('en-IN')}</td>
                                <td className="px-4 py-3 text-sm">
                                    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${getStatusColor(item.status)}`}>{item.status}</span>
                                </td>
                                <td className="px-4 py-3 text-sm">
                                    <div className="flex flex-wrap gap-1">
                                        {(item.linkedProcesses || []).length === 0 ? (
                                            <span className="text-slate-400 text-xs">None</span>
                                        ) : (item.linkedProcesses || []).map(p => (
                                            <span key={p.processId} className="bg-indigo-100 text-indigo-700 px-1.5 py-0.5 rounded text-xs">{p.processCode}</span>
                                        ))}
                                    </div>
                                </td>
                                <td className="px-4 py-3 text-center">
                                    <div className="flex items-center justify-center gap-1">
                                        <button onClick={() => openLinkProcesses(item)} className="p-1.5 rounded hover:bg-indigo-50 text-slate-500 hover:text-indigo-600" title="Link Processes">
                                            <Link2 size={15} />
                                        </button>
                                        <button onClick={() => openEdit(item)} className="p-1.5 rounded hover:bg-blue-50 text-slate-500 hover:text-blue-600" title="Edit">
                                            <Edit2 size={15} />
                                        </button>
                                        <button onClick={() => handleDelete(item)} className="p-1.5 rounded hover:bg-red-50 text-slate-500 hover:text-red-600" title="Deactivate">
                                            <Trash2 size={15} />
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {/* ─── Create/Edit Form Modal ─── */}
            {showForm && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg">
                        <div className="flex items-center justify-between p-5 border-b">
                            <h2 className="text-lg font-bold text-slate-800">{editItem ? 'Edit' : 'Create'} Equipment</h2>
                            <button onClick={() => setShowForm(false)} className="p-1.5 rounded-lg hover:bg-slate-100"><X size={20} /></button>
                        </div>
                        <form onSubmit={handleSubmit} className="p-5 space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Equipment Code *</label>
                                    <input type="text" value={formData.equipmentCode} onChange={(e) => setFormData({...formData, equipmentCode: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" required />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Equipment Name *</label>
                                    <input type="text" value={formData.equipmentName} onChange={(e) => setFormData({...formData, equipmentName: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" required />
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-1">Work Center *</label>
                                <SearchSelect
                                    value={formData.workCenterId}
                                    displayValue={(() => { const wc = workCenters.find(w => w.workCenterId === formData.workCenterId); return wc ? `${wc.centerName} (${wc.centerCode})` : ''; })()}
                                    placeholder="Search work center..."
                                    items={workCenters}
                                    title="Select Work Center"
                                    displayFields={[
                                        { key: 'centerCode', label: 'Code', width: '30%', bold: true },
                                        { key: 'centerName', label: 'Name', width: '70%' },
                                    ]}
                                    searchKeys={['centerCode', 'centerName']}
                                    valueKey="workCenterId"
                                    onSelect={(wc) => setFormData({...formData, workCenterId: wc.workCenterId})}
                                    required
                                />
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Manufacturer</label>
                                    <input type="text" value={formData.manufacturer} onChange={(e) => setFormData({...formData, manufacturer: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Model</label>
                                    <input type="text" value={formData.model} onChange={(e) => setFormData({...formData, model: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" />
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-1">Cost Per Hour (₹) *</label>
                                <input type="number" step="any" value={formData.costPerHour} onChange={(e) => setFormData({...formData, costPerHour: e.target.value})}
                                    className="w-full px-3 py-2.5 border rounded-lg text-sm" required />
                            </div>
                            <div className="flex justify-end gap-3 pt-2">
                                <button type="button" onClick={() => setShowForm(false)} className="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">Cancel</button>
                                <button type="submit" className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium">
                                    <Save size={16} /> {editItem ? 'Update' : 'Create'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* ─── Link Processes Modal ─── */}
            {linkEquipment && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
                        <div className="flex items-center justify-between p-5 border-b">
                            <h2 className="text-lg font-bold text-slate-800 flex items-center gap-2">
                                <Link2 size={20} className="text-indigo-500" /> Link Processes
                            </h2>
                            <button onClick={() => setLinkEquipment(null)} className="p-1.5 rounded-lg hover:bg-slate-100"><X size={20} /></button>
                        </div>
                        <div className="p-5 space-y-4">
                            <div className="bg-blue-50 rounded-lg p-3 text-sm">
                                <span className="font-bold text-blue-700">{linkEquipment.equipmentCode}</span>
                                <span className="text-slate-500 ml-2">{linkEquipment.equipmentName}</span>
                            </div>
                            <p className="text-xs text-slate-500">Select which processes this equipment can perform:</p>
                            {linkLoading ? (
                                <div className="flex items-center gap-2 text-slate-400 text-sm py-4 justify-center">
                                    <Loader size={16} className="animate-spin" /> Loading...
                                </div>
                            ) : (
                                <div className="space-y-2 max-h-64 overflow-y-auto">
                                    {processes.map(p => (
                                        <label key={p.processId} className="flex items-center gap-3 p-2 rounded-lg hover:bg-slate-50 cursor-pointer">
                                            <input type="checkbox"
                                                checked={selectedProcessIds.includes(p.processId)}
                                                onChange={() => toggleProcess(p.processId)}
                                                className="w-4 h-4 accent-indigo-600"
                                            />
                                            <div>
                                                <span className="font-medium text-sm text-slate-700">{p.processCode}</span>
                                                <span className="text-sm text-slate-500 ml-2">{p.processName}</span>
                                                <span className={`ml-2 px-1.5 py-0.5 rounded text-xs ${
                                                    p.category === 'Raw' ? 'bg-orange-100 text-orange-700' :
                                                    p.category === 'Assembly' ? 'bg-blue-100 text-blue-700' :
                                                    'bg-green-100 text-green-700'
                                                }`}>{p.category}</span>
                                            </div>
                                        </label>
                                    ))}
                                    {processes.length === 0 && <p className="text-sm text-slate-400 text-center">No processes defined</p>}
                                </div>
                            )}
                            <div className="flex justify-end gap-3 pt-2 border-t">
                                <button onClick={() => setLinkEquipment(null)} className="px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">Cancel</button>
                                <button onClick={handleLinkSubmit} className="flex items-center gap-2 px-5 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm">
                                    <Link2 size={14} /> Save Links ({selectedProcessIds.length})
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default EquipmentPage;
