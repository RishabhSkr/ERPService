import React, { useEffect, useState } from 'react';
import { X, Zap, Loader } from 'lucide-react';
import useApi from '../../hooks/useApi';
import { getWOPlanningInfo, activateWO } from '../../api/productionService';
import SearchSelect from '../common/SearchSelect';

const ActivateWOModal = ({ workOrder, onClose, onActivated }) => {
    const [equipment, setEquipment] = useState([]);
    const [selectedEquipmentId, setSelectedEquipmentId] = useState('');
    const [notes, setNotes] = useState('');
    // TODO: Replace with actual logged-in user from auth context
    const currentUser = 'System User';
    const { loading, requestHandlerFunction } = useApi();

    // Fetch available equipment for this WO's step
    useEffect(() => {
        const load = async () => {
            const res = await requestHandlerFunction(() => getWOPlanningInfo(workOrder.productionOrderId));
            if (res.success) {
                const data = res.data?.data?.data || res.data?.data;
                // Find the step matching this WO
                const step = data?.steps?.find(s => s.processRouteStepId === workOrder.processRouteStepId 
                    || s.stepNumber === workOrder.stepNumber);
                if (step) {
                    // Filter equipment that belongs to the WO's work center (or show all)
                    setEquipment(step.linkedEquipment || []);
                }
            }
        };
        load();
    }, [workOrder.productionOrderId]);

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
                    <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-slate-100"><X size={20} /></button>
                </div>

                <form onSubmit={handleSubmit} className="p-5 space-y-5">
                    {/* WO Info */}
                    <div className="bg-blue-50 rounded-xl p-3 text-sm">
                        <span className="font-bold text-blue-700">{workOrder.workOrderNumber}</span>
                        <span className="text-slate-500 ml-2">Step #{workOrder.stepNumber} — {workOrder.operationName}</span>
                        <span className="text-slate-500 ml-2">| Qty: {workOrder.quantityPlanned}</span>
                    </div>

                    {/* Equipment Selection */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Select Equipment *</label>
                        {equipment.length === 0 ? (
                            <div className="text-sm text-slate-400 py-2 flex items-center gap-2">
                                {loading ? <><Loader size={14} className="animate-spin" /> Loading equipment...</> : 'No compatible equipment found'}
                            </div>
                        ) : (
                            <SearchSelect
                                value={selectedEquipmentId}
                                displayValue={(() => { const eq = equipment.find(e => e.equipmentId === selectedEquipmentId); return eq ? `${eq.equipmentCode} — ${eq.equipmentName} (${eq.workCenterCode})` : ''; })()}
                                placeholder="Choose equipment..."
                                items={equipment}
                                title="Select Equipment"
                                displayFields={[
                                    { key: 'equipmentCode', label: 'Code', width: '25%', bold: true },
                                    { key: 'equipmentName', label: 'Name', width: '40%' },
                                    { key: 'workCenterCode', label: 'Work Center', width: '20%' },
                                    { key: 'status', label: 'Status', width: '15%' },
                                ]}
                                searchKeys={['equipmentCode', 'equipmentName', 'workCenterCode']}
                                valueKey="equipmentId"
                                onSelect={(eq) => setSelectedEquipmentId(eq.equipmentId)}
                            />
                        )}
                    </div>

                    {/* Equipment cost info */}
                    {selectedEq && (
                        <div className="text-xs bg-slate-50 rounded-lg p-2 text-slate-500">
                            Cost: ₹{selectedEq.costPerHour}/hr | Work Center: {selectedEq.workCenterCode}
                        </div>
                    )}

                    {/* Activated By (auto — from logged-in user) */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Activated By</label>
                        <div className="w-full border rounded-lg px-3 py-2 text-sm bg-slate-50 text-slate-600">
                            {currentUser}
                            <span className="text-xs text-slate-400 ml-2">(auto — logged-in user)</span>
                        </div>
                    </div>

                    {/* Notes */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Notes</label>
                        <input type="text" value={notes} onChange={(e) => setNotes(e.target.value)}
                            className="w-full border rounded-lg px-3 py-2 text-sm" placeholder="Optional" />
                    </div>

                    {/* Submit */}
                    <div className="flex justify-end gap-3 pt-3 border-t">
                        <button type="button" onClick={onClose} className="px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">Cancel</button>
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
