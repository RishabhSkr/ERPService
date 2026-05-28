import React, { useEffect, useState } from 'react';
import { X, Zap, Loader, Layers, ArrowRight } from 'lucide-react';
import useApi from '../../hooks/useApi';
import { useAuth } from '../../context/AuthContext';
import { getWOPlanningInfo, activateWO } from '../../api/productionService';
import SearchSelect from '../common/SearchSelect';

const ActivateWOModal = ({ workOrder, onClose, onActivated }) => {
    const [equipment, setEquipment] = useState([]);
    const [firstStepInfo, setFirstStepInfo] = useState(null);   // For Entire Route WO
    const [lastStepInfo, setLastStepInfo] = useState(null);     // For Entire Route WO output info
    const [selectedEquipmentId, setSelectedEquipmentId] = useState('');
    const [notes, setNotes] = useState('');
    const { user } = useAuth();
    const currentUser = user?.username ?? 'Unknown';
    const { loading, requestHandlerFunction } = useApi();

    // stepNumber === 0 means "Entire Route" WO (all steps under 1 WO)
    const isEntireRouteWO = workOrder.stepNumber === 0;

    useEffect(() => {
        const load = async () => {
            // Both WO types use planning info — same API call
            const res = await requestHandlerFunction(
                () => getWOPlanningInfo(workOrder.productionOrderId)
            );
            if (!res.success) return;

            const data = res.data?.data?.data ?? res.data?.data;
            // Find the exact route this WO belongs to (routeCode is in WO DTO)
            const matchedRoute = data?.routes?.find(r => r.routeCode === workOrder.routeCode);
            if (!matchedRoute) return;

            const sortedSteps = [...(matchedRoute.steps ?? [])].sort(
                (a, b) => a.stepNumber - b.stepNumber
            );

            if (isEntireRouteWO) {
                // ── ENTIRE ROUTE WO ──────────────────────────────────────────
                // Activate on FIRST step's machine.
                // Output = LAST step's final output.
                // Logic: First step ki process se linked equipment dikhao.
                const first = sortedSteps[0];
                const last = sortedSteps[sortedSteps.length - 1];

                if (first) {
                    setFirstStepInfo(first);
                    setEquipment(first.linkedEquipment || []);
                }
                if (last) {
                    setLastStepInfo(last);
                }
            } else {
                // ── INDIVIDUAL STEP WO ───────────────────────────────────────
                // Match by routeCode (already filtered above) + stepNumber.
                // This avoids ambiguity when multiple routes have same stepNumber.
                const step = sortedSteps.find(s => s.stepNumber === workOrder.stepNumber);
                if (step) {
                    setEquipment(step.linkedEquipment || []);
                }
            }
        };
        load();
    }, [workOrder.productionOrderId, workOrder.routeCode]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!selectedEquipmentId) return;

        const payload = {
            equipmentId: selectedEquipmentId,
            activatedBy: currentUser,
            notes: notes || null,
        };

        const res = await requestHandlerFunction(
            () => activateWO(workOrder.workOrderId, payload),
            'Work Order activated on equipment!'
        );
        if (res.success) onActivated();
    };

    const selectedEq = equipment.find(e => e.equipmentId === selectedEquipmentId);

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg">
                {/* Header */}
                <div className="flex items-center justify-between p-5 border-b">
                    <h2 className="text-lg font-bold text-slate-800 flex items-center gap-2">
                        <Zap size={20} className="text-blue-500" /> Activate on Equipment
                    </h2>
                    <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-slate-100">
                        <X size={20} />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-5 space-y-4">

                    {/* WO Info Card */}
                    <div className="rounded-xl border p-3 space-y-2">
                        <div className="flex items-center gap-2 flex-wrap">
                            <span className="font-bold text-slate-800">{workOrder.workOrderNumber}</span>
                            {isEntireRouteWO && (
                                <span className="flex items-center gap-1 px-2 py-0.5 bg-emerald-100 text-emerald-700 rounded-full text-xs font-semibold">
                                    <Layers size={11} /> Entire Route
                                </span>
                            )}
                            <span className="text-xs text-slate-500 ml-auto">
                                Qty: <strong>{workOrder.quantityPlanned}</strong>
                            </span>
                        </div>

                        {isEntireRouteWO && firstStepInfo ? (
                            // Entire Route: Show step chain with highlight on first
                            <div className="space-y-1.5">
                                <p className="text-xs text-slate-500">Route covers:</p>
                                <div className="flex items-center gap-1 flex-wrap text-xs">
                                    <span className="px-2 py-1 bg-blue-100 text-blue-700 rounded font-semibold border border-blue-300">
                                        ⚡ Step {firstStepInfo.stepNumber} — {firstStepInfo.processName}
                                        <span className="text-blue-500 ml-1 font-normal">(Activate here)</span>
                                    </span>
                                    {lastStepInfo && lastStepInfo.stepNumber !== firstStepInfo.stepNumber && (
                                        <>
                                            <ArrowRight size={12} className="text-slate-400" />
                                            <span className="px-2 py-1 bg-emerald-50 text-emerald-700 rounded font-semibold border border-emerald-200">
                                                Step {lastStepInfo.stepNumber} — {lastStepInfo.processName}
                                                <span className="text-emerald-500 ml-1 font-normal">(Final output)</span>
                                            </span>
                                        </>
                                    )}
                                </div>
                                <p className="text-xs text-amber-600 bg-amber-50 rounded px-2 py-1">
                                    💡 Output of this WO = <strong>{lastStepInfo?.processName ?? 'last step'}</strong> ka final quantity
                                </p>
                            </div>
                        ) : (
                            <p className="text-sm text-slate-600">
                                Step #{workOrder.stepNumber} — {workOrder.operationName}
                            </p>
                        )}
                    </div>

                    {/* Equipment Selection */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">
                            {isEntireRouteWO
                                ? `Select Equipment — Step ${firstStepInfo?.stepNumber ?? 1}: ${firstStepInfo?.processName ?? ''} *`
                                : 'Select Equipment *'
                            }
                        </label>
                        {equipment.length === 0 ? (
                            <div className="text-sm text-slate-400 py-2 flex items-center gap-2">
                                {loading
                                    ? <><Loader size={14} className="animate-spin" /> Loading equipment...</>
                                    : 'No compatible equipment found for this process'
                                }
                            </div>
                        ) : (
                            <SearchSelect
                                value={selectedEquipmentId}
                                displayValue={(() => {
                                    const eq = equipment.find(e => e.equipmentId === selectedEquipmentId);
                                    return eq ? `${eq.equipmentCode} — ${eq.equipmentName}` : '';
                                })()}
                                placeholder="Choose equipment..."
                                items={equipment}
                                title="Select Equipment"
                                displayFields={[
                                    { key: 'equipmentCode', label: 'Code', width: '25%', bold: true },
                                    { key: 'equipmentName', label: 'Name', width: '40%' },
                                    { key: 'workCenterCode', label: 'Work Center', width: '20%' },
                                    { key: 'status', label: 'Status', width: '15%' },
                                ]}
                                searchKeys={['equipmentCode', 'equipmentName']}
                                valueKey="equipmentId"
                                onSelect={(eq) => setSelectedEquipmentId(eq.equipmentId)}
                            />
                        )}
                    </div>

                    {/* Selected equipment info */}
                    {selectedEq && (
                        <div className="text-xs bg-slate-50 rounded-lg p-2 text-slate-500 flex gap-3">
                            <span>₹{selectedEq.costPerHour}/hr</span>
                            <span>Work Center: {selectedEq.workCenterCode}</span>
                        </div>
                    )}

                    {/* Notes */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Notes</label>
                        <input type="text" value={notes} onChange={(e) => setNotes(e.target.value)}
                            className="w-full border rounded-lg px-3 py-2 text-sm"
                            placeholder="Optional" />
                    </div>

                    {/* Submit */}
                    <div className="flex justify-end gap-3 pt-3 border-t">
                        <button type="button" onClick={onClose}
                            className="px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">
                            Cancel
                        </button>
                        <button type="submit" disabled={loading || !selectedEquipmentId}
                            className="px-5 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50 flex items-center gap-2">
                            {loading ? <Loader size={14} className="animate-spin" /> : <Zap size={14} />}
                            Activate
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default ActivateWOModal;
