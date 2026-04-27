import React, { useCallback, useEffect, useState } from 'react';
import { Factory, Plus, Eye, Play, RotateCcw, XCircle, Zap, Pause, CheckCircle, Loader } from 'lucide-react';
import useApi from '../../hooks/useApi';
import {
    getAllOrders,
    getWorkOrdersByPO,
    releaseWO,
    cancelWO,
    retryWOReservation,
    pauseExecution,
} from '../../api/productionService';
import CreateWOModal from '../../components/production/CreateWOModal';
import ActivateWOModal from '../../components/production/ActivateWOModal';
import CompleteExecutionModal from '../../components/production/CompleteExecutionModal';
import WODetailModal from '../../components/production/WODetailModal';
import SearchSelect from '../../components/common/SearchSelect';

const STATUS_COLORS = {
    Pending: 'bg-yellow-100 text-yellow-700',
    Released: 'bg-purple-100 text-purple-700',
    InProgress: 'bg-blue-100 text-blue-700',
    Completed: 'bg-green-100 text-green-700',
    Cancelled: 'bg-red-100 text-red-700',
};

const WOManagement = () => {
    const [poList, setPOList] = useState([]);
    const [selectedPOId, setSelectedPOId] = useState('');
    const [workOrders, setWorkOrders] = useState([]);
    const { loading, requestHandlerFunction } = useApi();

    // Modal states
    const [createModal, setCreateModal] = useState(false);
    const [activateWO, setActivateWO] = useState(null);   // WO object
    const [completeWO, setCompleteWO] = useState(null);    // WO object (has active execution)
    const [detailWO, setDetailWO] = useState(null);        // WO object

    // Load PO list for dropdown
    const fetchPOs = useCallback(async () => {
        const res = await requestHandlerFunction(() => getAllOrders());
        if (res.success) {
            const data = res.data?.data?.data || res.data?.data || [];
            // Only show Released/InProgress POs (can have WOs)
            const activePOs = (Array.isArray(data) ? data : [])
                .filter(po => ['Released', 'InProgress'].includes(po.status));
            setPOList(activePOs);
        }
    }, [requestHandlerFunction]);

    // Load WOs for selected PO
    const fetchWOs = useCallback(async () => {
        if (!selectedPOId) { setWorkOrders([]); return; }
        const res = await requestHandlerFunction(() => getWorkOrdersByPO(selectedPOId));
        if (res.success) {
            const data = res.data?.data?.data || res.data?.data || [];
            setWorkOrders(Array.isArray(data) ? data : []);
        }
    }, [selectedPOId, requestHandlerFunction]);

    useEffect(() => { fetchPOs(); }, [fetchPOs]);
    useEffect(() => { fetchWOs(); }, [fetchWOs]);

    // Background polling for pending reservations (RabbitMQ async response)
    useEffect(() => {
        let interval;
        const hasPending = workOrders.some(wo => wo.status === 'Released' && wo.reservationStatus === 'Pending');
        if (hasPending && selectedPOId) {
            interval = setInterval(async () => {
                try {
                    const res = await getWorkOrdersByPO(selectedPOId);
                    const data = res.data?.data?.data || res.data?.data || [];
                    setWorkOrders(Array.isArray(data) ? data : []);
                } catch (e) {
                    console.error('Polling error', e);
                }
            }, 1500); // Poll every 1.5s
        }
        return () => clearInterval(interval);
    }, [workOrders, selectedPOId]);

    const selectedPO = poList.find(p => p.id === selectedPOId);

    // ─── WO Actions ───
    const handleRelease = async (woId) => {
        const res = await requestHandlerFunction(() => releaseWO(woId), 'WO Released! Reserving materials...');
        if (res.success) fetchWOs(); // The polling useEffect will take over
    };

    const handleCancel = async (woId) => {
        const reason = prompt('Cancel reason:');
        if (!reason) return;
        const res = await requestHandlerFunction(() => cancelWO(woId, reason), 'WO Cancelled.');
        if (res.success) fetchWOs();
    };

    const handleRetry = async (woId) => {
        const res = await requestHandlerFunction(() => retryWOReservation(woId), 'Retry initiated...');
        if (res.success) fetchWOs(); // The polling useEffect will take over
    };

    const handlePause = async (woId, execId) => {
        const res = await requestHandlerFunction(() => pauseExecution(woId, execId), 'Execution paused.');
        if (res.success) fetchWOs();
    };

    // Find active execution for a WO
    const getActiveExecution = (wo) => {
        return wo.executions?.find(e => e.status === 'Active');
    };

    const formatDate = (d) => {
        if (!d) return '-';
        return new Date(d).toLocaleDateString('en-IN', { day: '2-digit', month: 'short' });
    };

    return (
        <div className="p-6 max-w-7xl mx-auto space-y-6">
            {/* Header */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-slate-800 flex items-center gap-2">
                        <Zap className="text-indigo-600" /> Work Order Management
                    </h1>
                    <p className="text-sm text-slate-500 mt-1">Create, manage and execute work orders</p>
                </div>

                <div className="flex items-center gap-3">
                    {/* PO Selector */}
                    <SearchSelect
                        value={selectedPOId}
                        displayValue={(() => { const po = poList.find(p => p.id === selectedPOId); return po ? `${po.orderNumber} — ${po.productName} (${po.quantityPlanned} qty)` : ''; })()}
                        placeholder="Search Production Order..."
                        items={poList}
                        title="Select Production Order"
                        displayFields={[
                            { key: 'orderNumber', label: 'PO#', width: '25%', bold: true },
                            { key: 'productName', label: 'Product', width: '45%' },
                            { key: 'quantityPlanned', label: 'Qty', width: '15%' },
                            { key: 'status', label: 'Status', width: '15%' },
                        ]}
                        searchKeys={['orderNumber', 'productName', 'productCode']}
                        valueKey="id"
                        onSelect={(po) => setSelectedPOId(po.id)}
                        onClear={() => setSelectedPOId('')}
                        className="min-w-[260px]"
                    />

                    {/* Create WO button */}
                    {selectedPOId && (
                        <button
                            onClick={() => setCreateModal(true)}
                            className="px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm hover:bg-indigo-700 flex items-center gap-2"
                        >
                            <Plus size={16} /> Create WO
                        </button>
                    )}
                </div>
            </div>

            {/* Selected PO Info */}
            {selectedPO && (
                <div className="bg-indigo-50 border border-indigo-200 rounded-xl p-4 flex flex-wrap gap-6 text-sm">
                    <div><span className="text-indigo-400 text-xs uppercase">PO#</span><br/><span className="font-bold text-indigo-700">{selectedPO.orderNumber}</span></div>
                    <div><span className="text-indigo-400 text-xs uppercase">Product</span><br/><span className="font-medium">{selectedPO.productName}</span></div>
                    <div><span className="text-indigo-400 text-xs uppercase">Planned</span><br/><span className="font-bold">{selectedPO.quantityPlanned}</span></div>
                    <div><span className="text-indigo-400 text-xs uppercase">Status</span><br/><span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${STATUS_COLORS[selectedPO.status] || ''}`}>{selectedPO.status}</span></div>
                    <div><span className="text-indigo-400 text-xs uppercase">Work Orders</span><br/><span className="font-bold text-indigo-700">{workOrders.length}</span></div>
                </div>
            )}

            {/* No PO selected */}
            {!selectedPOId && (
                <div className="bg-white rounded-xl border border-slate-200 p-12 text-center text-slate-400">
                    <Factory size={48} className="mx-auto mb-3 opacity-30" />
                    <p>Select a Production Order to view and manage its Work Orders</p>
                </div>
            )}

            {/* WO Table */}
            {selectedPOId && (
                <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
                    <table className="w-full text-sm text-left">
                        <thead className="bg-slate-50 text-slate-500 uppercase text-xs border-b">
                            <tr>
                                <th className="p-3">WO #</th>
                                <th className="p-3">Step / Process</th>
                                <th className="p-3">Work Center</th>
                                <th className="p-3 text-center">Quantity</th>
                                <th className="p-3">Status</th>
                                <th className="p-3">Schedule</th>
                                <th className="p-3 text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-100">
                            {loading && workOrders.length === 0 ? (
                                <tr><td colSpan="7" className="p-8 text-center text-slate-400">Loading...</td></tr>
                            ) : workOrders.length === 0 ? (
                                <tr><td colSpan="7" className="p-8 text-center text-slate-400">
                                    No work orders yet. Click "Create WO" to start.
                                </td></tr>
                            ) : (
                                workOrders.map((wo) => {
                                    const activeExec = getActiveExecution(wo);
                                    return (
                                        <tr key={wo.workOrderId} className="hover:bg-slate-50 transition-colors">
                                            {/* WO # */}
                                            <td className="p-3">
                                                <span className="font-mono font-semibold text-slate-800">{wo.workOrderNumber}</span>
                                            </td>
                                            {/* Step / Process */}
                                            <td className="p-3">
                                                <span className="bg-indigo-100 text-indigo-700 px-1.5 py-0.5 rounded text-xs font-bold mr-1">
                                                    #{wo.stepNumber}
                                                </span>
                                                <span className="font-medium text-slate-700">{wo.operationName}</span>
                                                {wo.processCode && <div className="text-xs text-slate-400">{wo.processCode}</div>}
                                            </td>
                                            {/* Work Center */}
                                            <td className="p-3 text-xs text-slate-600">
                                                {wo.workCenterName || '-'}
                                                {wo.workCenterCode && <div className="text-slate-400">{wo.workCenterCode}</div>}
                                            </td>
                                            {/* Qty */}
                                            <td className="p-3 text-center">
                                                <span className="font-semibold">{wo.quantityCompleted || 0}</span>
                                                <span className="text-slate-400"> / {wo.quantityPlanned}</span>
                                                {wo.quantityScrap > 0 && (
                                                    <div className="text-xs text-red-500">⚠ {wo.quantityScrap} scrap</div>
                                                )}
                                            </td>
                                            {/* Status */}
                                            <td className="p-3">
                                                <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${STATUS_COLORS[wo.status] || 'bg-gray-100'}`}>
                                                    {wo.status}
                                                </span>
                                                {wo.reservationStatus && wo.status === 'Released' && (
                                                    <div className={`text-xs mt-0.5 font-medium flex items-center gap-1 ${
                                                        wo.reservationStatus === 'Reserved' ? 'text-green-600' :
                                                        wo.reservationStatus === 'Failed' ? 'text-red-500' : 'text-orange-600'
                                                    }`}>
                                                        {wo.reservationStatus === 'Pending' ? (
                                                            <>⚠ Pending — Retry</>
                                                        ) : wo.reservationStatus === 'Failed' ? (
                                                            <>❌ Failed — Retry</>
                                                        ) : (
                                                            `📦 ${wo.reservationStatus}`
                                                        )}
                                                    </div>
                                                )}
                                                {activeExec && (
                                                    <div className="text-xs text-blue-500 mt-0.5">⚡ {activeExec.equipmentCode}</div>
                                                )}
                                            </td>
                                            {/* Schedule */}
                                            <td className="p-3 text-xs text-slate-500">
                                                {formatDate(wo.scheduledStart)} → {formatDate(wo.scheduledEnd)}
                                            </td>
                                            {/* Actions */}
                                            <td className="p-3 text-right">
                                                <div className="flex justify-end gap-1 flex-wrap">
                                                    {/* View */}
                                                    <button onClick={() => setDetailWO(wo)}
                                                        className="p-1.5 rounded hover:bg-blue-50 text-slate-400 hover:text-blue-600" title="Details">
                                                        <Eye size={14} />
                                                    </button>

                                                    {/* Pending → Release, Cancel */}
                                                    {wo.status === 'Pending' && (
                                                        <>
                                                            <button onClick={() => handleRelease(wo.workOrderId)} disabled={loading}
                                                                className="px-2 py-1 bg-purple-500 text-white text-xs rounded hover:bg-purple-600 disabled:opacity-50">
                                                                Release
                                                            </button>
                                                            <button onClick={() => handleCancel(wo.workOrderId)} disabled={loading}
                                                                className="px-2 py-1 bg-red-500 text-white text-xs rounded hover:bg-red-600 disabled:opacity-50">
                                                                Cancel
                                                            </button>
                                                        </>
                                                    )}

                                                    {/* Released + Reserved → Activate */}
                                                    {wo.status === 'Released' && wo.reservationStatus === 'Reserved' && (
                                                        <>
                                                            <button onClick={() => setActivateWO(wo)} disabled={loading}
                                                                className="px-2 py-1 bg-blue-500 text-white text-xs rounded hover:bg-blue-600 disabled:opacity-50 flex items-center gap-1">
                                                                <Play size={12} /> Activate
                                                            </button>
                                                            <button onClick={() => handleCancel(wo.workOrderId)} disabled={loading}
                                                                className="px-2 py-1 bg-red-500 text-white text-xs rounded hover:bg-red-600 disabled:opacity-50">
                                                                Cancel
                                                            </button>
                                                        </>
                                                    )}

                                                    {/* Released + Failed or Pending → Retry */}
                                                    {wo.status === 'Released' && (wo.reservationStatus === 'Failed' || wo.reservationStatus === 'Pending') && (
                                                        <>
                                                            <button onClick={() => handleRetry(wo.workOrderId)} disabled={loading}
                                                                className="px-2 py-1 bg-orange-500 text-white text-xs rounded hover:bg-orange-600 disabled:opacity-50 flex items-center gap-1">
                                                                <RotateCcw size={12} /> Retry
                                                            </button>
                                                            <button onClick={() => handleCancel(wo.workOrderId)} disabled={loading}
                                                                className="px-2 py-1 bg-red-500 text-white text-xs rounded hover:bg-red-600 disabled:opacity-50">
                                                                Cancel
                                                            </button>
                                                        </>
                                                    )}

                                                    {/* InProgress → Complete Exec, Pause, Activate (more equipment) */}
                                                    {wo.status === 'InProgress' && (
                                                        <>
                                                            {activeExec && (
                                                                <>
                                                                    <button onClick={() => setCompleteWO(wo)} disabled={loading}
                                                                        className="px-2 py-1 bg-green-500 text-white text-xs rounded hover:bg-green-600 disabled:opacity-50 flex items-center gap-1">
                                                                        <CheckCircle size={12} /> Complete
                                                                    </button>
                                                                    <button onClick={() => handlePause(wo.workOrderId, activeExec.executionId)} disabled={loading}
                                                                        className="px-2 py-1 bg-yellow-500 text-white text-xs rounded hover:bg-yellow-600 disabled:opacity-50 flex items-center gap-1">
                                                                        <Pause size={12} /> Pause
                                                                    </button>
                                                                </>
                                                            )}
                                                            <button onClick={() => setActivateWO(wo)} disabled={loading}
                                                                className="px-2 py-1 bg-blue-500 text-white text-xs rounded hover:bg-blue-600 disabled:opacity-50 flex items-center gap-1"
                                                                title="Activate on another equipment">
                                                                <Play size={12} /> +Equip
                                                            </button>
                                                        </>
                                                    )}
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })
                            )}
                        </tbody>
                    </table>
                </div>
            )}

            {/* ─── Modals ─── */}
            {createModal && selectedPOId && (
                <CreateWOModal
                    poId={selectedPOId}
                    onClose={() => setCreateModal(false)}
                    onCreated={() => { setCreateModal(false); fetchWOs(); }}
                />
            )}

            {activateWO && (
                <ActivateWOModal
                    workOrder={activateWO}
                    onClose={() => setActivateWO(null)}
                    onActivated={() => { setActivateWO(null); fetchWOs(); }}
                />
            )}

            {completeWO && (
                <CompleteExecutionModal
                    workOrder={completeWO}
                    execution={getActiveExecution(completeWO)}
                    onClose={() => setCompleteWO(null)}
                    onCompleted={() => { setCompleteWO(null); fetchWOs(); }}
                />
            )}

            {detailWO && (
                <WODetailModal
                    workOrder={detailWO}
                    onClose={() => setDetailWO(null)}
                />
            )}
        </div>
    );
};

export default WOManagement;
