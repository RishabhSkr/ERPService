import React, { useEffect, useState } from 'react';
import { X, Plus, Loader, Zap, Layers } from 'lucide-react';
import useApi from '../../hooks/useApi';
import { getWOPlanningInfo, createWorkOrder, generateRouteWOs } from '../../api/productionService';
import SearchSelect from '../common/SearchSelect';

const CreateWOModal = ({ poId, onClose, onCreated }) => {
    const [planningInfo, setPlanningInfo] = useState(null);
    const [selectedRouteId, setSelectedRouteId] = useState('');
    const [selectedScope, setSelectedScope] = useState(''); // 'entire' or stepId
    const [workCenterId, setWorkCenterId] = useState('');
    const [qty, setQty] = useState('');
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [notes, setNotes] = useState('');
    const { loading, requestHandlerFunction } = useApi();

    // Fetch planning info for PO
    useEffect(() => {
        const load = async () => {
            const res = await requestHandlerFunction(() => getWOPlanningInfo(poId));
            if (res.success) {
                const data = res.data?.data?.data || res.data?.data;
                setPlanningInfo(data);
                // Auto-select if only 1 route
                if (data?.routes?.length === 1) {
                    setSelectedRouteId(data.routes[0].processRouteId);
                }
            }
        };
        load();
    }, [poId]);

    // Derived values
    const routes = planningInfo?.routes || [];
    const selectedRoute = routes.find(r => r.processRouteId === selectedRouteId);
    const currentSteps = selectedRoute?.steps || [];
    const isEntireRoute = selectedScope === 'entire';
    const selectedStep = !isEntireRoute ? currentSteps.find(s => s.processRouteStepId === selectedScope) : null;

    // Reset scope when route changes
    useEffect(() => { setSelectedScope(''); setWorkCenterId(''); setQty(''); }, [selectedRouteId]);

    // Work Center always comes from the route (not from equipment)
    // Auto-set for BOTH modes: entire route AND individual step
    useEffect(() => {
        if (selectedRoute) {
            setWorkCenterId(selectedRoute.workCenterId);
        }
    }, [selectedScope, selectedRoute]);

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (isEntireRoute) {
            // Generate all WOs for the route
            const payload = {
                productionOrderId: poId,
                processRouteId: selectedRouteId,
            };
            const res = await requestHandlerFunction(() => generateRouteWOs(payload), `Generated WOs for ${selectedRoute?.routeCode}!`);
            if (res.success) onCreated();
        } else {
            // Create single WO
            if (!selectedScope || !workCenterId || !qty) return;
            const payload = {
                productionOrderId: poId,
                processRouteId: selectedRouteId,
                processRouteStepId: selectedScope,
                workCenterId: workCenterId,
                quantityPlanned: parseFloat(qty),
                scheduledStart: startDate ? new Date(startDate).toISOString() : null,
                scheduledEnd: endDate ? new Date(endDate).toISOString() : null,
                notes: notes || null,
            };
            const res = await requestHandlerFunction(() => createWorkOrder(payload), 'Work Order Created!');
            if (res.success) onCreated();
        }
    };

    const canSubmit = isEntireRoute
        ? !!selectedRouteId
        : (!!selectedScope && !!workCenterId && !!qty);

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="flex items-center justify-between p-5 border-b sticky top-0 bg-white rounded-t-2xl z-10">
                    <h2 className="text-lg font-bold text-slate-800 flex items-center gap-2">
                        <Plus size={20} className="text-indigo-500" /> Create Work Order
                    </h2>
                    <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-slate-100"><X size={20} /></button>
                </div>

                {!planningInfo ? (
                    <div className="p-8 text-center text-slate-400 flex items-center justify-center gap-2">
                        <Loader size={16} className="animate-spin" /> Loading planning info...
                    </div>
                ) : (
                    <form onSubmit={handleSubmit} className="p-5 space-y-5">
                        {/* PO Info */}
                        <div className="bg-indigo-50 rounded-xl p-3 text-sm">
                            <span className="font-bold text-indigo-700">{planningInfo.orderNumber}</span>
                            <span className="text-slate-500 ml-2">{planningInfo.productName} ({planningInfo.productCode})</span>
                            <span className="text-slate-500 ml-2">| Planned: <strong>{planningInfo.poQuantityPlanned}</strong></span>
                        </div>

                        {/* Route Selection */}
                        <div>
                            <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Select Process Route *</label>
                            <SearchSelect
                                value={selectedRouteId}
                                displayValue={(() => { const r = routes.find(r => r.processRouteId === selectedRouteId); return r ? `${r.routeCode} — ${r.workCenterName}` : ''; })()}
                                placeholder="Choose a route..."
                                items={routes.map(r => ({...r, _display: `${r.routeCode} — ${r.workCenterName}`, _steps: `${r.steps?.length || 0} steps`}))}
                                title="Select Process Route"
                                displayFields={[
                                    { key: 'routeCode', label: 'Route', width: '30%', bold: true },
                                    { key: 'workCenterName', label: 'Work Center', width: '45%' },
                                    { key: '_steps', label: 'Steps', width: '25%' },
                                ]}
                                searchKeys={['routeCode', 'workCenterName']}
                                valueKey="processRouteId"
                                onSelect={(r) => { setSelectedRouteId(r.processRouteId); }}
                            />
                        </div>

                        {/* Scope Selection (only after route selected) */}
                        {selectedRoute && (
                            <div>
                                <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Select Scope *</label>
                                <select
                                    value={selectedScope}
                                    onChange={(e) => { setSelectedScope(e.target.value); setWorkCenterId(''); setQty(''); }}
                                    className="w-full border rounded-lg px-3 py-2 text-sm"
                                >
                                    <option value="">Choose...</option>
                                    <option value="entire">🔄 Entire Route (All {currentSteps.length} steps) — 1 WO</option>
                                    <optgroup label="Individual Steps">
                                        {currentSteps.map(s => (
                                            <option key={s.processRouteStepId} value={s.processRouteStepId}>
                                                #{s.stepNumber} {s.processName} — Remaining: {s.remainingQuantity} {s.outputUnit}
                                            </option>
                                        ))}
                                    </optgroup>
                                </select>
                            </div>
                        )}

                        {/* ENTIRE ROUTE MODE — Preview */}
                        {isEntireRoute && selectedRoute && (
                            <div className="bg-emerald-50 border border-emerald-200 rounded-xl p-4">
                                <p className="text-sm font-semibold text-emerald-700 flex items-center gap-2 mb-3">
                                    <Zap size={16} /> 1 Work Order will be created
                                </p>
                                <div className="space-y-2 text-xs text-slate-600">
                                    <div className="flex justify-between">
                                        <span className="text-slate-500">Route</span>
                                        <span className="font-semibold">{selectedRoute.routeCode}</span>
                                    </div>
                                    <div className="flex justify-between">
                                        <span className="text-slate-500">Work Center</span>
                                        <span className="font-semibold">{selectedRoute.workCenterName}</span>
                                    </div>
                                    <div className="flex justify-between">
                                        <span className="text-slate-500">Covers Steps</span>
                                        <span className="font-semibold">{currentSteps.map(s => s.processName).join(' → ')}</span>
                                    </div>
                                    <div className="flex justify-between">
                                        <span className="text-slate-500">Planned Qty</span>
                                        <span className="font-semibold text-emerald-700">
                                            {currentSteps[currentSteps.length - 1]?.remainingQuantity ?? '—'} {currentSteps[currentSteps.length - 1]?.outputUnit ?? ''}
                                        </span>
                                    </div>
                                    <p className="text-slate-400 pt-1 border-t border-emerald-200 mt-2">
                                        💡 Last step output = WO completion. No step-wise tracking.
                                    </p>
                                </div>
                            </div>
                        )}

                        {/* INDIVIDUAL STEP MODE — Form fields */}
                        {selectedStep && (
                            <>
                                {/* Step Details */}
                                <div className="grid grid-cols-3 gap-3 text-center">
                                    <div className="bg-blue-50 rounded-lg p-2">
                                        <p className="text-xs text-blue-500">Remaining Target</p>
                                        <p className="text-xl font-bold text-blue-700">{selectedStep.remainingQuantity} <span className="text-sm font-normal">{selectedStep.outputUnit}</span></p>
                                    </div>
                                    <div className="bg-slate-50 rounded-lg p-2">
                                        <p className="text-xs text-slate-400">Setup Time</p>
                                        <p className="text-xl font-bold text-slate-600">{selectedStep.setupTimeMinutes}m</p>
                                    </div>
                                    <div className="bg-slate-50 rounded-lg p-2">
                                        <p className="text-xs text-slate-400">Run/Unit</p>
                                        <p className="text-xl font-bold text-slate-600">{selectedStep.runTimePerUnitMinutes}m</p>
                                    </div>
                                </div>

                                {/* Existing WOs for this step */}
                                {selectedStep.existingWorkOrders?.length > 0 && (
                                    <div className="text-xs bg-yellow-50 rounded-lg p-3 border border-yellow-200">
                                        <p className="font-semibold text-yellow-700 mb-1">Existing WOs for this step:</p>
                                        {selectedStep.existingWorkOrders.map(ew => (
                                            <div key={ew.workOrderId} className="flex justify-between">
                                                <span>{ew.workOrderNumber} @ {ew.workCenterCode}</span>
                                                <span>Planned: {ew.quantityPlanned} {ew.outputUnit} | Done: {ew.quantityCompleted} {ew.outputUnit} | {ew.status}</span>
                                            </div>
                                        ))}
                                    </div>
                                )}

                                {/* Work Center — auto from Route (read-only) */}
                                <div>
                                    <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Work Center</label>
                                    <div className="flex items-center gap-2 bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-sm">
                                        <span className="text-slate-400 text-xs">🏭</span>
                                        <span className="font-semibold text-slate-700">{selectedRoute?.workCenterName || '—'}</span>
                                        <span className="text-slate-400 text-xs ml-auto">From Route</span>
                                    </div>
                                </div>

                                {/* Quantity */}
                                <div>
                                    <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Target Quantity ({selectedStep.outputUnit || 'units'}) *</label>
                                    <input type="number" value={qty} onChange={(e) => setQty(e.target.value)}
                                        min="0.01" max={selectedStep.remainingQuantity || 99999} step="0.01"
                                        className="w-full border rounded-lg px-3 py-2 text-sm" required
                                        placeholder={`Max: ${selectedStep.remainingQuantity || '—'} ${selectedStep.outputUnit || ''}`} />
                                </div>

                                {/* Schedule */}
                                <div className="grid grid-cols-2 gap-3">
                                    <div>
                                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Start Date</label>
                                        <input type="datetime-local" value={startDate} onChange={(e) => setStartDate(e.target.value)}
                                            className="w-full border rounded-lg px-3 py-2 text-sm" />
                                    </div>
                                    <div>
                                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">End Date</label>
                                        <input type="datetime-local" value={endDate} onChange={(e) => setEndDate(e.target.value)}
                                            className="w-full border rounded-lg px-3 py-2 text-sm" />
                                    </div>
                                </div>

                                {/* Notes */}
                                <div>
                                    <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Notes</label>
                                    <input type="text" value={notes} onChange={(e) => setNotes(e.target.value)}
                                        className="w-full border rounded-lg px-3 py-2 text-sm" placeholder="Optional notes" />
                                </div>
                            </>
                        )}

                        {/* Submit */}
                        <div className="flex justify-end gap-3 pt-3 border-t">
                            <button type="button" onClick={onClose} className="px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">Cancel</button>
                            <button type="submit" disabled={loading || !canSubmit}
                                className={`px-5 py-2 text-white rounded-lg text-sm flex items-center gap-2 disabled:opacity-50 ${isEntireRoute ? 'bg-emerald-600 hover:bg-emerald-700' : 'bg-indigo-600 hover:bg-indigo-700'}`}>
                                {loading ? <Loader size={14} className="animate-spin" /> : isEntireRoute ? <Zap size={14} /> : <Plus size={14} />}
                                {isEntireRoute ? 'Create 1 WO (Entire Route)' : 'Create Work Order'}
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    );
};

export default CreateWOModal;
