import { useState, useEffect } from 'react';
import { X, Package, CalendarClock, CheckCircle, Layers, AlertCircle } from 'lucide-react';

/**
 * PlanningModal — Approval Modal for PendingRequests
 * 
 * Shows all items from a Sales Order PendingRequest.
 * User sets start date + priority → "Approve" creates 1 PO per item.
 * 
 * Props:
 *   isOpen: boolean
 *   onClose: () => void
 *   data: PendingRequest object { id, salesOrderId, salesOrderNumber, customerName, status, items[], createdAt }
 *   isLoading: boolean
 *   onConfirm: (startDate, endDate, priority) => void
 */
const PlanningModal = ({ isOpen, onClose, data, isLoading, onConfirm }) => {
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [priority, setPriority] = useState(3);

    // Reset form when modal opens
    useEffect(() => {
        if (isOpen) {
            const today = new Date().toISOString().split('T')[0];
            setStartDate(today);
            setEndDate(today);
            setPriority(3);
        }
    }, [isOpen]);

    if (!isOpen) return null;

    const items = data?.items || [];
    const isValid = startDate && endDate && items.length > 0;

    const handleConfirm = () => {
        onConfirm(startDate, endDate, priority);
    };

    const priorityLabels = {
        1: { label: 'Urgent', color: 'text-red-600' },
        2: { label: 'High', color: 'text-orange-600' },
        3: { label: 'Normal', color: 'text-blue-600' },
        4: { label: 'Low', color: 'text-slate-600' },
        5: { label: 'Lowest', color: 'text-slate-400' },
    };

    return (
        <div className="fixed inset-0 bg-black/50 flex justify-center items-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl overflow-hidden">
                
                {/* Header */}
                <div className="flex justify-between items-center px-6 py-4 border-b bg-gradient-to-r from-blue-50 to-indigo-50">
                    <div>
                        <h3 className="font-bold text-lg text-slate-800 flex items-center gap-2">
                            <CheckCircle size={20} className="text-blue-600" />
                            Approve Production Request
                        </h3>
                        <p className="text-xs text-slate-500 mt-0.5">
                            This will create {items.length} Production Order{items.length > 1 ? 's' : ''}
                        </p>
                    </div>
                    <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-white/80 text-gray-500 hover:text-gray-700 transition">
                        <X size={20} />
                    </button>
                </div>

                {/* Body */}
                <div className="p-6 space-y-5">
                    {/* SO Info */}
                    {data && (
                        <div className="flex items-center gap-4 bg-slate-50 p-4 rounded-xl border border-slate-200">
                            <div className="bg-blue-100 p-2.5 rounded-lg text-blue-600">
                                <Package size={20} />
                            </div>
                            <div className="flex-1 min-w-0">
                                <p className="text-xs text-slate-400 font-semibold uppercase">Sales Order</p>
                                <p className="font-bold text-slate-800">{data.salesOrderNumber || `SO #${String(data.salesOrderId || '').slice(0, 8)}`}</p>
                                {data.customerName && (
                                    <p className="text-xs text-slate-500">Customer: {data.customerName}</p>
                                )}
                            </div>
                            <div className="text-right flex-shrink-0">
                                <p className="text-xs text-slate-400">Created</p>
                                <p className="text-sm font-medium text-slate-600">
                                    {data.createdAt ? new Date(data.createdAt).toLocaleDateString() : '-'}
                                </p>
                            </div>
                        </div>
                    )}

                    {/* Items Table */}
                    <div className="border rounded-xl overflow-hidden">
                        <div className="bg-indigo-50 px-4 py-2.5 border-b border-indigo-100 flex items-center justify-between">
                            <span className="text-xs font-bold text-indigo-700 uppercase tracking-wide flex items-center gap-1.5">
                                <Layers size={12} /> Items to Produce
                            </span>
                            <span className="bg-indigo-200 text-indigo-800 px-2 py-0.5 rounded-full text-[10px] font-bold">
                                {items.length} item{items.length > 1 ? 's' : ''}
                            </span>
                        </div>
                        {items.length > 0 ? (
                            <table className="w-full text-sm">
                                <thead>
                                    <tr className="bg-slate-50 text-xs text-slate-500 uppercase">
                                        <th className="text-left px-4 py-2">#</th>
                                        <th className="text-left px-4 py-2">Product</th>
                                        <th className="text-right px-4 py-2">Quantity</th>
                                        <th className="text-right px-4 py-2">Unit Price</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {items.map((item, idx) => (
                                        <tr key={item.id || idx} className="border-t border-slate-100 hover:bg-slate-50">
                                            <td className="px-4 py-2.5 text-slate-400 font-mono text-xs">{idx + 1}</td>
                                            <td className="px-4 py-2.5">
                                                <span className="font-medium text-slate-800">{item.productName}</span>
                                                {item.productCode && (
                                                    <span className="ml-2 text-xs text-slate-400 font-mono">{item.productCode}</span>
                                                )}
                                            </td>
                                            <td className="px-4 py-2.5 text-right font-bold text-slate-700">{item.quantity}</td>
                                            <td className="px-4 py-2.5 text-right text-slate-500">
                                                {item.unitPrice ? `₹${Number(item.unitPrice).toLocaleString()}` : '-'}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        ) : (
                            <div className="p-6 text-center text-slate-400 text-sm flex flex-col items-center gap-2">
                                <AlertCircle size={24} className="opacity-20" />
                                <p>No items found in this request.</p>
                            </div>
                        )}
                    </div>

                    {/* Info Banner */}
                    <div className="bg-blue-50 border border-blue-200 rounded-lg px-4 py-3 text-xs text-blue-700">
                        <strong>ℹ️ What happens on Approve:</strong> One Production Order will be created for each item above.
                        BOM will be auto-looked up for each product.
                    </div>

                    {/* Scheduling Inputs */}
                    <div className="grid grid-cols-3 gap-4">
                        <div>
                            <label className="block text-xs font-semibold text-slate-500 uppercase mb-1.5">
                                <CalendarClock size={12} className="inline mr-1" /> Start Date *
                            </label>
                            <input 
                                type="date"
                                className="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
                                value={startDate}
                                onChange={(e) => setStartDate(e.target.value)}
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-semibold text-slate-500 uppercase mb-1.5">End Date *</label>
                            <input 
                                type="date"
                                className="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
                                value={endDate}
                                min={startDate}
                                onChange={(e) => setEndDate(e.target.value)}
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-semibold text-slate-500 uppercase mb-1.5">Priority</label>
                            <select 
                                value={priority}
                                onChange={(e) => setPriority(Number(e.target.value))}
                                className="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none bg-white"
                            >
                                {Object.entries(priorityLabels).map(([val, meta]) => (
                                    <option key={val} value={val}>{val} — {meta.label}</option>
                                ))}
                            </select>
                        </div>
                    </div>
                </div>

                {/* Footer */}
                <div className="px-6 py-4 border-t bg-slate-50 flex justify-between items-center">
                    <p className="text-xs text-slate-400">
                        {items.length} Production Order{items.length > 1 ? 's' : ''} will be created
                    </p>
                    <div className="flex gap-3">
                        <button onClick={onClose} className="px-4 py-2.5 text-slate-600 hover:bg-slate-200 rounded-lg text-sm font-medium transition">
                            Cancel
                        </button>
                        <button 
                            onClick={handleConfirm}
                            disabled={!isValid || isLoading}
                            className="px-6 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-sm font-medium flex items-center gap-2 transition"
                        >
                            {isLoading ? (
                                <>
                                    <div className="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full" />
                                    Approving...
                                </>
                            ) : (
                                <>
                                    <CheckCircle size={16} />
                                    Approve & Create POs
                                </>
                            )}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default PlanningModal;