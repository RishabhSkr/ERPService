import React, { useState, useEffect } from 'react';
import { ShoppingCart, Plus, Eye, CheckCircle, XCircle, RefreshCw } from 'lucide-react';
import { useSales } from '../../hooks/useSales';
import CreateOrderModal from '../../components/sales/CreateOrderModal';
import OrderDetailModal from '../../components/sales/OrderDetailModal';

const STATUS_COLORS = {
    Pending: 'bg-gray-100 text-gray-700',
    Confirmed: 'bg-blue-100 text-blue-700',
    InProduction: 'bg-yellow-100 text-yellow-800',
    Shipped: 'bg-green-100 text-green-700',
    Delivered: 'bg-emerald-100 text-emerald-700',
    Cancelled: 'bg-red-100 text-red-700',
};

const SalesOrders = () => {
    // ─── useSales hook ───
    const { orders, loading, fetchOrders, confirmOrder, handleCancel } = useSales();

    const [statusFilter, setStatusFilter] = useState('');
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [selectedOrderId, setSelectedOrderId] = useState(null);

    // Fetch orders when page or filter changes
    useEffect(() => {
        fetchOrders(page, 10, statusFilter);
    }, [page, statusFilter]);

    const onConfirm = async (id) => {
        const success = await confirmOrder(id);
        if (success) fetchOrders(page, 10, statusFilter);
    };

    const onCancel = async (id) => {
        console.log('Cancel clicked for:', id);
        try {
            const success = await handleCancel(id);
            console.log('Cancel result:', success);
            if (success) {
                fetchOrders(page, 10, statusFilter);
            }
        } catch (err) {
            console.error('Cancel failed:', err);
        }
    };

    const refreshList = () => fetchOrders(page, 10, statusFilter);

    const formatDate = (dateStr) => {
        if (!dateStr) return '-';
        return new Date(dateStr).toLocaleDateString('en-IN', {
            day: '2-digit', month: 'short', year: 'numeric'
        });
    };

    return (
        <div className="p-6">
            {/* Header */}
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <ShoppingCart className="text-blue-500" size={28} />
                    <h1 className="text-2xl font-bold text-slate-800">Sales Orders</h1>
                </div>
                <div className="flex gap-3">
                    <button
                        onClick={refreshList}
                        className="flex items-center gap-2 px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm transition-colors"
                    >
                        <RefreshCw size={16} /> Refresh
                    </button>
                    <button
                        onClick={() => setShowCreateModal(true)}
                        className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
                    >
                        <Plus size={16} /> Create Order
                    </button>
                </div>
            </div>

            {/* Status Filter */}
            <div className="flex gap-2 mb-4 flex-wrap">
                {['', 'Pending', 'Confirmed', 'InProduction', 'Shipped', 'Delivered', 'Cancelled'].map(status => (
                    <button
                        key={status}
                        onClick={() => { setStatusFilter(status); setPage(1); }}
                        className={`px-3 py-1.5 rounded-full text-xs font-medium transition-colors ${
                            statusFilter === status 
                                ? 'bg-blue-600 text-white' 
                                : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                        }`}
                    >
                        {status || 'All'}
                    </button>
                ))}
            </div>

            {/* Orders Table */}
            <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
                <table className="w-full">
                    <thead className="bg-slate-50 border-b border-slate-200">
                        <tr>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Order #</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Customer</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Date</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Status</th>
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Total</th>
                            <th className="text-center px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                        {loading ? (
                            <tr><td colSpan={6} className="text-center py-10 text-slate-400">Loading...</td></tr>
                        ) : orders.length === 0 ? (
                            <tr><td colSpan={6} className="text-center py-10 text-slate-400">No orders found</td></tr>
                        ) : orders.map(order => (
                            <tr key={order.id} className="hover:bg-slate-50 transition-colors">
                                <td className="px-4 py-3 font-mono text-sm font-medium text-slate-800">
                                    {order.orderNumber}
                                </td>
                                <td className="px-4 py-3 text-sm text-slate-600">
                                    {order.customerName || 'N/A'}
                                </td>
                                <td className="px-4 py-3 text-sm text-slate-500">
                                    {formatDate(order.orderDate)}
                                </td>
                                <td className="px-4 py-3">
                                    <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${STATUS_COLORS[order.orderStatus] || 'bg-gray-100'}`}>
                                        {order.orderStatus}
                                    </span>
                                </td>
                                <td className="px-4 py-3 text-sm text-right font-medium text-slate-700">
                                    ₹{order.totalAmount?.toLocaleString('en-IN') || '0'}
                                </td>
                                <td className="px-4 py-3">
                                    <div className="flex items-center justify-center gap-1">
                                        <button 
                                            onClick={() => setSelectedOrderId(order.id)}
                                            className="p-1.5 rounded hover:bg-slate-100 text-slate-500 hover:text-blue-600" 
                                            title="View Details"
                                        >
                                            <Eye size={16} />
                                        </button>
                                        {order.orderStatus === 'Pending' && (
                                            <>
                                                <button 
                                                    onClick={() => onConfirm(order.id)}
                                                    className="p-1.5 rounded hover:bg-green-50 text-slate-500 hover:text-green-600" 
                                                    title="Confirm"
                                                >
                                                    <CheckCircle size={16} />
                                                </button>
                                                <button 
                                                    onClick={() => onCancel(order.id)}
                                                    className="p-1.5 rounded hover:bg-red-50 text-slate-500 hover:text-red-600" 
                                                    title="Cancel"
                                                >
                                                    <XCircle size={16} />
                                                </button>
                                            </>
                                        )}
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
                <div className="flex justify-center gap-2 mt-4">
                    {Array.from({ length: totalPages }, (_, i) => i + 1).map(p => (
                        <button
                            key={p}
                            onClick={() => setPage(p)}
                            className={`px-3 py-1.5 rounded text-sm ${
                                page === p ? 'bg-blue-600 text-white' : 'bg-slate-100 hover:bg-slate-200 text-slate-600'
                            }`}
                        >
                            {p}
                        </button>
                    ))}
                </div>
            )}

            {/* Create Order Modal */}
            {showCreateModal && (
                <CreateOrderModal
                    onClose={() => setShowCreateModal(false)}
                    onCreated={() => { setShowCreateModal(false); refreshList(); }}
                />
            )}

            {/* Order Detail Modal */}
            {selectedOrderId && (
                <OrderDetailModal
                    orderId={selectedOrderId}
                    onClose={() => setSelectedOrderId(null)}
                    onRefresh={refreshList}
                />
            )}
        </div>
    );
};

export default SalesOrders;
