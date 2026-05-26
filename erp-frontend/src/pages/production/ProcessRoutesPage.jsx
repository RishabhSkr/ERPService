import React, { useState, useEffect, useCallback } from 'react';
import { Route as RouteIcon, Plus, Edit2, Trash2, X, RefreshCw, Save, ChevronDown, ChevronRight, Loader } from 'lucide-react';
import toast from 'react-hot-toast';
import {
    getProcessRoutes, createProcessRoute, updateProcessRoute, deleteProcessRoute,
    getProcesses, getWorkCenters, getEquipment
} from '../../api/productionService';
import { getUnits } from '../../api/inventoryService';
import { getProducts } from '../../api/master/product';
import SearchSelect from '../../components/common/SearchSelect';

/**
 * Process Routes — Route → Steps (ordered)
 * Materials removed from steps — now defined in BOM Line (industry standard)
 * 
 * CreateProcessRouteDto: { routeCode, productId, workCenterId, description, steps[] }
 * CreateProcessRouteStepDto: { stepNumber, processId, equipmentId?, setupTimeMinutes, runTimePerUnitMinutes, notes }
 */
const ProcessRoutesPage = () => {
    const [routes, setRoutes] = useState([]);
    const [processes, setProcesses] = useState([]);
    const [workCenters, setWorkCenters] = useState([]);
    const [equipmentList, setEquipmentList] = useState([]);
    const [products, setProducts] = useState([]);
    const [units, setUnits] = useState([]);
    const [loading, setLoading] = useState(true);

    // Form state
    const [showForm, setShowForm] = useState(false);
    const [editRoute, setEditRoute] = useState(null);
    const [formData, setFormData] = useState(emptyForm());

    // Detail view
    const [expandedId, setExpandedId] = useState(null);

    const extractData = (res) => {
        const d = res.data?.data;
        return Array.isArray(d) ? d : (d?.data || d || []);
    };

    const extractProducts = (res) => {
        const d = res?.data;
        return Array.isArray(d) ? d : (d?.data || []);
    };

    const load = useCallback(async () => {
        setLoading(true);
        try {
            const [routeRes, procRes, wcRes, eqRes, prodRes, unitRes] = await Promise.all([
                getProcessRoutes(), getProcesses(), getWorkCenters(), getEquipment(),
                getProducts(), getUnits()
            ]);
            console.log('Route Res', routeRes);
            setRoutes(extractData(routeRes));
            setProcesses(extractData(procRes));
            setWorkCenters(extractData(wcRes));
            setEquipmentList(extractData(eqRes));
            setProducts(extractProducts(prodRes));
            setUnits(extractData(unitRes));
        } catch (err) {
            toast.error('Failed to load data');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { load(); }, [load]);

    function emptyForm() {
        return {
            routeCode: '', productId: '', workCenterId: '', description: '',
            steps: [emptyStep(1)],
        };
    }

    function emptyStep(num) {
        return {
            stepNumber: num, processId: '', equipmentId: '',
            setupTimeMinutes: '', runTimePerUnitMinutes: '', notes: '',
            outputMultiplier: '1', outputUnit: 'pcs'
        };
    }

    const openCreate = () => { setEditRoute(null); setFormData(emptyForm()); setShowForm(true); };
    const openEdit = (route) => {
        setEditRoute(route);
        setFormData({
            routeCode: route.routeCode,
            productId: route.productId,
            workCenterId: route.workCenterId || '',
            description: route.description || '',
            steps: (route.steps || []).map(s => ({
                processRouteStepId: s.processRouteStepId,
                stepNumber: s.stepNumber,
                processId: s.processId || '',
                equipmentId: s.equipmentId || '',
                setupTimeMinutes: s.setupTimeMinutes,
                runTimePerUnitMinutes: s.runTimePerUnitMinutes,
                notes: s.notes || '',
                outputMultiplier: s.outputMultiplier || '1',
                outputUnit: s.outputUnit || 'pcs'
            })),
        });
        setShowForm(true);
    };

    // Step management
    const addStep = () => {
        setFormData({
            ...formData,
            steps: [...formData.steps, emptyStep(formData.steps.length + 1)]
        });
    };

    const removeStep = (idx) => {
        const newSteps = formData.steps.filter((_, i) => i !== idx).map((s, i) => ({ ...s, stepNumber: i + 1 }));
        setFormData({ ...formData, steps: newSteps });
    };

    const updateStep = (idx, field, value) => {
        const newSteps = [...formData.steps];
        newSteps[idx] = { ...newSteps[idx], [field]: value };
        setFormData({ ...formData, steps: newSteps });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!formData.routeCode || !formData.productId || !formData.workCenterId || formData.steps.length === 0) {
            toast.error('Fill all required fields');
            return;
        }

        const payload = {
            routeCode: formData.routeCode,
            productId: formData.productId,
            workCenterId: formData.workCenterId,
            description: formData.description || null,
            steps: formData.steps.map(s => ({
                processRouteStepId: s.processRouteStepId || null,
                stepNumber: s.stepNumber,
                processId: s.processId,
                equipmentId: s.equipmentId || null,
                setupTimeMinutes: parseInt(s.setupTimeMinutes) || 0,
                runTimePerUnitMinutes: parseInt(s.runTimePerUnitMinutes) || 0,
                notes: s.notes || null,
                outputMultiplier: parseFloat(s.outputMultiplier) || 1.0,
                outputUnit: s.outputUnit || 'pcs'
            })),
        };

        try {
            if (editRoute) {
                await updateProcessRoute(editRoute.processRouteId, payload);
                toast.success('Route updated!');
            } else {
                await createProcessRoute(payload);
                toast.success('Route created!');
            }
            setShowForm(false);
            load();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Failed');
        }
    };

    const getProductName = (pid) => {
        const p = products.find(pr => (pr.id || pr.productId) === pid);
        return p ? `${p.productName || p.name} (${p.productCode || p.code})` : pid;
    };

    const handleDelete = async (route) => {
        const confirmed = confirm(`Deactivate route "${route.routeCode}"? It can be reactivated later.`);
        if (!confirmed) return;
        try {
            await deleteProcessRoute(route.processRouteId);
            toast.success('Route deactivated!');
            load();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Cannot delete route');
        }
    };

    // Helper to get display values for SearchSelect
    const getProductDisplayValue = (pid) => {
        const p = products.find(pr => (pr.id || pr.productId) === pid);
        return p ? `${p.productName || p.name} (${p.productCode || p.code})` : '';
    };
    const getWCDisplayValue = (wcId) => {
        const wc = workCenters.find(w => w.workCenterId === wcId);
        return wc ? `${wc.centerName} (${wc.centerCode})` : '';
    };
    const getProcessDisplayValue = (procId) => {
        const p = processes.find(pr => pr.processId === procId);
        return p ? `${p.processCode} — ${p.processName}` : '';
    };
    const getEquipmentDisplayValue = (eqId) => {
        const eq = equipmentList.find(e => e.equipmentId === eqId);
        return eq ? `${eq.equipmentCode} — ${eq.equipmentName || ''}` : '';
    };

    return (
        <div className="p-6">
            {/* Header */}
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <RouteIcon className="text-blue-500" size={28} />
                    <h1 className="text-2xl font-bold text-slate-800">Process Routes</h1>
                    <span className="text-sm text-slate-400">({routes.length})</span>
                </div>
                <div className="flex gap-3">
                    <button onClick={load} className="flex items-center gap-2 px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">
                        <RefreshCw size={16} /> Refresh
                    </button>
                    <button onClick={openCreate} className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium">
                        <Plus size={16} /> Create Route
                    </button>
                </div>
            </div>

            {/* Table */}
            <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
                <table className="w-full">
                    <thead className="bg-slate-50 border-b">
                        <tr>
                            <th className="w-8"></th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Route Code</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Product</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Work Center</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Version</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Steps</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Status</th>
                            <th className="text-center px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                        {loading ? (
                            <tr><td colSpan="8" className="text-center py-10 text-slate-400"><Loader className="inline animate-spin mr-2" size={16} />Loading...</td></tr>
                        ) : routes.length === 0 ? (
                            <tr><td colSpan="8" className="text-center py-10 text-slate-400">No process routes defined</td></tr>
                        ) : routes.map(route => (
                            <React.Fragment key={route.processRouteId}>
                                <tr className="hover:bg-slate-50 transition-colors cursor-pointer" onClick={() => setExpandedId(expandedId === route.processRouteId ? null : route.processRouteId)}>
                                    <td className="px-2 text-center text-slate-400">
                                        {expandedId === route.processRouteId ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
                                    </td>
                                    <td className="px-4 py-3 text-sm font-mono font-semibold text-slate-700">{route.routeCode}</td>
                                    <td className="px-4 py-3 text-sm text-slate-700">{getProductName(route.productId)}</td>
                                    <td className="px-4 py-3 text-sm text-slate-600">{route.workCenterName} ({route.workCenterCode})</td>
                                    <td className="px-4 py-3 text-sm"><span className="bg-slate-100 px-2 py-0.5 rounded font-mono text-xs">v{route.version}</span></td>
                                    <td className="px-4 py-3 text-sm font-bold text-indigo-600">{(route.steps || []).length}</td>
                                    <td className="px-4 py-3 text-sm">
                                        <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${route.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                                            {route.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </td>
                                    <td className="px-4 py-3 text-center" onClick={(e) => e.stopPropagation()}>
                                        <button onClick={() => openEdit(route)} className="p-1.5 rounded hover:bg-blue-50 text-slate-500 hover:text-blue-600" title="Edit">
                                            <Edit2 size={15} />
                                        </button>
                                        {route.isActive && (
                                            <button onClick={() => handleDelete(route)} className="p-1.5 rounded hover:bg-red-50 text-slate-500 hover:text-red-600" title="Deactivate">
                                                <Trash2 size={15} />
                                            </button>
                                        )}
                                    </td>
                                </tr>
                                {/* Expanded: Steps — NO materials */}
                                {expandedId === route.processRouteId && (
                                    <tr>
                                        <td colSpan="8" className="bg-slate-50 px-6 py-4">
                                            <table className="w-full text-xs">
                                                <thead>
                                                    <tr className="text-slate-400 uppercase">
                                                        <th className="text-left py-1">Step</th>
                                                        <th className="text-left py-1">Process</th>
                                                        <th className="text-left py-1">Equipment</th>
                                                        <th className="text-left py-1">Output Target</th>
                                                        <th className="text-left py-1">Setup (min)</th>
                                                        <th className="text-left py-1">Run/Unit (min)</th>
                                                        <th className="text-left py-1">Notes</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {(route.steps || []).sort((a, b) => a.stepNumber - b.stepNumber).map(step => (
                                                        <tr key={step.processRouteStepId} className="border-t border-slate-200">
                                                            <td className="py-2"><span className="bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded font-bold">#{step.stepNumber}</span></td>
                                                            <td className="py-2 font-medium">{step.processName} <span className="text-slate-400">({step.processCode})</span></td>
                                                            <td className="py-2 text-slate-600">{step.equipmentCode || '-'}</td>
                                                            <td className="py-2">
                                                                <span className="bg-blue-50 text-blue-700 px-1.5 py-0.5 rounded border border-blue-100">
                                                                    1 PO Unit = {step.outputMultiplier} {step.outputUnit}
                                                                </span>
                                                            </td>
                                                            <td className="py-2">{step.setupTimeMinutes}</td>
                                                            <td className="py-2">{step.runTimePerUnitMinutes}</td>
                                                            <td className="py-2 text-slate-500">{step.notes || '-'}</td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                )}
                            </React.Fragment>
                        ))}
                    </tbody>
                </table>
            </div>

            {/* ─── Create/Edit Route Form ─── */}
            {showForm && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-4xl max-h-[90vh] overflow-y-auto">
                        <div className="flex items-center justify-between p-5 border-b sticky top-0 bg-white rounded-t-2xl z-10">
                            <h2 className="text-lg font-bold text-slate-800">{editRoute ? 'Edit' : 'Create'} Process Route</h2>
                            <button onClick={() => setShowForm(false)} className="p-1.5 rounded-lg hover:bg-slate-100"><X size={20} /></button>
                        </div>
                        <form onSubmit={handleSubmit} className="p-5 space-y-5">
                            {/* Route info */}
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Route Code *</label>
                                    <input type="text" value={formData.routeCode} onChange={(e) => setFormData({...formData, routeCode: e.target.value})}
                                        className="w-full px-3 py-2 border rounded-lg text-sm" required placeholder="e.g. ROUTE-CHAIR-001" />
                                </div>
                                <div>
                                    <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Product *</label>
                                    <SearchSelect
                                        value={formData.productId}
                                        displayValue={getProductDisplayValue(formData.productId)}
                                        placeholder="Search product..."
                                        items={products}
                                        title="Select Product"
                                        displayFields={[
                                            { key: 'productCode', label: 'Code', width: '30%', bold: true },
                                            { key: 'productName', label: 'Name', width: '70%' },
                                        ]}
                                        searchKeys={['productCode', 'productName', 'code', 'name']}
                                        valueKey="id"
                                        onSelect={(p) => setFormData({...formData, productId: p.id || p.productId})}
                                    />
                                </div>
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Default Work Center *</label>
                                    <SearchSelect
                                        value={formData.workCenterId}
                                        displayValue={getWCDisplayValue(formData.workCenterId)}
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
                                    />
                                </div>
                                <div>
                                    <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Description</label>
                                    <input type="text" value={formData.description} onChange={(e) => setFormData({...formData, description: e.target.value})}
                                        className="w-full px-3 py-2 border rounded-lg text-sm" />
                                </div>
                            </div>

                            {/* Steps — NO materials */}
                            <div>
                                <div className="flex items-center justify-between mb-3">
                                    <h3 className="font-semibold text-slate-700">Steps ({formData.steps.length})</h3>
                                    <button type="button" onClick={addStep} className="text-sm text-indigo-600 hover:text-indigo-700 flex items-center gap-1">
                                        <Plus size={14} /> Add Step
                                    </button>
                                </div>

                                {formData.steps.map((step, si) => (
                                    <div key={si} className="bg-slate-50 rounded-xl p-4 mb-3 border border-slate-200">
                                        <div className="flex items-center justify-between mb-3">
                                            <span className="bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded text-xs font-bold">Step #{step.stepNumber}</span>
                                            <button type="button" onClick={() => removeStep(si)}
                                                className="flex items-center gap-1 px-2 py-1 rounded-lg text-xs text-red-500 hover:text-red-700 hover:bg-red-50 transition"
                                                title="Delete step">
                                                <Trash2 size={13} /> Remove
                                            </button>
                                        </div>
                                        <div className="grid grid-cols-2 md:grid-cols-6 gap-3 mb-3">
                                            <div className="md:col-span-2">
                                                <label className="block text-xs text-slate-500 mb-1">Process *</label>
                                                <SearchSelect
                                                    value={step.processId}
                                                    displayValue={getProcessDisplayValue(step.processId)}
                                                    placeholder="Select..."
                                                    items={processes}
                                                    title="Select Process"
                                                    displayFields={[
                                                        { key: 'processCode', label: 'Code', width: '25%', bold: true },
                                                        { key: 'processName', label: 'Name', width: '45%' },
                                                        { key: 'category', label: 'Category', width: '30%' },
                                                    ]}
                                                    searchKeys={['processCode', 'processName', 'category']}
                                                    valueKey="processId"
                                                    onSelect={(p) => updateStep(si, 'processId', p.processId)}
                                                    size="sm"
                                                />
                                            </div>
                                            <div className="md:col-span-2">
                                                <label className="block text-xs text-slate-500 mb-1">Equipment</label>
                                                <SearchSelect
                                                    value={step.equipmentId}
                                                    displayValue={getEquipmentDisplayValue(step.equipmentId)}
                                                    placeholder="None"
                                                    items={equipmentList}
                                                    title="Select Equipment"
                                                    displayFields={[
                                                        { key: 'equipmentCode', label: 'Code', width: '40%', bold: true },
                                                        { key: 'equipmentName', label: 'Name', width: '60%' },
                                                    ]}
                                                    searchKeys={['equipmentCode', 'equipmentName']}
                                                    valueKey="equipmentId"
                                                    onSelect={(eq) => updateStep(si, 'equipmentId', eq.equipmentId)}
                                                    onClear={() => updateStep(si, 'equipmentId', '')}
                                                    size="sm"
                                                />
                                            </div>
                                            <div>
                                                <label className="block text-xs text-slate-500 mb-1">Setup (min)</label>
                                                <input type="number" value={step.setupTimeMinutes} onChange={(e) => updateStep(si, 'setupTimeMinutes', e.target.value)}
                                                    className="w-full px-2 py-1.5 border rounded text-xs" />
                                            </div>
                                            <div>
                                                <label className="block text-xs text-slate-500 mb-1">Run/Unit (min)</label>
                                                <input type="number" value={step.runTimePerUnitMinutes} onChange={(e) => updateStep(si, 'runTimePerUnitMinutes', e.target.value)}
                                                    className="w-full px-2 py-1.5 border rounded text-xs" />
                                            </div>
                                            <div>
                                                <label className="block text-xs text-slate-500 mb-1" title="How many output units per 1 planned PO unit?">Out. Multiplier *</label>
                                                <input type="number" step="0.01" value={step.outputMultiplier} onChange={(e) => updateStep(si, 'outputMultiplier', e.target.value)}
                                                    className="w-full px-2 py-1.5 border rounded text-xs" required />
                                            </div>
                                            <div>
                                                <label className="block text-xs text-slate-500 mb-1">Out. Unit *</label>
                                                <select 
                                                    value={step.outputUnit} 
                                                    onChange={(e) => updateStep(si, 'outputUnit', e.target.value)}
                                                    className="w-full px-2 py-1.5 border rounded text-xs" 
                                                    required
                                                >
                                                    <option value="pcs">pcs</option>
                                                    {units.map((u) => (
                                                        <option key={u.id} value={u.unitCode}>{u.unitName} ({u.unitCode})</option>
                                                    ))}
                                                </select>
                                            </div>
                                        </div>
                                        <div>
                                            <label className="block text-xs text-slate-500 mb-1">Notes</label>
                                            <input type="text" value={step.notes} onChange={(e) => updateStep(si, 'notes', e.target.value)}
                                                className="w-full px-2 py-1.5 border rounded text-xs" placeholder="Step notes" />
                                        </div>
                                    </div>
                                ))}
                            </div>

                            {/* Submit */}
                            <div className="flex justify-end gap-3 pt-3 border-t">
                                <button type="button" onClick={() => setShowForm(false)} className="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">Cancel</button>
                                <button type="submit" className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium">
                                    <Save size={16} /> {editRoute ? 'Update' : 'Create'} Route
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ProcessRoutesPage;
