import React, { useEffect } from 'react';
import { X, Truck, PackageCheck, Loader } from 'lucide-react';

import { useSales } from '../../hooks/useSales';

const STATUS_COLORS = {
    Pending: 'bg-gray-100 text-gray-700',
    Confirmed: 'bg-blue-100 text-blue-700',
    InProduction: 'bg-yellow-100 text-yellow-800',
    Shipped: 'bg-green-100 text-green-700',
    Delivered: 'bg-emerald-100 text-emerald-700',
    Cancelled: 'bg-red-100 text-red-700',
};

/**
 * OrderDetailModal
 * 
 * Props:
 * - orderId: string (the order ID to fetch details for)
 * - onClose: () => void
 * - onRefresh: () => void (refresh parent list)
 */
const OrderDetailModal = ({ orderId, onClose, onRefresh }) => {
    //  Hook always called at top — before any returns
    const { orderDetail, loading, fetchOrderById, confirmOrder, handleDeliver,forceComplete } = useSales();

    // Fetch full order detail when orderId changes
    useEffect(() => {
        if (orderId) {
            fetchOrderById(orderId);
        }
    }, [orderId]);

    // No orderId? Don't render
    if (!orderId) return null;

    const order = orderDetail;

    // Loading state
    if (loading || !order) {
        return (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                <div className="bg-white rounded-2xl shadow-xl p-10 flex flex-col items-center gap-3">
                    <Loader size={32} className="animate-spin text-blue-500" />
                    <p className="text-sm text-slate-500">Loading order details...</p>
                </div>
            </div>
        );
    }

    const handleConfirm = async () => {
        const success = await confirmOrder(order.id);
        if (success) {
            onRefresh?.();
            onClose();
        }
    };

    const handleDelivered = async () => {
        const success = await handleDeliver(order.id);
        if (success) {
            onRefresh?.();
            onClose();
        }
    };

    const items = order.items || [];

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-3xl max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="flex items-center justify-between p-5 border-b border-slate-200">
                    <div>
                        <h2 className="text-lg font-bold text-slate-800">{order.orderNumber}</h2>
                        <p className="text-sm text-slate-500">{order.customerName}</p>
                    </div>
                    <div className="flex items-center gap-3">
                        <span className={`px-3 py-1 rounded-full text-xs font-medium ${STATUS_COLORS[order.orderStatus]}`}>
                            {order.orderStatus}
                        </span>
                        <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-slate-100">
                            <X size={20} />
                        </button>
                    </div>
                </div>

                {/* Order Info */}
                <div className="p-5 grid grid-cols-3 gap-4 border-b border-slate-100">
                    <div>
                        <p className="text-xs text-slate-400 uppercase font-medium">Order Date</p>
                        <p className="text-sm font-medium text-slate-700 mt-1">
                            {new Date(order.orderDate).toLocaleDateString('en-IN')}
                        </p>
                    </div>
                    <div>
                        <p className="text-xs text-slate-400 uppercase font-medium">Total Amount</p>
                        <p className="text-sm font-bold text-slate-800 mt-1">
                            ₹{order.totalAmount?.toLocaleString('en-IN')}
                        </p>
                    </div>
                    <div>
                        <p className="text-xs text-slate-400 uppercase font-medium">Notes</p>
                        <p className="text-sm text-slate-600 mt-1">{order.notes || '-'}</p>
                    </div>
                </div>

                {/* Items Table */}
                <div className="p-5">
                    <h3 className="text-sm font-semibold text-slate-700 mb-3">
                        Order Items ({items.length})
                    </h3>
                    {items.length === 0 ? (
                        <p className="text-sm text-slate-400 text-center py-4">No items found</p>
                    ) : (
                        <table className="w-full">
                            <thead className="bg-slate-50">
                                <tr>
                                    <th className="text-left px-3 py-2 text-xs font-semibold text-slate-500">Product</th>
                                    <th className="text-right px-3 py-2 text-xs font-semibold text-slate-500">Ordered</th>
                                    <th className="text-right px-3 py-2 text-xs font-semibold text-slate-500">Produced</th>
                                    <th className="text-right px-3 py-2 text-xs font-semibold text-slate-500">Dispatched</th>
                                    <th className="text-right px-3 py-2 text-xs font-semibold text-slate-500">Remaining</th>
                                    <th className="text-right px-3 py-2 text-xs font-semibold text-slate-500">Progress</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-slate-100">
                                {items.map((item, i) => {
                                    const remaining = item.quantity - (item.quantityDispatched || 0);
                                    const progress = item.quantity > 0 
                                        ? Math.round(((item.quantityProduced || 0) / item.quantity) * 100) 
                                        : 0;
                                    return (
                                        <tr key={item.id || i} className="hover:bg-slate-50">
                                            <td className="px-3 py-2.5">
                                                <p className="text-sm font-medium text-slate-700">{item.productName}</p>
                                                <p className="text-xs text-slate-400">{item.productCode}</p>
                                            </td>
                                            <td className="px-3 py-2.5 text-sm text-right">{item.quantity}</td>
                                            <td className="px-3 py-2.5 text-sm text-right font-medium text-blue-600">
                                                {item.quantityProduced || 0}
                                            </td>
                                            <td className="px-3 py-2.5 text-sm text-right font-medium text-green-600">
                                                {item.quantityDispatched || 0}
                                            </td>
                                            <td className="px-3 py-2.5 text-sm text-right font-medium text-orange-600">
                                                {remaining}
                                            </td>
                                            <td className="px-3 py-2.5 text-right">
                                                <div className="flex items-center justify-end gap-2">
                                                    <div className="w-16 h-1.5 bg-slate-200 rounded-full overflow-hidden">
                                                        <div 
                                                            className="h-full bg-blue-500 rounded-full transition-all"
                                                            style={{ width: `${Math.min(progress, 100)}%` }}
                                                        />
                                                    </div>
                                                    <span className="text-xs text-slate-500">{progress}%</span>
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    )}
                </div>

                {/* Actions */}
                <div className="flex justify-end gap-3 p-5 border-t border-slate-100">
                    {order.orderStatus === 'Pending' && (
                        <button
                            onClick={handleConfirm}
                            disabled={loading}
                            className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium disabled:opacity-50"
                        >
                            <PackageCheck size={16} /> Confirm Order
                        </button>
                    )}

                    {/* FORCE COMPLETE BUTTON - Only for Confirmed orders */}
                    {order.orderStatus === 'InProduction' && (
                        <button
                            onClick={async () => {
                                if(window.confirm('Are you sure you want to force complete this order?')) {
                                    const success = await forceComplete(order.id);
                                    if(success) { onRefresh?.(); onClose(); }
                                }
                            }}
                            disabled={loading}
                            className="flex items-center gap-2 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg text-sm font-medium disabled:opacity-50"
                        >
                            <PackageCheck size={16} /> Force Complete
                        </button>
                    )}

                    {order.orderStatus === 'Shipped' && (
                        <button
                            onClick={handleDelivered}
                            disabled={loading}
                            className="flex items-center gap-2 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-sm font-medium disabled:opacity-50"
                        >
                            <Truck size={16} /> Mark Delivered
                        </button>
                    )}
                    <button
                        onClick={onClose}
                        className="px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm font-medium"
                    >
                        Close
                    </button>
                </div>
            </div>
        </div>
    );
};

export default OrderDetailModal;
