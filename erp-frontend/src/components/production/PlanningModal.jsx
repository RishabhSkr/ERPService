import { useState, useEffect } from 'react';
import { X, AlertTriangle, Package, CalendarClock, Beaker } from 'lucide-react';
import { getEfficiencyWarning } from '../../utility/batchUtils';
import SmartBatchCard from './SmartBatchCard';

const PlanningModal = ({ isOpen, onClose, data, isLoading, onConfirm }) => {
    const [batchQty, setBatchQty] = useState('');
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [forceCreate, setForceCreate] = useState(false);
    const efficiencyWarning = getEfficiencyWarning(data, batchQty);
    // Reset form when modal opens
    useEffect(() => {
        const resetForm = () => {
            if (isOpen) {
                // Default dates: Start = Today, End = Today (Optional)
                const today = new Date().toISOString().split('T')[0];
                setBatchQty('');
                setStartDate(today);
                setEndDate(today);
            }
        };

        resetForm();
    }, [isOpen]);

    if (!isOpen) return null;

    const maxAllowed = data ? Math.min(data.remainingQuantity, data.maxPossibleByMaterial) : 0;
    
    // Validation: Qty > 0, dates must be selected
    const isValid = batchQty > 0 && batchQty <= maxAllowed && startDate && endDate;

    const handleConfirm = () => {
        onConfirm(batchQty, startDate, endDate, forceCreate);
    };

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50 animate-in fade-in">
            <div className="bg-white rounded-lg shadow-xl w-full max-w-4xl overflow-hidden">
                
                {/* Header */}
                <div className="flex justify-between items-center p-4 border-b bg-slate-50">
                    <h3 className="font-bold text-lg text-slate-800">Plan Production Batch</h3>
                    <button onClick={onClose} className="text-gray-500 hover:text-gray-700">
                        <X size={20} />
                    </button>
                </div>

                {/* Body */}
                <div className="p-6 flex gap-6">
                    {isLoading ? (
                        <div className="flex-1">
                            <div className="flex flex-col items-center justify-center py-8 text-gray-500">
                                <div className="animate-spin h-8 w-8 border-4 border-blue-500 border-t-transparent rounded-full mb-2"></div>
                                Checking Inventory & Capacity...
                            </div>
                        </div>
                    ) : data ? (
                        <>
                        {/* LEFT: Form */}
                        <div className="flex-1 space-y-6">
                            {/* Product Info */}
                            <div className="flex items-center gap-3 bg-blue-50 p-3 rounded-md border border-blue-100">
                                <div className="bg-blue-200 p-2 rounded text-blue-700">
                                    <Package size={20} />
                                </div>
                                <div>
                                    <p className="text-xs text-blue-600 font-bold uppercase">Product</p>
                                    <p className="font-semibold text-slate-800">{data.productName}</p>
                                    <p className="text-xs text-slate-500">Order #{data.salesOrderId}</p>
                                </div>
                                <div className="ml-auto text-right">
                                    <p className="text-xs text-gray-500">Pending Qty</p>
                                    <p className="font-bold text-lg text-slate-800">{data.remainingQuantity}</p>
                                </div>
                            </div>
                            {/* Constraints Grid */}
                            <div className="grid grid-cols-2 gap-4">
                                <div className="border p-3 rounded bg-gray-50">
                                    <div className="flex items-center gap-2 mb-1 text-gray-500 text-xs font-semibold uppercase">
                                        <CalendarClock size={14} /> Machine Capacity
                                    </div>
                                    <span className="text-lg font-bold text-slate-700">{data.machineDailyCapacity}</span> units/day
                                </div>
                                <div className={`border p-3 rounded ${data.maxPossibleByMaterial === 0 ? 'bg-red-50 border-red-200' : 'bg-green-50 border-green-200'}`}>
                                    <div className="flex items-center gap-2 mb-1 text-gray-500 text-xs font-semibold uppercase">
                                        <Beaker size={14} /> Material Limit
                                    </div>
                                    <span className={`text-lg font-bold ${data.maxPossibleByMaterial === 0 ? 'text-red-600' : 'text-green-700'}`}>
                                        {data.maxPossibleByMaterial}{data.limitingMaterial ? ` (${data.limitingMaterial})` : ''}
                                    </span>
                                </div>
                            </div>

                            {/* BOM Materials — which recipe will be used */}
                            {data.bomMaterials && data.bomMaterials.length > 0 && (
                                <div className="border rounded-md overflow-hidden">
                                    <div className="bg-indigo-50 px-3 py-2 border-b border-indigo-100 flex justify-between items-center">
                                        <span className="text-xs font-bold text-indigo-700 uppercase tracking-wide">📋 BOM Recipe</span>
                                        <span className="text-xs text-indigo-500 font-mono">{data.bomNumber} v{data.bomVersion}</span>
                                    </div>
                                    <table className="w-full text-sm">
                                        <thead>
                                            <tr className="bg-gray-50 text-xs text-gray-500 uppercase">
                                                <th className="text-left px-3 py-1.5">Material</th>
                                                <th className="text-right px-3 py-1.5">Qty/Unit</th>
                                                <th className="text-right px-3 py-1.5">UOM</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {data.bomMaterials.map((m, idx) => (
                                                <tr key={idx} className="border-t border-gray-100 hover:bg-gray-50">
                                                    <td className="px-3 py-1.5 text-slate-700">{m.rawMaterialName}</td>
                                                    <td className="px-3 py-1.5 text-right font-mono text-slate-600">{m.quantityRequired}</td>
                                                    <td className="px-3 py-1.5 text-right text-gray-400">{m.uom || '—'}</td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                </div>
                            )}
                            {data.maxPossibleByMaterial < data.remainingQuantity && (
                                <div className="flex gap-2 items-start text-sm text-amber-700 bg-amber-50 p-3 rounded border border-amber-200">
                                    <AlertTriangle size={18} className="mt-0.5 shrink-0" />
                                    <span>
                                        <strong>Bottleneck:</strong> Production limited by <u>{data.limitingMaterial}</u>. You cannot produce full pending quantity.
                                    </span>
                                </div>
                            )}

                            {/* Inputs Section */}
                            <div className="space-y-4">
                                {/* Quantity Input */}
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Batch Quantity</label>
                                    <input 
                                        type="number" 
                                        className="w-full border rounded-md p-2 focus:ring-2 focus:ring-blue-500 outline-none"
                                        placeholder={`Max: ${maxAllowed}`}
                                        max={maxAllowed}
                                        value={batchQty}
                                        onChange={(e) => setBatchQty(e.target.value)}
                                    />
                                    <p className="text-xs text-gray-400 mt-1">Available to produce: {maxAllowed}</p>
                                    
                                    {/* Efficiency Warning */}
                                    {efficiencyWarning && (
                                        <div className="bg-amber-50 p-3 rounded-lg border border-amber-300 mt-2">
                                            {efficiencyWarning.type === 'lowEfficiency' ? (
                                                <p className="text-amber-800 text-sm font-medium">
                                                    ⚠️ Last batch efficiency: {efficiencyWarning.efficiency}% 
                                                    ({efficiencyWarning.lastBatch}/{efficiencyWarning.capacity} per day)
                                                </p>
                                            ) : (
                                                <p className="text-red-700 text-sm font-medium">
                                                    ⚠️ Cannot produce more than available: {efficiencyWarning.availableQtyToProduce}
                                                </p>
                                            )}
                                        </div>
                                    )}
                                    {/* Force Create — works for both efficiency warning & buffer cap */}
                                    <label className="flex items-center gap-2 mt-2 cursor-pointer">
                                        <input 
                                            type="checkbox"
                                            checked={forceCreate}
                                            onChange={(e) => setForceCreate(e.target.checked)}
                                            className="w-4 h-4 accent-amber-600"
                                        />
                                        <span className="text-sm text-gray-600">Force Create (ignore batch efficiency warning)</span>
                                    </label>
                                </div>

                                {/* Date Inputs */}
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
                                        <input 
                                            type="date"
                                            className="w-full border rounded-md p-2 focus:ring-2 focus:ring-blue-500 outline-none"
                                            value={startDate}
                                            onChange={(e) => setStartDate(e.target.value)}
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">End Date</label>
                                        <input 
                                            type="date"
                                            className="w-full border rounded-md p-2 focus:ring-2 focus:ring-blue-500 outline-none"
                                            value={endDate}
                                            min={startDate}
                                            onChange={(e) => setEndDate(e.target.value)}
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* RIGHT: Smart Batch Card */}
                        <div className="w-72">
                            <SmartBatchCard planningInfo={data} onUseSuggestion={(qty) => setBatchQty(qty)} />
                        </div>
                        </>
                    ) : (
                        <p className="text-red-500">Failed to load planning info.</p>
                    )}
                </div>

                {/* Footer */}
                <div className="p-4 border-t bg-gray-50 flex justify-end gap-3">
                    <button onClick={onClose} className="px-4 py-2 text-gray-600 hover:bg-gray-200 rounded-md">Cancel</button>
                    <button 
                        onClick={handleConfirm} 
                        disabled={!isValid || isLoading}
                        className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        Confirm Plan
                    </button>
                </div>
            </div>
        </div>
    );
};

export default PlanningModal;