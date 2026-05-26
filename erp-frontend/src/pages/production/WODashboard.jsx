import { useCallback, useEffect, useState } from 'react';
import { Factory, ChevronDown, ChevronRight, CheckCircle, Clock, AlertCircle } from 'lucide-react';
import useApi from '../../hooks/useApi';
import { getWODashboard } from '../../api/productionService';

/**
 * Work Order Dashboard
 * Shows all Production Orders with per-route, per-step WO progress.
 * API: GET /api/production/work-orders/dashboard
 * 
 * Each PO has routes[], each route has steps[] from ProcessRoute.
 */

const STEP_STATUS_COLORS = {
    'New': 'bg-gray-100 text-gray-600',
    'Planned': 'bg-blue-100 text-blue-700',
    'In Progress': 'bg-yellow-100 text-yellow-700',
    'Completed': 'bg-green-100 text-green-700',
};

const PO_STATUS_COLORS = {
    Created: 'bg-yellow-100 text-yellow-700',
    Released: 'bg-purple-100 text-purple-700',
    InProgress: 'bg-blue-100 text-blue-700',
    Completed: 'bg-green-100 text-green-700',
    Cancelled: 'bg-red-100 text-red-700',
};

const WODashboard = () => {
    const [poList, setPOList] = useState([]);
    const [expandedPO, setExpandedPO] = useState(null);
    const { loading, requestHandlerFunction } = useApi();

    const fetchDashboard = useCallback(async () => {
        const response = await requestHandlerFunction(() => getWODashboard());
        if (response.success) {
            const data = response.data?.data?.data || response.data?.data || [];
            setPOList(Array.isArray(data) ? data : []);
        }
    }, [requestHandlerFunction]);

    useEffect(() => {
        fetchDashboard();
    }, [fetchDashboard]);

    const toggleExpand = (poId) => {
        setExpandedPO(prev => prev === poId ? null : poId);
    };

    // Summary stats
    const totalPOs = poList.length;
    const allSteps = poList.flatMap(po => (po.routes || []).flatMap(r => r.steps || []));
    const totalSteps = allSteps.length;
    const inProgressSteps = allSteps.filter(s => s.displayStatus === 'In Progress').length;

    return (
        <div className="p-6 space-y-6">
            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold text-slate-800 flex items-center gap-2">
                    <Factory className="text-indigo-600" size={28} />
                    Work Order Dashboard
                </h1>
                <p className="text-sm text-slate-500 mt-1">
                    Production Order execution — step-by-step progress tracking
                </p>
            </div>

            {/* Summary Cards */}
            <div className="grid grid-cols-3 gap-4">
                <div className="bg-white rounded-xl border border-slate-200 p-4">
                    <p className="text-xs text-slate-400 uppercase font-medium">Active POs</p>
                    <p className="text-3xl font-bold text-slate-800 mt-1">{totalPOs}</p>
                </div>
                <div className="bg-indigo-50 rounded-xl border border-indigo-200 p-4">
                    <p className="text-xs text-indigo-600 uppercase font-medium">Total Steps</p>
                    <p className="text-3xl font-bold text-indigo-700 mt-1">{totalSteps}</p>
                </div>
                <div className="bg-yellow-50 rounded-xl border border-yellow-200 p-4">
                    <p className="text-xs text-yellow-600 uppercase font-medium">In Progress</p>
                    <p className="text-3xl font-bold text-yellow-700 mt-1">{inProgressSteps}</p>
                </div>
            </div>

            {/* Loading */}
            {loading && poList.length === 0 && (
                <div className="text-center py-12 text-slate-400">Loading work order data...</div>
            )}

            {/* Empty */}
            {!loading && poList.length === 0 && (
                <div className="bg-white p-8 rounded-lg shadow text-center text-gray-500">
                    No active production orders with work order data.
                </div>
            )}

            {/* PO Cards with expandable steps */}
            <div className="space-y-3">
                {poList.map((po) => {
                    const isExpanded = expandedPO === po.productionOrderId;
                    const poSteps = (po.routes || []).flatMap(r => r.steps || []);
                    const overallProgress = poSteps.length > 0
                        ? Math.round(poSteps.reduce((s, step) => s + (step.progressPercentage || 0), 0) / poSteps.length)
                        : 0;

                    return (
                        <div key={po.productionOrderId} className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
                            {/* PO Header Row — clickable */}
                            <button
                                onClick={() => toggleExpand(po.productionOrderId)}
                                className="w-full flex items-center justify-between px-5 py-4 hover:bg-slate-50 transition-colors text-left"
                            >
                                <div className="flex items-center gap-4">
                                    {isExpanded ? <ChevronDown size={18} className="text-slate-400" /> : <ChevronRight size={18} className="text-slate-400" />}
                                    <div>
                                        <span className="font-mono font-bold text-slate-800">{po.orderNumber}</span>
                                        <div className="text-xs text-slate-500">{po.productName} ({po.productCode})</div>
                                    </div>
                                </div>
                                <div className="flex items-center gap-6">
                                    <div className="text-right">
                                        <p className="text-xs text-slate-400">Planned</p>
                                        <p className="font-bold text-slate-700">{po.poQuantityPlanned}</p>
                                    </div>
                                    <div className="flex items-center gap-2 w-32">
                                        <div className="flex-1 h-2 bg-slate-200 rounded-full overflow-hidden">
                                            <div 
                                                className="h-full rounded-full transition-all"
                                                style={{ 
                                                    width: `${overallProgress}%`,
                                                    backgroundColor: overallProgress >= 100 ? '#10b981' : '#6366f1' 
                                                }}
                                            />
                                        </div>
                                        <span className="text-xs font-medium text-slate-500 w-8">{overallProgress}%</span>
                                    </div>
                                    <span className={`px-2.5 py-1 rounded-full text-xs font-semibold ${PO_STATUS_COLORS[po.poStatus] || 'bg-gray-100'}`}>
                                        {po.poStatus}
                                    </span>
                                    <span className="text-xs text-slate-400">{po.routes?.length || 0} routes</span>
                                </div>
                            </button>

                            {/* Expanded: Step-by-step breakdown */}
                            {isExpanded && (
                                <div className="border-t border-slate-100 bg-slate-50/50">
                                    {(po.routes || []).map((route) => (
                                        <div key={route.processRouteId} className="border-b border-slate-100 last:border-b-0">
                                            <div className="px-5 py-2 bg-slate-100/60 text-xs font-semibold text-slate-600 flex items-center gap-2">
                                                <span className="bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded">{route.routeCode}</span>
                                                <span className="text-slate-400">—</span>
                                                <span>{route.workCenterName}</span>
                                            </div>
                                            <table className="w-full text-sm">
                                                <thead className="text-xs text-slate-500 uppercase bg-slate-100/40">
                                                    <tr>
                                                        <th className="px-5 py-2 text-left">Step</th>
                                                        <th className="px-5 py-2 text-left">Process</th>
                                                        <th className="px-5 py-2 text-center">Planned</th>
                                                        <th className="px-5 py-2 text-center">Completed</th>
                                                        <th className="px-5 py-2 text-center">Unplanned</th>
                                                        <th className="px-5 py-2 text-center">Progress</th>
                                                        <th className="px-5 py-2 text-center">WOs</th>
                                                        <th className="px-5 py-2">Status</th>
                                                    </tr>
                                                </thead>
                                                <tbody className="divide-y divide-slate-100">
                                                    {(route.steps || []).map((step) => (
                                                        <tr key={step.processRouteStepId} className="hover:bg-white transition-colors">
                                                            <td className="px-5 py-3">
                                                                <span className="bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded-full text-xs font-bold">
                                                                    #{step.stepNumber}
                                                                </span>
                                                            </td>
                                                            <td className="px-5 py-3">
                                                                <span className="font-medium text-slate-700">{step.processName}</span>
                                                                <div className="text-xs text-slate-400">{step.processCode}</div>
                                                            </td>
                                                            <td className="px-5 py-3 text-center font-semibold text-blue-600">
                                                                {step.totalPlanned || 0} <span className="text-xs font-normal text-slate-400">{step.outputUnit}</span>
                                                            </td>
                                                            <td className="px-5 py-3 text-center">
                                                                <span className="font-semibold text-green-600 flex items-center gap-1 justify-center">
                                                                    <CheckCircle size={12} /> {step.totalCompleted || 0} <span className="text-xs font-normal text-slate-400">{step.outputUnit}</span>
                                                                </span>
                                                            </td>
                                                            <td className="px-5 py-3 text-center">
                                                                <span className={`font-semibold ${step.unplannedQuantity > 0 ? 'text-red-500' : 'text-slate-400'}`}>
                                                                    {step.unplannedQuantity || 0} <span className="text-xs font-normal">{step.outputUnit}</span>
                                                                </span>
                                                            </td>
                                                            <td className="px-5 py-3">
                                                                <div className="flex items-center justify-center gap-2">
                                                                    <div className="w-16 h-1.5 bg-slate-200 rounded-full overflow-hidden">
                                                                        <div 
                                                                            className="h-full rounded-full"
                                                                            style={{ 
                                                                                width: `${step.progressPercentage || 0}%`,
                                                                                backgroundColor: step.progressPercentage >= 100 ? '#10b981' : '#6366f1'
                                                                            }}
                                                                        />
                                                                    </div>
                                                                    <span className="text-xs font-medium text-slate-500">{step.progressPercentage || 0}%</span>
                                                                </div>
                                                            </td>
                                                            <td className="px-5 py-3 text-center font-medium text-slate-600">
                                                                {step.woCount || 0}
                                                            </td>
                                                            <td className="px-5 py-3">
                                                                <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${STEP_STATUS_COLORS[step.displayStatus] || 'bg-gray-100'}`}>
                                                                    {step.displayStatus}
                                                                </span>
                                                            </td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                            </table>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>
        </div>
    );
};

export default WODashboard;
