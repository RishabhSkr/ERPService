import { useCallback, useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { Play, ShoppingCart, Package, Clock } from 'lucide-react';
import PlanningModal from '../../components/production/PlanningModal';
import useApi from '../../hooks/useApi';
import { 
    getAllPendingOrders as getPendingOrders,
    getWOPlanningInfo as getPlanningInfo,
    approveRequest
} from '../../api/productionService';

/**
 * Production Planning Dashboard
 * Shows confirmed Sales Orders that need Production Orders created.
 * API: GET /api/production/pending-requests
 */
const Dashboard = () => {
    const location = useLocation();
    const [orders, setOrders] = useState([]);
    
    // Modal & Selection States
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [planningData, setPlanningData] = useState(null);
    const [selectedOrderId, setSelectedOrderId] = useState(null);

    const { loading: isLoading, requestHandlerFunction } = useApi();

    const loadDashboardData = useCallback(async () => {
        const response = await requestHandlerFunction(
            () => getPendingOrders()
        );
        
        if (response.success) {
            // useApi wraps: { success, data: axiosResponse }
            // axiosResponse.data = { data: [...], success, message }
            const backendData = response.data?.data?.data || response.data?.data || [];
            setOrders(Array.isArray(backendData) ? backendData : []);
        } else {
            setOrders([]);
        }
    }, [requestHandlerFunction]);

    useEffect(() => {
        loadDashboardData();
    }, [loadDashboardData, location.key]);

    // Plan Button Click — Open modal with request data
    const handlePlanClick = async (requestId) => {
        setSelectedOrderId(requestId);
        setIsModalOpen(true);
        // PlanningModal shows request info, user sets start date + priority
        setPlanningData(null);
        // Set data to pass to modal (the request itself)
        const order = orders.find(o => o.id === requestId);
        setPlanningData(order); // pass request data to modal
    };

    // Approve Pending Request → auto-creates POs for all items
    const handleCreateBatch = async (startDate, endDate, priority) => {
        const payload = {
            plannedStartDate: new Date(startDate).toISOString(),
            priority: priority || 3,
            notes: `Approved from dashboard. Planned start: ${startDate}`,
        };

        const response = await requestHandlerFunction(
            () => approveRequest(selectedOrderId, payload),
            'Request approved! Production orders created.'
        );

        if (response.success) {
            setIsModalOpen(false);
            loadDashboardData();
        }
    };

    const getStatusColor = (status) => {
        switch (status) {
            case 'Pending': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
            case 'Approved': return 'bg-green-100 text-green-800 border-green-200';
            case 'Cancelled': return 'bg-red-100 text-red-700 border-red-200';
            default: return 'bg-gray-100 text-gray-800 border-gray-200';
        }
    };

    const formatDate = (d) => {
        if (!d) return '-';
        return new Date(d).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
    };

    // Summary counts
    const pendingCount = orders.filter(o => o.status === 'Pending').length;
    const totalItems = orders.reduce((sum, o) => sum + (o.items?.length || 0), 0);

    if (isLoading && orders.length === 0) 
        return <div className="p-8 text-center text-blue-600">Loading Dashboard...</div>;

    return (
        <div className="p-6 space-y-6">
            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold text-slate-800 flex items-center gap-2">
                    <ShoppingCart className="text-blue-600" size={28} />
                    Production Planning
                </h1>
                <p className="text-sm text-slate-500 mt-1">Confirmed Sales Orders waiting for Production Orders</p>
            </div>

            {/* Summary Cards */}
            <div className="grid grid-cols-3 gap-4">
                <div className="bg-white rounded-xl border border-slate-200 p-4">
                    <p className="text-xs text-slate-400 uppercase font-medium">Total Requests</p>
                    <p className="text-3xl font-bold text-slate-800 mt-1">{orders.length}</p>
                </div>
                <div className="bg-yellow-50 rounded-xl border border-yellow-200 p-4">
                    <p className="text-xs text-yellow-600 uppercase font-medium">Pending</p>
                    <p className="text-3xl font-bold text-yellow-700 mt-1">{pendingCount}</p>
                </div>
                <div className="bg-blue-50 rounded-xl border border-blue-200 p-4">
                    <p className="text-xs text-blue-600 uppercase font-medium">Total Line Items</p>
                    <p className="text-3xl font-bold text-blue-700 mt-1">{totalItems}</p>
                </div>
            </div>

            {/* Empty State */}
            {orders.length === 0 && !isLoading && (
                <div className="bg-white p-8 rounded-lg shadow text-center text-gray-500">
                    No pending orders found. Good job! 🎉
                </div>
            )}

            {/* Orders Table */}
            {orders.length > 0 && (
                <div className="bg-white rounded-lg shadow overflow-hidden border border-gray-200">
                    <table className="w-full text-left border-collapse text-sm">
                        <thead className="bg-slate-50 text-slate-600 uppercase text-xs font-semibold">
                            <tr>
                                <th className="p-4 border-b">SO #</th>
                                <th className="p-4 border-b">Customer</th>
                                <th className="p-4 border-b">Items</th>
                                <th className="p-4 border-b">Date</th>
                                <th className="p-4 border-b">Status</th>
                                <th className="p-4 border-b text-right">Action</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                            {orders.map((order, index) => (
                                <tr key={order.id || index} className="hover:bg-slate-50 transition-colors">
                                    <td className="p-4">
                                        <span className="font-mono font-semibold text-slate-800">
                                            {order.salesOrderNumber || `SO-${String(order.salesOrderId).slice(0,8)}`}
                                        </span>
                                    </td>
                                    <td className="p-4">
                                        <span className="font-medium text-slate-700">{order.customerName}</span>
                                    </td>
                                    <td className="p-4">
                                        <div className="space-y-1">
                                            {(order.items || []).map((item, i) => (
                                                <div key={i} className="flex items-center gap-2 text-xs">
                                                    <Package size={12} className="text-slate-400" />
                                                    <span className="text-slate-600">{item.productName}</span>
                                                    <span className="font-semibold text-slate-800">× {item.quantity}</span>
                                                </div>
                                            ))}
                                        </div>
                                    </td>
                                    <td className="p-4 text-slate-500 text-xs">
                                        <div className="flex items-center gap-1">
                                            <Clock size={12} />
                                            {formatDate(order.orderDate)}
                                        </div>
                                    </td>
                                    <td className="p-4">
                                        <span className={`px-2.5 py-1 rounded-full text-xs font-semibold border ${getStatusColor(order.status)}`}>
                                            {order.status}
                                        </span>
                                    </td>
                                    <td className="p-4 text-right">
                                        <button
                                            className="bg-slate-900 hover:bg-slate-700 text-white px-4 py-2 rounded-md text-sm flex items-center gap-2 ml-auto transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                                            disabled={order.status !== 'Pending' || isLoading}
                                            onClick={() => handlePlanClick(order.id)}
                                        >
                                            <Play size={16} /> Plan PO
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
            
            <PlanningModal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                data={planningData}
                isLoading={isLoading}
                onConfirm={handleCreateBatch}
            />
        </div>
    );
};

export default Dashboard;