import React, { useState, useEffect } from 'react';
import { X, CheckCircle, AlertTriangle } from 'lucide-react';

const CompleteOrderModal = ({ isOpen, onClose, order, isLoading, onConfirm }) => {
    const [producedQty, setProducedQty] = useState('');
    const [scrapQty, setScrapQty] = useState(0);

    useEffect(() => {
        const setOrderValues = () => {
            if (isOpen && order) {
                setProducedQty(order.batchQuantity); // Default to full batch
                setScrapQty(0);
            }
        };

        setOrderValues();
    }, [isOpen, order]);

    if (!isOpen || !order) return null;

    const handleConfirm = () => {
        onConfirm(order.productionOrderId, producedQty, scrapQty);

    };

    return (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50 animate-in fade-in">
            <div className="bg-white rounded-lg shadow-xl w-full max-w-md overflow-hidden">
                <div className="flex justify-between items-center p-4 border-b bg-green-50">
                    <h3 className="font-bold text-lg text-green-800 flex items-center gap-2">
                        <CheckCircle size={20} />
                        Complete Order #{order.productionOrderId}
                    </h3>
                    <button onClick={onClose} className="text-gray-500 hover:text-gray-700">
                        <X size={20} />
                    </button>
                </div>

                <div className="p-6 space-y-4">
                    <div className="bg-gray-50 p-3 rounded border text-sm text-gray-600">
                        <p><strong>Product:</strong> {order.productName}</p>
                        <p><strong>Target Batch Quantity:</strong> {order.batchQuantity}</p>
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Actual Produced Quantity</label>
                        <input 
                            type="number" 
                            className="w-full border rounded-md p-2 focus:ring-2 focus:ring-green-500 outline-none"
                            value={producedQty}
                            onChange={(e) => setProducedQty(e.target.value)}
                            min="0"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Scrap / Defective Quantity</label>
                        <input 
                            type="number" 
                            className="w-full border rounded-md p-2 focus:ring-2 focus:ring-red-500 outline-none"
                            value={scrapQty}
                            onChange={(e) => setScrapQty(e.target.value)}
                            min="0"
                        />
                        {parseInt(scrapQty) > 0 && (
                            <p className="text-xs text-red-500 mt-1 flex items-center gap-1">
                                <AlertTriangle size={12}/> {scrapQty} units will be recorded as scrap.
                            </p>
                        )}
                    </div>
                </div>

                <div className="p-4 border-t bg-gray-50 flex justify-end gap-3">
                    <button onClick={onClose} className="px-4 py-2 text-gray-600 hover:bg-gray-200 rounded-md">Cancel</button>
                    <button 
                        onClick={handleConfirm} 
                        disabled={isLoading || producedQty === ''}
                        className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50"
                    >
                        {isLoading ? 'Completing...' : 'Confirm Completion'}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default CompleteOrderModal;
