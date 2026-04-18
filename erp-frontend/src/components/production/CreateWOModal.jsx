import React, { useEffect, useState } from 'react';
import { X, Plus, Loader } from 'lucide-react';
import useApi from '../../hooks/useApi';
import { getWOPlanningInfo, createWorkOrder } from '../../api/productionService';

const CreateWOModal = ({ poId, onClose, onCreated }) => {
    const [planningInfo, setPlanningInfo] = useState(null);
    const [selectedStepId, setSelectedStepId] = useState('');
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
            }
        };
        load();
    }, [poId]);

    const selectedStep = planningInfo?.steps?.find(s => s.processRouteStepId === selectedStepId);
    const availableEquipment = selectedStep?.linkedEquipment || [];
    // Derive unique work centers from equipment
    const workCenters = [...new Map(availableEquipment.map(e => [e.workCenterId, { id: e.workCenterId, code: e.workCenterCode }])).values()];

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!selectedStepId || !workCenterId || !qty) return;

        const payload = {
            productionOrderId: poId,
            processRouteStepId: selectedStepId,
            workCenterId: workCenterId,
            quantityPlanned: parseFloat(qty),
            scheduledStart: startDate ? new Date(startDate).toISOString() : null,
            scheduledEnd: endDate ? new Date(endDate).toISOString() : null,
            notes: notes || null,
        };

        const res = await requestHandlerFunction(() => createWorkOrder(payload), 'Work Order Created!');
        if (res.success) onCreated();
    };

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

                        {/* Step Selection */}
                        <div>
                            <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Select Step *</label>
                            <select value={selectedStepId} onChange={(e) => { setSelectedStepId(e.target.value); setWorkCenterId(''); }}
                                className="w-full border rounded-lg px-3 py-2 text-sm" required>
                                <option value="">Choose a step...</option>
                                {(planningInfo.steps || []).map(step => (
                                    <option key={step.processRouteStepId} value={step.processRouteStepId}>
                                        Step #{step.stepNumber} — {step.processName} (Remaining: {step.remainingQuantity})
                                    </option>
                                ))}
                            </select>
                        </div>

                        {/* Step Details */}
                        {selectedStep && (
                            <div className="grid grid-cols-3 gap-3 text-center">
                                <div className="bg-blue-50 rounded-lg p-2">
                                    <p className="text-xs text-blue-500">Remaining</p>
                                    <p className="text-xl font-bold text-blue-700">{selectedStep.remainingQuantity}</p>
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
                        )}

                        {/* Existing WOs for this step */}
                        {selectedStep?.existingWorkOrders?.length > 0 && (
                            <div className="text-xs bg-yellow-50 rounded-lg p-3 border border-yellow-200">
                                <p className="font-semibold text-yellow-700 mb-1">Existing WOs for this step:</p>
                                {selectedStep.existingWorkOrders.map(ew => (
                                    <div key={ew.workOrderId} className="flex justify-between">
                                        <span>{ew.workOrderNumber} @ {ew.workCenterCode}</span>
                                        <span>Planned: {ew.quantityPlanned} | Done: {ew.quantityCompleted} | {ew.status}</span>
                                    </div>
                                ))}
                            </div>
                        )}

                        {/* Work Center */}
                        {selectedStep && (
                            <div>
                                <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Work Center *</label>
                                <select value={workCenterId} onChange={(e) => setWorkCenterId(e.target.value)}
                                    className="w-full border rounded-lg px-3 py-2 text-sm" required>
                                    <option value="">Choose work center...</option>
                                    {workCenters.map(wc => (
                                        <option key={wc.id} value={wc.id}>{wc.code}</option>
                                    ))}
                                </select>
                            </div>
                        )}

                        {/* Quantity */}
                        <div>
                            <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Quantity *</label>
                            <input type="number" value={qty} onChange={(e) => setQty(e.target.value)}
                                min="1" max={selectedStep?.remainingQuantity || 99999}
                                className="w-full border rounded-lg px-3 py-2 text-sm" required
                                placeholder={`Max: ${selectedStep?.remainingQuantity || '—'}`} />
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

                        {/* Submit */}
                        <div className="flex justify-end gap-3 pt-3 border-t">
                            <button type="button" onClick={onClose} className="px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">Cancel</button>
                            <button type="submit" disabled={loading || !selectedStepId || !workCenterId || !qty}
                                className="px-5 py-2 bg-indigo-600 text-white rounded-lg text-sm hover:bg-indigo-700 disabled:opacity-50 flex items-center gap-2">
                                {loading ? <Loader size={14} className="animate-spin" /> : <Plus size={14} />}
                                Create Work Order
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    );
};

export default CreateWOModal;
