import React, { useState } from 'react';
import { X, Truck } from 'lucide-react';
import toast from 'react-hot-toast';
import { dispatchItems } from '../../api/salesOrderService';

const DispatchModal = ({ order, onClose, onDispatched }) => {
    const [quantities, setQuantities] = useState(
        (order.items || []).reduce((acc, item) => {
            acc[item.productId] = 0;
            return acc;
        }, {})
    );
    const [submitting, setSubmitting] = useState(false);

    const updateQty = (productId, value) => {
        setQuantities(prev => ({ ...prev, [productId]: parseInt(value) || 0 }));
    };

    const handleSubmit = async () => {
        const dispatchItemsList = Object.entries(quantities)
            .filter(([_, qty]) => qty > 0)
            .map(([productId, qty]) => ({ productId, quantityToDispatch: qty }));

        if (dispatchItemsList.length === 0) {
            return toast.error('Enter quantity to dispatch');
        }

        setSubmitting(true);
        try {
            await dispatchItems(order.orderId, { items: dispatchItemsList });
            toast.success('Items dispatched!');
            onDispatched();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Failed to dispatch');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-2xl">
                <div className="flex items-center justify-between p-5 border-b border-slate-200">
                    <div>
                        <h2 className="text-lg font-bold text-slate-800">Dispatch Items</h2>
                        <p className="text-sm text-slate-500">{order.orderNumber} — {order.customerName}</p>
                    </div>
                    <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-slate-100">
                        <X size={20} />
                    </button>
                </div>

                <div className="p-5">
                    <table className="w-full">
                        <thead className="bg-slate-50">
                            <tr>
                                <th className="text-left px-3 py-2 text-xs font-semibold text-slate-500">Product</th>
                                <th className="text-right px-3 py-2 text-xs font-semibold text-slate-500">Available</th>
                                <th className="text-right px-3 py-2 text-xs font-semibold text-slate-500">Remaining</th>
                                <th className="text-right px-3 py-2 text-xs font-semibold text-slate-500">Can Dispatch</th>
                                <th className="text-center px-3 py-2 text-xs font-semibold text-slate-500">Qty to Dispatch</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-100">
                            {(order.items || []).map(item => (
                                <tr key={item.productId} className="hover:bg-slate-50">
                                    <td className="px-3 py-3">
                                        <p className="text-sm font-medium">{item.productName}</p>
                                        <p className="text-xs text-slate-400">{item.productCode}</p>
                                    </td>
                                    <td className="px-3 py-3 text-sm text-right text-blue-600 font-medium">
                                        {item.availableInInventory}
                                    </td>
                                    <td className="px-3 py-3 text-sm text-right text-orange-600 font-medium">
                                        {item.remaining}
                                    </td>
                                    <td className="px-3 py-3 text-sm text-right text-green-600 font-medium">
                                        {item.canDispatch}
                                    </td>
                                    <td className="px-3 py-3 text-center">
                                        <input
                                            type="number"
                                            min="0"
                                            max={item.canDispatch}
                                            value={quantities[item.productId] || 0}
                                            onChange={(e) => updateQty(item.productId, e.target.value)}
                                            className="w-20 px-2 py-1.5 border border-slate-300 rounded-lg text-sm text-center"
                                        />
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>

                <div className="flex justify-end gap-3 p-5 border-t border-slate-100">
                    <button onClick={onClose} className="px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm font-medium">
                        Cancel
                    </button>
                    <button
                        onClick={handleSubmit}
                        disabled={submitting}
                        className="flex items-center gap-2 px-5 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg text-sm font-medium disabled:opacity-50"
                    >
                        <Truck size={16} /> {submitting ? 'Dispatching...' : 'Dispatch'}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default DispatchModal;
