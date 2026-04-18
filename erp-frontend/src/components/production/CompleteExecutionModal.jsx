import React, { useState } from 'react';
import { X, CheckCircle, Loader } from 'lucide-react';
import useApi from '../../hooks/useApi';
import { completeExecution } from '../../api/productionService';

const CompleteExecutionModal = ({ workOrder, execution, onClose, onCompleted }) => {
    const [qtyProduced, setQtyProduced] = useState('');
    const [qtyScrap, setQtyScrap] = useState('0');
    const [notes, setNotes] = useState('');
    const { loading, requestHandlerFunction } = useApi();

    if (!execution) return null;

    const remaining = workOrder.quantityPlanned - workOrder.quantityCompleted;

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!qtyProduced) return;

        const payload = {
            quantityProduced: parseFloat(qtyProduced),
            quantityScrap: parseFloat(qtyScrap) || 0,
            notes: notes || null,
        };

        const res = await requestHandlerFunction(
            () => completeExecution(workOrder.workOrderId, execution.executionId, payload),
            'Execution completed!'
        );
        if (res.success) onCompleted();
    };

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
                {/* Header */}
                <div className="flex items-center justify-between p-5 border-b">
                    <h2 className="text-lg font-bold text-slate-800 flex items-center gap-2">
                        <CheckCircle size={20} className="text-green-500" /> Complete Execution
                    </h2>
                    <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-slate-100"><X size={20} /></button>
                </div>

                <form onSubmit={handleSubmit} className="p-5 space-y-5">
                    {/* Execution Info */}
                    <div className="bg-green-50 rounded-xl p-3 text-sm space-y-1">
                        <div><span className="text-green-400 text-xs">WO:</span> <span className="font-bold text-green-700">{workOrder.workOrderNumber}</span></div>
                        <div><span className="text-green-400 text-xs">Equipment:</span> <span className="font-medium">{execution.equipmentCode} — {execution.equipmentName}</span></div>
                        <div><span className="text-green-400 text-xs">Operator:</span> <span>{execution.activatedBy}</span></div>
                        <div><span className="text-green-400 text-xs">Started:</span> <span>{new Date(execution.activatedAt).toLocaleString()}</span></div>
                    </div>

                    {/* Progress Info */}
                    <div className="grid grid-cols-3 gap-3 text-center text-sm">
                        <div className="bg-blue-50 rounded-lg p-2">
                            <p className="text-xs text-blue-400">Planned</p>
                            <p className="font-bold text-blue-700">{workOrder.quantityPlanned}</p>
                        </div>
                        <div className="bg-green-50 rounded-lg p-2">
                            <p className="text-xs text-green-400">Completed</p>
                            <p className="font-bold text-green-700">{workOrder.quantityCompleted}</p>
                        </div>
                        <div className="bg-yellow-50 rounded-lg p-2">
                            <p className="text-xs text-yellow-500">Remaining</p>
                            <p className="font-bold text-yellow-700">{remaining}</p>
                        </div>
                    </div>

                    {/* Qty Produced */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Quantity Produced *</label>
                        <input type="number" value={qtyProduced} onChange={(e) => setQtyProduced(e.target.value)}
                            min="0" step="1"
                            className="w-full border rounded-lg px-3 py-2 text-sm" required
                            placeholder={`Max remaining: ${remaining}`} />
                    </div>

                    {/* Qty Scrap */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Quantity Scrap</label>
                        <input type="number" value={qtyScrap} onChange={(e) => setQtyScrap(e.target.value)}
                            min="0" step="1"
                            className="w-full border rounded-lg px-3 py-2 text-sm" placeholder="0" />
                    </div>

                    {/* Notes */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Notes</label>
                        <input type="text" value={notes} onChange={(e) => setNotes(e.target.value)}
                            className="w-full border rounded-lg px-3 py-2 text-sm" placeholder="e.g. 2 defective units" />
                    </div>

                    {/* Submit */}
                    <div className="flex justify-end gap-3 pt-3 border-t">
                        <button type="button" onClick={onClose} className="px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">Cancel</button>
                        <button type="submit" disabled={loading || !qtyProduced}
                            className="px-5 py-2 bg-green-600 text-white rounded-lg text-sm hover:bg-green-700 disabled:opacity-50 flex items-center gap-2">
                            {loading ? <Loader size={14} className="animate-spin" /> : <CheckCircle size={14} />}
                            Complete Execution
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CompleteExecutionModal;
