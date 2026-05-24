import React, { useState, useEffect } from 'react';
import { BarChart3, Truck, PackageCheck, RefreshCw, AlertCircle } from 'lucide-react';
import toast from 'react-hot-toast';
import { getFulfillmentDashboard, markDelivered } from '../../api/salesOrderService';
import DispatchModal from '../../components/sales/DispatchModal';

const STATUS_COLORS = {
    Pending: 'bg-gray-100 text-gray-700',
    Confirmed: 'bg-blue-100 text-blue-700',
    InProduction: 'bg-yellow-100 text-yellow-800',
    Shipped: 'bg-green-100 text-green-700',
    Delivered: 'bg-emerald-100 text-emerald-700',
};

const FulfillmentDashboard = () => {
    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);
    const [dispatchOrder, setDispatchOrder] = useState(null);
    // console.log(orders);
    const fetchDashboard = async () => {
        setLoading(true);
        try {
            const res = await getFulfillmentDashboard();
            console.log("Fulfillment Dashboard Response:", res.data);
            setOrders(res.data?.data || []);
        } catch (err) {
            toast.error('Failed to load dashboard');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchDashboard(); }, []);

    const handleDelivered = async (orderId) => {
        try {
            await markDelivered(orderId);
            toast.success('Marked as delivered!');
            fetchDashboard();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Failed');
        }
    };

    // Summary stats
    const totalOrders = orders.length;
    const inProduction = orders.filter(o => o.status === 'InProduction').length;
    const shipped = orders.filter(o => o.status === 'Shipped').length;

    return (
        <div className="p-6">
            {/* Header */}
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <BarChart3 className="text-blue-500" size={28} />
                    <h1 className="text-2xl font-bold text-slate-800">Fulfillment Dashboard</h1>
                </div>
                <button
                    onClick={fetchDashboard}
                    className="flex items-center gap-2 px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm"
                >
                    <RefreshCw size={16} /> Refresh
                </button>
            </div>

            {/* Summary Cards */}
            <div className="grid grid-cols-3 gap-4 mb-6">
                <div className="bg-white rounded-xl border border-slate-200 p-4">
                    <p className="text-xs text-slate-400 uppercase font-medium">Active Orders</p>
                    <p className="text-3xl font-bold text-slate-800 mt-1">{totalOrders}</p>
                </div>
                <div className="bg-white rounded-xl border border-yellow-200 p-4">
                    <p className="text-xs text-yellow-600 uppercase font-medium">In Production</p>
                    <p className="text-3xl font-bold text-yellow-700 mt-1">{inProduction}</p>
                </div>
                <div className="bg-white rounded-xl border border-green-200 p-4">
                    <p className="text-xs text-green-600 uppercase font-medium">Shipped</p>
                    <p className="text-3xl font-bold text-green-700 mt-1">{shipped}</p>
                </div>
            </div>

            {/* Orders */}
            {loading ? (
                <div className="text-center py-16 text-slate-400">Loading dashboard...</div>
            ) : orders.length === 0 ? (
                <div className="text-center py-16">
                    <AlertCircle size={48} className="text-slate-300 mx-auto mb-3" />
                    <p className="text-slate-400">No active orders to fulfill</p>
                </div>
            ) : (
                <div className="space-y-4">
                    {orders.map(order => (
                        <div key={order.orderId} className="bg-white rounded-xl border border-slate-200 overflow-hidden">
                            {/* Order Header */}
                            <div className="flex items-center justify-between px-5 py-4 border-b border-slate-100">
                                <div className="flex items-center gap-4">
                                    <div>
                                        <p className="font-mono font-bold text-slate-800">{order.orderNumber}</p>
                                        <p className="text-sm text-slate-500">{order.customerName}</p>
                                    </div>
                                    <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${STATUS_COLORS[order.status]}`}>
                                        {order.status}
                                    </span>
                                </div>
                                <div className="flex gap-2">
                                    {['InProduction', 'Confirmed'].includes(order.status) && (
                                        <button
                                            onClick={() => setDispatchOrder(order)}
                                            className="flex items-center gap-1.5 px-3 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-medium"
                                        >
                                            <Truck size={14} /> Dispatch
                                        </button>
                                    )}
                                    {order.status === 'Shipped' && (
                                        <button
                                            onClick={() => handleDelivered(order.orderId)}
                                            className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-medium"
                                        >
                                            <PackageCheck size={14} /> Delivered
                                        </button>
                                    )}
                                </div>
                            </div>

                            {/* Items Table */}
                            <table className="w-full">
                                <thead className="bg-slate-50">
                                    <tr>
                                        <th className="text-left px-4 py-2 text-xs font-semibold text-slate-500">Product</th>
                                        <th className="text-right px-4 py-2 text-xs font-semibold text-slate-500">Ordered</th>
                                        <th className="text-right px-4 py-2 text-xs font-semibold text-slate-500">Produced</th>
                                        <th className="text-right px-4 py-2 text-xs font-semibold text-slate-500">Available</th>
                                        <th className="text-right px-4 py-2 text-xs font-semibold text-slate-500">Dispatched</th>
                                        <th className="text-right px-4 py-2 text-xs font-semibold text-slate-500">Remaining</th>
                                        <th className="text-right px-4 py-2 text-xs font-semibold text-slate-500">Progress</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-slate-50">
                                    {(order.items || []).map((item, i) => (
                                        <tr key={i} className="hover:bg-slate-50">
                                            <td className="px-4 py-2.5">
                                                <p className="text-sm font-medium text-slate-700">{item.productName}</p>
                                                <p className="text-xs text-slate-400">{item.productCode}</p>
                                            </td>
                                            <td className="px-4 py-2.5 text-sm text-right">{item.ordered}</td>
                                            <td className="px-4 py-2.5 text-sm text-right font-medium text-blue-600">{item.produced}</td>
                                            <td className="px-4 py-2.5 text-sm text-right font-medium text-purple-600">{item.availableInInventory}</td>
                                            <td className="px-4 py-2.5 text-sm text-right font-medium text-green-600">{item.dispatched}</td>
                                            <td className="px-4 py-2.5 text-sm text-right font-medium text-orange-600">{item.remaining}</td>
                                            <td className="px-4 py-2.5">
                                                <div className="flex items-center justify-end gap-2">
                                                    <div className="w-20 h-2 bg-slate-200 rounded-full overflow-hidden">
                                                        <div 
                                                            className={`h-full rounded-full transition-all ${
                                                                item.progressPercent >= 100 ? 'bg-green-500' : 'bg-blue-500'
                                                            }`}
                                                            style={{ width: `${Math.min(item.progressPercent, 100)}%` }}
                                                        />
                                                    </div>
                                                    <span className="text-xs font-medium text-slate-500 w-10 text-right">
                                                        {item.progressPercent}%
                                                    </span>
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    ))}
                </div>
            )}

            {/* Dispatch Modal */}
            {dispatchOrder && (
                <DispatchModal
                    order={dispatchOrder}
                    onClose={() => setDispatchOrder(null)}
                    onDispatched={() => { setDispatchOrder(null); fetchDashboard(); }}
                />
            )}
        </div>
    );
};

export default FulfillmentDashboard;
