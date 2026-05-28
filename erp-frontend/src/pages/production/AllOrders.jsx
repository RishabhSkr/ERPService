import { useCallback, useEffect, useState } from 'react';
import { getAllOrders as getAllBatches } from '../../api/productionService';
import FilterBar from '../../components/common/FilterBar';
import useApi from '../../hooks/useApi';
import ProductionOrderDetailModal from '../../components/production/ProductionOrderDetailModal';
import { Factory, Eye } from 'lucide-react';

const Production = () => {
    const [orders, setOrders] = useState([]);
    const [filterId, setFilterId] = useState('');
    const [statusFilter, setStatusFilter] = useState('All');
    const [selectedOrder, setSelectedOrder] = useState(null);
    const[currentPage,setCurrentPage] = useState(1);
    const pageSize = 10;

    // hook API Calls
    const { loading: isLoading, requestHandlerFunction } = useApi();

    // Fetch Function 
    const fetchOrders = useCallback(async (soId = null) => {
        const response = await requestHandlerFunction(
            () => getAllBatches(soId)
        );
        // response.data = axiosResponse
        // response.data.data = backend wrapper { data: [...], success, message }
        // response.data.data.data = actual orders array
        const backendWrapper = response.data?.data;
        const orderList = backendWrapper?.data || backendWrapper || [];
        setOrders(Array.isArray(orderList) ? orderList : []);
    }, [requestHandlerFunction]);

    //  Initial Load
    useEffect(() => {
        const loadOrders = async () => await fetchOrders();
        loadOrders();
    }, [fetchOrders]);
    
    // Combined filter: text search + status dropdown
    const filteredOrders = orders.filter(order => {
        const search = filterId.toLowerCase();
        const matchesSearch = !filterId
            || (order.orderNumber || '').toLowerCase().includes(search)
            || (order.salesOrderNumber || '').toLowerCase().includes(search)
            || (order.productName || '').toLowerCase().includes(search)
            || (order.productCode || '').toLowerCase().includes(search);
        const matchesStatus = statusFilter === 'All'
            || (order.status || '').toLowerCase() === statusFilter.toLowerCase();
        return matchesSearch && matchesStatus;
    });

    const handleClear = () => {
        setFilterId('');
        setStatusFilter('All');
        fetchOrders(null);
    };

    const getStatusColor = (status) => {
        const safeStatus = String(status || '').toLowerCase();
        switch (safeStatus) {
            case 'completed': return 'bg-green-100 text-green-700 border-green-200';
            case 'cancelled': return 'bg-red-100 text-red-700 border-red-200';
            case 'created': return 'bg-yellow-100 text-yellow-700 border-yellow-200';
            case 'released': return 'bg-purple-100 text-purple-700 border-purple-200';
            case 'inprogress':
            case 'in progress': return 'bg-blue-100 text-blue-700 border-blue-200';
            default: return 'bg-gray-100 text-gray-700 border-gray-200';
        }
    };

    const formatDate = (dateString) => {
        if (!dateString) return '-';
        return new Date(dateString).toLocaleString('en-US', {
            month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit'
        });
    };

    return (
        <div className="p-6 max-w-7xl mx-auto">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-6 gap-4">
                <h1 className="text-2xl font-bold flex items-center gap-2 text-slate-800">
                    <Factory className="text-blue-600" />
                    Production Orders List
                </h1>
                
                <div className="flex flex-col sm:flex-row gap-3 items-center w-full md:w-auto">
                    <FilterBar
                        value={filterId}
                        onChange={setFilterId}
                        onSearch={() => {}}
                        onClear={handleClear}
                        placeholder="Search by PO#, Product..."
                        type="text"
                    />

                    {/* Status Filter Dropdown */}
                    <select
                        value={statusFilter}
                        onChange={e => { setStatusFilter(e.target.value); setCurrentPage(1); }}
                        className="border border-gray-200 rounded-lg px-3 py-2 text-sm text-slate-700 bg-white focus:outline-none focus:ring-2 focus:ring-blue-300 min-w-[140px]"
                    >
                        <option value="All">All Status</option>
                        <option value="Created">Created</option>
                        <option value="Released">Released</option>
                        <option value="InProgress">In Progress</option>
                        <option value="Completed">Completed</option>
                        <option value="Cancelled">Cancelled</option>
                    </select>

                    <div className="text-sm text-gray-500 whitespace-nowrap">
                        <span className="font-bold text-gray-800">{filteredOrders.length}</span>
                        <span className="text-gray-400"> / {orders.length}</span>
                    </div>
                </div>
            </div>

            <div className="bg-white rounded-lg shadow border border-gray-200 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-sm text-left">
                        <thead className="bg-slate-50 text-slate-600 uppercase text-xs font-semibold border-b border-gray-200">
                            <tr>
                                <th className="px-5 py-3">Order</th>
                                <th className="px-5 py-3">Product</th>
                                <th className="px-5 py-3">BOM</th>
                                <th className="px-5 py-3 text-center">Quantity</th>
                                <th className="px-5 py-3 text-center">Progress</th>
                                <th className="px-5 py-3">Status</th>
                                <th className="px-5 py-3">Created</th>
                                <th className="px-5 py-3 text-center">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                            {isLoading ? (
                                <tr>
                                    <td colSpan="7" className="px-6 py-12 text-center text-gray-400">
                                        <div className="flex justify-center items-center gap-2">
                                            <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-500"></div>
                                            Loading production orders...
                                        </div>
                                    </td>
                                </tr>
                            ) : orders.length === 0 ? (
                                <tr>
                                    <td colSpan="7" className="px-6 py-12 text-center text-gray-400">
                                        <div className="flex flex-col items-center gap-2">
                                            <Factory size={40} className="opacity-20" />
                                            <p>No production orders found.</p>
                                        </div>
                                    </td>
                                </tr>
                            ) : (
                                filteredOrders.slice((currentPage -1 ) * pageSize,currentPage * pageSize).map((order) => (
                                    <tr key={order.id} className="hover:bg-slate-50 transition-colors">
                                        {/* PO# + SO link */}
                                        <td className="px-5 py-3">
                                            <span className="font-mono font-semibold text-slate-800">{order.orderNumber}</span>
                                            {order.salesOrderNumber ? (
                                                <div className="text-xs text-blue-500 mt-0.5">🔗 {order.salesOrderNumber}</div>
                                            ) : (
                                                <div className="text-xs text-slate-400 mt-0.5">Manual</div>
                                            )}
                                        </td>
                                        {/* Product */}
                                        <td className="px-5 py-3">
                                            <span className="font-medium text-slate-700">{order.productName}</span>
                                            <div className="text-xs text-slate-400">{order.productCode}</div>
                                        </td>
                                        {/* BOM */}
                                        <td className="px-5 py-3">
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
                                        {/* Qty: Produced / Planned */}
                                        <td className="px-5 py-3 text-center">
                                            <span className="font-semibold text-slate-700">{order.quantityProduced || 0}</span>
                                            <span className="text-slate-400"> / {order.quantityPlanned}</span>
                                            {order.quantityScrap > 0 && (
                                                <div className="text-xs text-red-500">⚠ {order.quantityScrap} scrap</div>
                                            )}
                                        </td>
                                        {/* Progress bar */}
                                        <td className="px-5 py-3 text-center">
                                            <div className="flex items-center justify-center gap-2">
                                                <div className="w-16 h-1.5 bg-slate-200 rounded-full overflow-hidden">
                                                    <div 
                                                        className="h-full rounded-full"
                                                        style={{ 
                                                            width: `${Math.min(order.percentComplete || 0, 100)}%`,
                                                            backgroundColor: order.percentComplete >= 100 ? '#10b981' : '#3b82f6'
                                                        }}
                                                    />
                                                </div>
                                                <span className="text-xs font-medium text-slate-500">{order.percentComplete || 0}%</span>
                                            </div>
                                        </td>
                                        {/* Status + Reservation */}
                                        <td className="px-3 py-3">
                                            <span className={`px-2.5 py-1 rounded-full text-xs font-semibold border ${getStatusColor(order.status)}`}>
                                                {order.status}
                                            </span>
                                            <div className={`text-xs mt-1 font-medium ${
                                                order.reservationStatus === 'Reserved' ? 'text-green-600' :
                                                order.reservationStatus === 'Failed' ? 'text-red-500' :
                                                'text-yellow-600'
                                            }`}>
                                                {/* 📦 {order.reservationStatus || '-'} */}
                                            </div>
                                        </td>
                                        {/* Created date */}
                                        <td className="px-5 py-3 text-xs text-slate-500">
                                            {formatDate(order.createdAt)}
                                        </td>
                                        {/* View details */}
                                        <td className="px-5 py-3 text-center">
                                            <button
                                                onClick={() => setSelectedOrder(order)}
                                                className="p-1.5 rounded hover:bg-blue-50 text-slate-400 hover:text-blue-600 transition-colors"
                                                title="View Details"
                                            >
                                                <Eye size={16} />
                                            </button>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                    {/* pagination */}
                    {orders.length > pageSize && (
                        <div className="flex items-center justify-center gap-3 mt-4">
                            <button onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                                disabled={currentPage <= 1}
                                className="px-3 py-1.5 rounded bg-slate-100 text-sm disabled:opacity-40">
                                ← Prev
                            </button>
                            <span className="text-sm text-slate-500">
                                Page {currentPage} of {Math.ceil(orders.length / pageSize)}
                            </span>
                            <button onClick={() => setCurrentPage(p => p + 1)}
                                disabled={currentPage >= Math.ceil(orders.length / pageSize)}
                                className="px-3 py-1.5 rounded bg-slate-100 text-sm disabled:opacity-40">
                                Next →
                            </button>
                        </div>
                    )}
                </div>
            </div>

            {/* Detail Modal */}
            {selectedOrder && (
                <ProductionOrderDetailModal
                    order={selectedOrder}
                    onClose={() => setSelectedOrder(null)}
                />
            )}
        </div>
    );
};

export default Production;