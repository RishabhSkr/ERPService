import { useCallback, useEffect, useState } from 'react';
import { PlayCircle, CheckCircle, XCircle, RotateCcw, Eye, Factory } from 'lucide-react';
import ProductionOrderDetailModal from '../../components/production/ProductionOrderDetailModal';
import FilterBar from '../../components/common/FilterBar';
import useApi from '../../hooks/useApi';
import {
    getAllOrders,
    releaseOrder,
    startOrder,
    cancelOrder,
    forceCompleteOrder,
    retryReservation
} from '../../api/productionService';

const OrderManagement = () => {
    const [orders, setOrders] = useState([]);
    const [filterStatus, setFilterStatus] = useState('All');
    const [filterId, setFilterId] = useState('');
    const [detailOrder, setDetailOrder] = useState(null);

    const { loading: isLoading, requestHandlerFunction } = useApi();

    // Fetch all orders
    const fetchOrders = useCallback(async () => {
        const response = await requestHandlerFunction(() => getAllOrders());
        if (response.success) {
            const backendWrapper = response.data?.data;
            const orderList = backendWrapper?.data || backendWrapper || [];
            setOrders(Array.isArray(orderList) ? orderList : []);
        } else {
            setOrders([]);
        }
    }, [requestHandlerFunction]);

    useEffect(() => {
        fetchOrders();
    }, [fetchOrders]);

    // Filters — null-safe
    const filteredOrders = orders.filter(order => {
        const statusMatch = filterStatus === 'All' || order.status === filterStatus;
        const searchMatch = !filterId
            || (order.orderNumber || '').toLowerCase().includes(filterId.toLowerCase())
            || (order.productName || '').toLowerCase().includes(filterId.toLowerCase())
            || (order.salesOrderNumber || '').toLowerCase().includes(filterId.toLowerCase());
        return statusMatch && searchMatch;
    });

    // ─── PO Lifecycle Actions ───
    const handleRelease = async (id) => {
        const response = await requestHandlerFunction(
            () => releaseOrder(id), 'Order Released!'
        );
        if (response.success) fetchOrders();
    };

    const handleStart = async (id) => {
        const response = await requestHandlerFunction(
            () => startOrder(id), 'Production Started!'
        );
        if (response.success) fetchOrders();
    };

    const handleCancel = async (id) => {
        const reason = prompt('Cancel reason:');
        if (!reason) return;
        const response = await requestHandlerFunction(
            () => cancelOrder(id, reason), 'Order Cancelled.'
        );
        if (response.success) fetchOrders();
    };

    const handleRetryReservation = async (id) => {
        const response = await requestHandlerFunction(
            () => retryReservation(id), 'Reservation retry initiated.'
        );
        if (response.success) fetchOrders();
    };

    const handleForceComplete = async (order) => {
        const confirmed = confirm(
            `Force-complete "${order.orderNumber}"?\n\nQuantities will be auto-summed from all Work Orders.\nUse this if not all WOs are needed (partial completion).`
        );
        if (!confirmed) return;
        const response = await requestHandlerFunction(
            () => forceCompleteOrder(order.id), 'Production Order Completed!'
        );
        if (response.success) fetchOrders();
    };

    const getStatusColor = (status) => {
        switch (status) {
            case 'Created': return 'bg-yellow-100 text-yellow-700 border-yellow-200';
            case 'Released': return 'bg-purple-100 text-purple-700 border-purple-200';
            case 'InProgress': return 'bg-blue-100 text-blue-700 border-blue-200';
            case 'Completed': return 'bg-green-100 text-green-700 border-green-200';
            case 'Cancelled': return 'bg-red-100 text-red-700 border-red-200';
            default: return 'bg-gray-100 text-gray-700 border-gray-200';
        }
    };

    const getReservationColor = (status) => {
        switch (status) {
            case 'Reserved': return 'text-green-600';
            case 'Failed': return 'text-red-500';
            case 'Pending': return 'text-yellow-600';
            default: return 'text-gray-400';
        }
    };

    const formatDate = (d) => {
        if (!d) return '-';
        return new Date(d).toLocaleDateString('en-IN', { day: '2-digit', month: 'short' });
    };

    return (
        <div className="p-6 max-w-7xl mx-auto">
            <div className="mb-6 flex flex-col md:flex-row justify-between items-center gap-4">
                <h1 className="text-2xl font-bold text-slate-800 flex items-center gap-2">
                    <Factory className="text-blue-600" /> PO Management
                </h1>

                <FilterBar
                    value={filterId}
                    onChange={setFilterId}
                    onSearch={() => {}}
                    onClear={() => setFilterId('')}
                    placeholder="Search PO#, Product, SO#"
                    type="text"
                />

                {/* Status Tabs */}
                <div className="flex bg-white rounded-lg shadow-sm p-1 border flex-wrap">
                    {['All', 'Created', 'Released', 'InProgress', 'Completed', 'Cancelled'].map(status => (
                        <button
                            key={status}
                            onClick={() => setFilterStatus(status)}
                            className={`px-3 py-1.5 text-xs rounded-md transition-all ${
                                filterStatus === status
                                    ? 'bg-blue-600 text-white shadow'
                                    : 'text-gray-600 hover:bg-gray-50'
                            }`}
                        >
                            {status}
                        </button>
                    ))}
                </div>
            </div>

            {/* Table */}
            <div className="bg-white rounded-lg shadow border border-gray-200 overflow-hidden">
                <table className="w-full text-left text-sm">
                    <thead className="bg-slate-50 border-b text-xs uppercase text-slate-500">
                        <tr>
                            <th className="p-3">Order</th>
                            <th className="p-3">Product</th>
                            <th className="p-3">BOM</th>
                            <th className="p-3 text-center">Qty</th>
                            <th className="p-3">Dates</th>
                            <th className="p-3">Status</th>
                            <th className="p-3 text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                        {isLoading && orders.length === 0 ? (
                            <tr><td colSpan="6" className="p-8 text-center text-gray-400">Loading...</td></tr>
                        ) : filteredOrders.length === 0 ? (
                            <tr><td colSpan="6" className="p-8 text-center text-gray-400">No orders found.</td></tr>
                        ) : (
                            filteredOrders.map((order) => (
                                <tr key={order.id} className="hover:bg-slate-50 transition-colors">
                                    {/* Order # */}
                                    <td className="p-3">
                                        <span className="font-mono font-semibold text-slate-800">{order.orderNumber}</span>
                                        {order.salesOrderNumber ? (
                                            <div className="text-xs text-blue-500">🔗 {order.salesOrderNumber}</div>
                                        ) : (
                                            <div className="text-xs text-slate-400">Manual</div>
                                        )}
                                    </td>
                                    <td className="p-3">
                                        <span className="font-medium text-slate-700">{order.productName}</span>
                                        <div className="text-xs text-slate-400">{order.productCode}</div>
                                    </td>
                                    {/* BOM */}
                                    <td className="p-3">
                                        {order.bomCode ? (
                                            <div className="inline-flex flex-col gap-0.5">
                                                <span className="text-xs font-mono font-medium text-slate-700">{order.bomCode}</span>
                                                <span className="text-[10px] text-emerald-600 bg-emerald-50 px-1.5 py-0.5 rounded border border-emerald-100 w-max">
                                                    v{order.bomVersion}
                                                </span>
                                            </div>
                                        ) : (
                                            <span className="text-xs text-slate-400">-</span>
                                        )}
                                    </td>
                                    {/* Qty */}
                                    <td className="p-3 text-center">
                                        <span className="font-semibold">{order.quantityProduced || 0}</span>
                                        <span className="text-slate-400"> / {order.quantityPlanned}</span>
                                        {order.quantityScrap > 0 && (
                                            <div className="text-xs text-red-500">⚠ {order.quantityScrap} scrap</div>
                                        )}
                                    </td>
                                    {/* Dates */}
                                    <td className="p-3 text-xs text-slate-500">
                                        <div>{formatDate(order.plannedStartDate)} → {formatDate(order.plannedEndDate)}</div>
                                        {order.actualStartDate && (
                                            <div className="text-blue-500">▶ {formatDate(order.actualStartDate)}</div>
                                        )}
                                    </td>
                                    {/* Status + Reservation */}
                                    <td className="p-3">
                                        <span className={`px-2 py-0.5 rounded-full text-xs font-semibold border ${getStatusColor(order.status)}`}>
                                            {order.status}
                                        </span>
                                        {order.status === 'Released' && (
                                            <div className={`text-xs mt-0.5 font-medium ${getReservationColor(order.reservationStatus)}`}>
                                                📦 {order.reservationStatus || '-'}
                                            </div>
                                        )}
                                    </td>
                                    {/* Actions */}
                                    <td className="p-3 text-right">
                                        <div className="flex justify-end gap-1.5 flex-wrap">
                                            {/* View Detail */}
                                            <button onClick={() => setDetailOrder(order)}
                                                className="p-1.5 rounded hover:bg-blue-50 text-slate-400 hover:text-blue-600"
                                                title="View Details">
                                                <Eye size={15} />
                                            </button>

                                            {/* Created → Release, Cancel */}
                                            {order.status === 'Created' && (
                                                <>
                                                    <button onClick={() => handleRelease(order.id)} disabled={isLoading}
                                                        className="px-2.5 py-1 bg-purple-500 text-white text-xs rounded hover:bg-purple-600 disabled:opacity-50">
                                                        Release
                                                    </button>
                                                    <button onClick={() => handleCancel(order.id)} disabled={isLoading}
                                                        className="px-2.5 py-1 bg-red-500 text-white text-xs rounded hover:bg-red-600 disabled:opacity-50">
                                                        Cancel
                                                    </button>
                                                </>
                                            )}

                                            {/* Released → Start (if Reserved), Retry (if Failed), Cancel */}
                                            {order.status === 'Released' && (
                                                <>
                                                    {order.reservationStatus === 'Reserved' && (
                                                        <button onClick={() => handleStart(order.id)} disabled={isLoading}
                                                            className="px-2.5 py-1 bg-blue-500 text-white text-xs rounded hover:bg-blue-600 disabled:opacity-50 flex items-center gap-1">
                                                            <PlayCircle size={12} /> Start
                                                        </button>
                                                    )}
                                                    {order.reservationStatus === 'Failed' && (
                                                        <button onClick={() => handleRetryReservation(order.id)} disabled={isLoading}
                                                            className="px-2.5 py-1 bg-orange-500 text-white text-xs rounded hover:bg-orange-600 disabled:opacity-50 flex items-center gap-1">
                                                            <RotateCcw size={12} /> Retry
                                                        </button>
                                                    )}
                                                    <button onClick={() => handleCancel(order.id)} disabled={isLoading}
                                                        className="px-2.5 py-1 bg-red-500 text-white text-xs rounded hover:bg-red-600 disabled:opacity-50">
                                                        Cancel
                                                    </button>
                                                </>
                                            )}

                                            {/* InProgress → Force Complete (no qty popup — auto-sums from WOs) */}
                                            {order.status === 'InProgress' && (
                                                <button onClick={() => handleForceComplete(order)} disabled={isLoading}
                                                    className="px-2.5 py-1 bg-green-500 text-white text-xs rounded hover:bg-green-600 disabled:opacity-50 flex items-center gap-1"
                                                    title="Auto-complete PO by summing WO quantities">
                                                    <CheckCircle size={12} /> Force Complete
                                                </button>
                                            )}
                                        </div>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {/* Detail Modal */}
            {detailOrder && (
                <ProductionOrderDetailModal
                    order={detailOrder}
                    onClose={() => setDetailOrder(null)}
                />
            )}
        </div>
    );
};

export default OrderManagement;