import React, { useState, useEffect, useCallback } from 'react';
import { Route as RouteIcon, Plus, Edit2, Trash2, X, RefreshCw, Save, ChevronDown, ChevronRight, Eye, Loader } from 'lucide-react';
import toast from 'react-hot-toast';
import {
    getProcessRoutes, createProcessRoute, updateProcessRoute,
    getProcesses, getWorkCenters, getEquipment
} from '../../api/productionService';
import { getProducts } from '../../api/master/product';
import { getRawMaterials } from '../../api/master/rawMaterial';

/**
 * Process Routes — Route → Steps (ordered) → Materials per step
 * 
 * CreateProcessRouteDto: { routeCode, productId, workCenterId, description, steps[] }
 * CreateProcessRouteStepDto: { stepNumber, processId, equipmentId?, setupTimeMinutes, runTimePerUnitMinutes, notes, materials[] }
 * CreateStepMaterialDto: { bomLineId?, rawMaterialId, materialCode, materialName, quantity, unit }
 */
const ProcessRoutesPage = () => {
    const [routes, setRoutes] = useState([]);
    const [processes, setProcesses] = useState([]);
    const [workCenters, setWorkCenters] = useState([]);
    const [equipmentList, setEquipmentList] = useState([]);
    const [products, setProducts] = useState([]);
    const [rawMaterials, setRawMaterials] = useState([]);
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
            const [routeRes, procRes, wcRes, eqRes, prodRes, rmRes] = await Promise.all([
                getProcessRoutes(), getProcesses(), getWorkCenters(), getEquipment(),
                getProducts(), getRawMaterials()
            ]);
            setRoutes(extractData(routeRes));
            setProcesses(extractData(procRes));
            setWorkCenters(extractData(wcRes));
            setEquipmentList(extractData(eqRes));
            setProducts(extractProducts(prodRes));
            setRawMaterials(extractProducts(rmRes));
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
            materials: [],
        };
    }

    function emptyMaterial() {
        return { rawMaterialId: '', materialCode: '', materialName: '', quantity: '', unit: '' };
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
                stepNumber: s.stepNumber,
                processId: processes.find(p => p.processCode === s.processCode)?.processId || '',
                equipmentId: '',
                setupTimeMinutes: s.setupTimeMinutes,
                runTimePerUnitMinutes: s.runTimePerUnitMinutes,
                notes: s.notes || '',
                materials: (s.materials || []).map(m => ({
                    rawMaterialId: m.rawMaterialId,
                    materialCode: m.materialCode,
                    materialName: m.materialName,
                    quantity: m.quantity,
                    unit: m.unit,
                })),
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

    // Material management within step
    const addMaterial = (stepIdx) => {
        const newSteps = [...formData.steps];
        newSteps[stepIdx].materials = [...newSteps[stepIdx].materials, emptyMaterial()];
        setFormData({ ...formData, steps: newSteps });
    };

    const removeMaterial = (stepIdx, matIdx) => {
        const newSteps = [...formData.steps];
        newSteps[stepIdx].materials = newSteps[stepIdx].materials.filter((_, i) => i !== matIdx);
        setFormData({ ...formData, steps: newSteps });
    };

    const updateMaterial = (stepIdx, matIdx, field, value) => {
        const newSteps = [...formData.steps];
        const mat = { ...newSteps[stepIdx].materials[matIdx], [field]: value };
        // Auto-fill code/name when rawMaterialId changes
        if (field === 'rawMaterialId') {
            const rm = rawMaterials.find(r => (r.id || r.rawMaterialId) === value);
            if (rm) {
                mat.materialCode = rm.materialCode || rm.code || '';
                mat.materialName = rm.materialName || rm.name || '';
                mat.unit = rm.unit || rm.uom || '';
            }
        }
        newSteps[stepIdx].materials[matIdx] = mat;
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
                stepNumber: s.stepNumber,
                processId: s.processId,
                equipmentId: s.equipmentId || null,
                setupTimeMinutes: parseInt(s.setupTimeMinutes) || 0,
                runTimePerUnitMinutes: parseInt(s.runTimePerUnitMinutes) || 0,
                notes: s.notes || null,
                materials: s.materials.map(m => ({
                    bomLineId: null,
                    rawMaterialId: m.rawMaterialId,
                    materialCode: m.materialCode,
                    materialName: m.materialName,
                    quantity: parseFloat(m.quantity) || 0,
                    unit: m.unit,
                })),
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
                                    </td>
                                </tr>
                                {/* Expanded: Steps */}
                                {expandedId === route.processRouteId && (
                                    <tr>
                                        <td colSpan="8" className="bg-slate-50 px-6 py-4">
                                            <table className="w-full text-xs">
                                                <thead>
                                                    <tr className="text-slate-400 uppercase">
                                                        <th className="text-left py-1">Step</th>
                                                        <th className="text-left py-1">Process</th>
                                                        <th className="text-left py-1">Equipment</th>
                                                        <th className="text-left py-1">Setup (min)</th>
                                                        <th className="text-left py-1">Run/Unit (min)</th>
                                                        <th className="text-left py-1">Materials</th>
                                                        <th className="text-left py-1">Notes</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {(route.steps || []).sort((a, b) => a.stepNumber - b.stepNumber).map(step => (
                                                        <tr key={step.processRouteStepId} className="border-t border-slate-200">
                                                            <td className="py-2"><span className="bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded font-bold">#{step.stepNumber}</span></td>
                                                            <td className="py-2 font-medium">{step.processName} <span className="text-slate-400">({step.processCode})</span></td>
                                                            <td className="py-2 text-slate-600">{step.equipmentCode || '-'}</td>
                                                            <td className="py-2">{step.setupTimeMinutes}</td>
                                                            <td className="py-2">{step.runTimePerUnitMinutes}</td>
                                                            <td className="py-2">
                                                                {(step.materials || []).length === 0 ? <span className="text-slate-400">-</span> : (
                                                                    <div className="space-y-0.5">
                                                                        {step.materials.map((m, i) => (
                                                                            <div key={i} className="flex gap-2">
                                                                                <span className="text-slate-700">{m.materialName}</span>
                                                                                <span className="text-slate-500">×{m.quantity} {m.unit}</span>
                                                                            </div>
                                                                        ))}
                                                                    </div>
                                                                )}
                                                            </td>
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
                                    <select value={formData.productId} onChange={(e) => setFormData({...formData, productId: e.target.value})}
                                        className="w-full px-3 py-2 border rounded-lg text-sm bg-white" required>
                                        <option value="">Select product...</option>
                                        {products.map(p => (
                                            <option key={p.id || p.productId} value={p.id || p.productId}>{p.productName || p.name} ({p.productCode || p.code})</option>
                                        ))}
                                    </select>
                                </div>
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Default Work Center *</label>
                                    <select value={formData.workCenterId} onChange={(e) => setFormData({...formData, workCenterId: e.target.value})}
                                        className="w-full px-3 py-2 border rounded-lg text-sm bg-white" required>
                                        <option value="">Select work center...</option>
                                        {workCenters.map(wc => (
                                            <option key={wc.workCenterId} value={wc.workCenterId}>{wc.centerName} ({wc.centerCode})</option>
                                        ))}
                                    </select>
                                </div>
                                <div>
                                    <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Description</label>
                                    <input type="text" value={formData.description} onChange={(e) => setFormData({...formData, description: e.target.value})}
                                        className="w-full px-3 py-2 border rounded-lg text-sm" />
                                </div>
                            </div>

                            {/* Steps */}
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
                                            {formData.steps.length > 1 && (
                                                <button type="button" onClick={() => removeStep(si)} className="text-red-500 hover:text-red-600 text-xs">Remove</button>
                                            )}
                                        </div>
                                        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-3">
                                            <div>
                                                <label className="block text-xs text-slate-500 mb-1">Process *</label>
                                                <select value={step.processId} onChange={(e) => updateStep(si, 'processId', e.target.value)}
                                                    className="w-full px-2 py-1.5 border rounded text-xs bg-white" required>
                                                    <option value="">Select...</option>
                                                    {processes.map(p => (
                                                        <option key={p.processId} value={p.processId}>{p.processCode} — {p.processName}</option>
                                                    ))}
                                                </select>
                                            </div>
                                            <div>
                                                <label className="block text-xs text-slate-500 mb-1">Equipment</label>
                                                <select value={step.equipmentId} onChange={(e) => updateStep(si, 'equipmentId', e.target.value)}
                                                    className="w-full px-2 py-1.5 border rounded text-xs bg-white">
                                                    <option value="">None</option>
                                                    {equipmentList.map(eq => (
                                                        <option key={eq.equipmentId} value={eq.equipmentId}>{eq.equipmentCode}</option>
                                                    ))}
                                                </select>
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
                                        </div>
                                        <div className="mb-3">
                                            <label className="block text-xs text-slate-500 mb-1">Notes</label>
                                            <input type="text" value={step.notes} onChange={(e) => updateStep(si, 'notes', e.target.value)}
                                                className="w-full px-2 py-1.5 border rounded text-xs" placeholder="Step notes" />
                                        </div>

                                        {/* Materials for this step */}
                                        <div className="border-t border-slate-200 pt-2 mt-2">
                                            <div className="flex justify-between items-center mb-2">
                                                <span className="text-xs font-semibold text-slate-500">Materials ({step.materials.length})</span>
                                                <button type="button" onClick={() => addMaterial(si)} className="text-xs text-blue-600 hover:text-blue-700">+ Add Material</button>
                                            </div>
                                            {step.materials.map((mat, mi) => (
                                                <div key={mi} className="grid grid-cols-4 gap-2 mb-2 items-end">
                                                    <div>
                                                        <label className="block text-[10px] text-slate-400">Material</label>
                                                        <select value={mat.rawMaterialId} onChange={(e) => updateMaterial(si, mi, 'rawMaterialId', e.target.value)}
                                                            className="w-full px-1.5 py-1 border rounded text-xs bg-white" required>
                                                            <option value="">Select...</option>
                                                            {rawMaterials.map(rm => (
                                                                <option key={rm.id || rm.rawMaterialId} value={rm.id || rm.rawMaterialId}>
                                                                    {rm.materialName || rm.name} ({rm.materialCode || rm.code})
                                                                </option>
                                                            ))}
                                                        </select>
                                                    </div>
                                                    <div>
                                                        <label className="block text-[10px] text-slate-400">Quantity</label>
                                                        <input type="number" step="any" value={mat.quantity} onChange={(e) => updateMaterial(si, mi, 'quantity', e.target.value)}
                                                            className="w-full px-1.5 py-1 border rounded text-xs" required />
                                                    </div>
                                                    <div>
                                                        <label className="block text-[10px] text-slate-400">Unit</label>
                                                        <input type="text" value={mat.unit} onChange={(e) => updateMaterial(si, mi, 'unit', e.target.value)}
                                                            className="w-full px-1.5 py-1 border rounded text-xs" placeholder="kg/pcs" />
                                                    </div>
                                                    <button type="button" onClick={() => removeMaterial(si, mi)} className="text-red-500 text-xs hover:text-red-600 pb-1">✕</button>
                                                </div>
                                            ))}
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
