import React from 'react';
import { X, Clock, CheckCircle, AlertTriangle, Zap, Layers, ArrowRight } from 'lucide-react';

const STATUS_COLORS = {
    Pending: 'bg-yellow-100 text-yellow-700',
    Released: 'bg-purple-100 text-purple-700',
    InProgress: 'bg-blue-100 text-blue-700',
    Completed: 'bg-green-100 text-green-700',
    Cancelled: 'bg-red-100 text-red-700',
    Active: 'bg-green-100 text-green-700',
    Paused: 'bg-yellow-100 text-yellow-700',
};

const WODetailModal = ({ workOrder, onClose }) => {
    if (!workOrder) return null;

    const formatDate = (d) => d ? new Date(d).toLocaleString('en-IN') : '-';

    const executions = workOrder.executions || [];

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-3xl max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="flex items-center justify-between p-5 border-b sticky top-0 bg-white rounded-t-2xl z-10">
                    <h2 className="text-lg font-bold text-slate-800">
                        Work Order — {workOrder.workOrderNumber}
                    </h2>
                    <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-slate-100"><X size={20} /></button>
                </div>

                <div className="p-5 space-y-6">
                    {/* Overview Grid */}
                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                        <InfoCard label="WO Number" value={workOrder.workOrderNumber} />
                        <InfoCard label="PO Number" value={workOrder.productionOrderNumber} />
                        <InfoCard label="Status">
                            <span className={`px-2.5 py-1 rounded-full text-xs font-semibold ${STATUS_COLORS[workOrder.status] || 'bg-gray-100'}`}>
                                {workOrder.status}
                            </span>
                        </InfoCard>
                        <InfoCard label="Product" value={`${workOrder.productName} (${workOrder.productCode})`} />

                        {workOrder.stepNumber === 0 ? (
                            // ── Entire Route WO ─────────────────────────────────────
                            <div className="col-span-2 md:col-span-3 bg-emerald-50 border border-emerald-200 rounded-lg p-3 space-y-2">
                                <div className="flex items-center gap-2">
                                    <span className="flex items-center gap-1.5 px-2.5 py-1 bg-emerald-100 text-emerald-700 rounded-full text-xs font-bold">
                                        <Layers size={12} /> Entire Route WO
                                    </span>
                                    <span className="text-xs text-slate-500">Work Center Level Tracking</span>
                                </div>
                                <p className="text-xs text-slate-600">
                                    <span className="font-semibold">Route:</span> {workOrder.routeCode} v{workOrder.routeVersion}
                                    {workOrder.workCenterName && <> &nbsp;|&nbsp; <span className="font-semibold">🏭</span> {workOrder.workCenterName}</>}
                                </p>
                                <p className="text-xs text-amber-700 bg-amber-50 rounded px-2 py-1">
                                    💡 <strong>Output = Last step ka final quantity.</strong> Intermediate steps tracked internally.
                                </p>
                            </div>
                        ) : (
                            // ── Individual Step WO ──────────────────────────────────
                            <>
                                <InfoCard label="Step" value={`#${workOrder.stepNumber} — ${workOrder.operationName}`} />
                                <InfoCard label="Process" value={workOrder.processCode || '-'} />
                                <InfoCard label="Work Center" value={workOrder.workCenterName ? `${workOrder.workCenterName} (${workOrder.workCenterCode})` : '-'} />
                                <InfoCard label="Route" value={workOrder.routeCode ? `${workOrder.routeCode} v${workOrder.routeVersion}` : '-'} />
                            </>
                        )}
                    </div>

                    {/* Quantity Section */}
                    <div>
                        <h3 className="text-sm font-semibold text-slate-600 uppercase mb-2">Quantities</h3>
                        <div className="grid grid-cols-3 gap-3">
                            <div className="bg-blue-50 rounded-xl p-3 text-center">
                                <p className="text-xs text-blue-500">Planned</p>
                                <p className="text-2xl font-bold text-blue-700">{workOrder.quantityPlanned}</p>
                            </div>
                            <div className="bg-green-50 rounded-xl p-3 text-center">
                                <p className="text-xs text-green-500 flex items-center justify-center gap-1"><CheckCircle size={12} /> Completed</p>
                                <p className="text-2xl font-bold text-green-700">{workOrder.quantityCompleted || 0}</p>
                            </div>
                            <div className="bg-red-50 rounded-xl p-3 text-center">
                                <p className="text-xs text-red-500 flex items-center justify-center gap-1"><AlertTriangle size={12} /> Scrap</p>
                                <p className="text-2xl font-bold text-red-600">{workOrder.quantityScrap || 0}</p>
                            </div>
                        </div>
                        {/* Progress bar */}
                        <div className="mt-2">
                            <div className="w-full h-2 bg-slate-200 rounded-full overflow-hidden">
                                <div className="h-full rounded-full bg-indigo-500 transition-all"
                                    style={{ width: `${Math.min(((workOrder.quantityCompleted || 0) / workOrder.quantityPlanned) * 100, 100)}%` }} />
                            </div>
                            <p className="text-xs text-slate-400 mt-1 text-right">
                                {Math.round(((workOrder.quantityCompleted || 0) / workOrder.quantityPlanned) * 100)}% complete
                            </p>
                        </div>
                    </div>

                    {/* Reservation */}
                    {workOrder.reservationStatus && (
                        <div>
                            <h3 className="text-sm font-semibold text-slate-600 uppercase mb-2">Reservation</h3>
                            <div className="bg-slate-50 rounded-xl p-3 flex items-center gap-4 text-sm">
                                <span className={`px-2.5 py-1 rounded-full text-xs font-semibold ${
                                    workOrder.reservationStatus === 'Reserved' ? 'bg-green-100 text-green-700' :
                                    workOrder.reservationStatus === 'Failed' ? 'bg-red-100 text-red-700' :
                                    'bg-yellow-100 text-yellow-700'
                                }`}>
                                    📦 {workOrder.reservationStatus}
                                </span>
                                {workOrder.reservationFailReason && (
                                    <span className="text-red-500 text-xs">{workOrder.reservationFailReason}</span>
                                )}
                            </div>
                        </div>
                    )}

                    {/* Dates */}
                    <div>
                        <h3 className="text-sm font-semibold text-slate-600 uppercase mb-2">Timeline</h3>
                        <div className="grid grid-cols-2 gap-3 text-sm">
                            <DateRow icon={<Clock size={14} className="text-slate-400" />} label="Scheduled Start" value={formatDate(workOrder.scheduledStart)} />
                            <DateRow icon={<Clock size={14} className="text-slate-400" />} label="Scheduled End" value={formatDate(workOrder.scheduledEnd)} />
                            <DateRow icon={<CheckCircle size={14} className="text-green-500" />} label="Actual Start" value={formatDate(workOrder.actualStartDate)} />
                            <DateRow icon={<CheckCircle size={14} className="text-green-500" />} label="Actual End" value={formatDate(workOrder.actualEndDate)} />
                            <DateRow icon={<Clock size={14} className="text-blue-400" />} label="Created" value={formatDate(workOrder.createdAt)} />
                        </div>
                    </div>

                    {/* Cancel Info */}
                    {workOrder.cancelReason && (
                        <div className="bg-red-50 border border-red-200 rounded-xl p-3">
                            <p className="text-xs text-red-400 uppercase font-semibold">Cancel Reason</p>
                            <p className="text-sm text-red-700 mt-1">{workOrder.cancelReason}</p>
                        </div>
                    )}

                    {/* Notes */}
                    {workOrder.notes && (
                        <div className="bg-slate-50 rounded-xl p-3">
                            <p className="text-xs text-slate-400 uppercase font-semibold">Notes</p>
                            <p className="text-sm text-slate-600 mt-1">{workOrder.notes}</p>
                        </div>
                    )}

                    {/* Execution History */}
                    <div>
                        <h3 className="text-sm font-semibold text-slate-600 uppercase mb-2 flex items-center gap-1">
                            <Zap size={14} /> Execution History ({executions.length})
                        </h3>
                        {executions.length === 0 ? (
                            <p className="text-sm text-slate-400">No executions yet.</p>
                        ) : (
                            <div className="border rounded-xl overflow-hidden">
                                <table className="w-full text-sm">
                                    <thead className="bg-slate-50 text-xs text-slate-500 uppercase">
                                        <tr>
                                            <th className="p-2.5 text-left">Equipment</th>
                                            <th className="p-2.5 text-left">Operator</th>
                                            <th className="p-2.5 text-center">Produced</th>
                                            <th className="p-2.5 text-center">Scrap</th>
                                            <th className="p-2.5">Status</th>
                                            <th className="p-2.5">Started</th>
                                            <th className="p-2.5">Ended</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y">
                                        {executions.map((exec) => (
                                            <tr key={exec.executionId} className="hover:bg-slate-50">
                                                <td className="p-2.5">
                                                    <span className="font-medium">{exec.equipmentCode}</span>
                                                    <div className="text-xs text-slate-400">{exec.equipmentName}</div>
                                                </td>
                                                <td className="p-2.5 text-slate-600">{exec.activatedBy}</td>
                                                <td className="p-2.5 text-center font-semibold text-green-600">{exec.quantityProduced}</td>
                                                <td className="p-2.5 text-center font-semibold text-red-500">{exec.quantityScrap}</td>
                                                <td className="p-2.5">
                                                    <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${STATUS_COLORS[exec.status] || 'bg-gray-100'}`}>
                                                        {exec.status}
                                                    </span>
                                                </td>
                                                <td className="p-2.5 text-xs text-slate-500">{formatDate(exec.activatedAt)}</td>
                                                <td className="p-2.5 text-xs text-slate-500">{formatDate(exec.deactivatedAt)}</td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

// Helper components
const InfoCard = ({ label, value, children }) => (
    <div className="bg-slate-50 rounded-lg p-2.5">
        <p className="text-xs text-slate-400 uppercase">{label}</p>
        {children || <p className="font-medium text-slate-700 text-sm mt-0.5">{value}</p>}
    </div>
);

const DateRow = ({ icon, label, value }) => (
    <div className="flex items-center gap-2 bg-slate-50 rounded-lg px-3 py-2">
        {icon}
        <span className="text-slate-400 text-xs">{label}:</span>
        <span className="text-slate-700 font-medium text-xs">{value}</span>
    </div>
);

export default WODetailModal;
