import React from 'react';
import { X, Package, Calendar, AlertTriangle, CheckCircle, Clock, Layers, Truck, Info } from 'lucide-react';

const STATUS_COLORS = {
    Created: 'bg-yellow-100 text-yellow-700',
    Released: 'bg-purple-100 text-purple-700',
    InProgress: 'bg-blue-100 text-blue-700',
    Completed: 'bg-green-100 text-green-700',
    Cancelled: 'bg-red-100 text-red-700',
};

const RESERVATION_COLORS = {
    Reserved: 'bg-green-100 text-green-700',
    Partial: 'bg-orange-100 text-orange-700',
    Pending: 'bg-yellow-100 text-yellow-700',
    Failed: 'bg-red-100 text-red-700',
};

const PRIORITY_LABELS = {
    1: { label: 'High', color: 'text-red-600 bg-red-50' },
    2: { label: 'Normal', color: 'text-blue-600 bg-blue-50' },
    3: { label: 'Low', color: 'text-gray-600 bg-gray-50' },
};

const formatDate = (d) => {
    if (!d) return '-';
    return new Date(d).toLocaleString('en-IN', {
        day: '2-digit', month: 'short', year: 'numeric',
        hour: '2-digit', minute: '2-digit'
    });
};

const InfoRow = ({ label, value, icon: Icon, valueClass = '' }) => (
    <div className="flex items-start gap-2 py-2">
        {Icon && <Icon size={14} className="text-slate-400 mt-0.5 shrink-0" />}
        <div className="min-w-0">
            <p className="text-xs text-slate-400 uppercase font-medium">{label}</p>
            <p className={`text-sm font-medium text-slate-700 mt-0.5 ${valueClass}`}>{value || '-'}</p>
        </div>
    </div>
);

/**
 * ProductionOrderDetailModal
 * Shows ALL fields of a Production Order + Material Requirements table
 */
const ProductionOrderDetailModal = ({ order, onClose }) => {
    if (!order) return null;

    const materials = order.materialRequirements || [];
    const priority = PRIORITY_LABELS[order.priority] || PRIORITY_LABELS[2];

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-4xl max-h-[92vh] overflow-y-auto">
                
                {/* ─── Header ─── */}
                <div className="flex items-center justify-between p-5 border-b border-slate-200 sticky top-0 bg-white z-10 rounded-t-2xl">
                    <div>
                        <h2 className="text-lg font-bold text-slate-800 flex items-center gap-2">
                            <Package size={20} className="text-blue-500" />
                            {order.orderNumber}
                        </h2>
                        <p className="text-sm text-slate-500 mt-0.5">{order.productName} ({order.productCode})</p>
                    </div>
                    <div className="flex items-center gap-3">
                        <span className={`px-3 py-1 rounded-full text-xs font-semibold ${STATUS_COLORS[order.status] || 'bg-gray-100'}`}>
                            {order.status}
                        </span>
                        <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${priority.color}`}>
                            ⚡ {priority.label}
                        </span>
                        <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-slate-100 transition-colors">
                            <X size={20} />
                        </button>
                    </div>
                </div>

                {/* ─── Order Info Grid ─── */}
                <div className="p-5 grid grid-cols-2 md:grid-cols-4 gap-x-6 gap-y-1 border-b border-slate-100">
                    <InfoRow label="Order Number" value={order.orderNumber} icon={Package} />
                    <InfoRow label="Sales Order" value={order.salesOrderNumber || 'Manual Order'} icon={Truck} />
                    <InfoRow label="Product" value={`${order.productName} (${order.productCode})`} icon={Layers} />
                    <InfoRow label="BOM" value={order.bomCode ? `${order.bomCode} v${order.bomVersion}` : '-'} icon={Layers} />
                </div>

                {/* ─── Quantities ─── */}
                <div className="p-5 border-b border-slate-100">
                    <h3 className="text-sm font-semibold text-slate-700 mb-3 flex items-center gap-2">
                        <CheckCircle size={16} className="text-green-500" /> Quantities
                    </h3>
                    <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
                        <div className="bg-blue-50 rounded-xl p-3 text-center">
                            <p className="text-xs text-blue-500 font-medium">Planned</p>
                            <p className="text-2xl font-bold text-blue-700">{order.quantityPlanned}</p>
                        </div>
                        <div className="bg-green-50 rounded-xl p-3 text-center">
                            <p className="text-xs text-green-500 font-medium">Produced</p>
                            <p className="text-2xl font-bold text-green-700">{order.quantityProduced || 0}</p>
                        </div>
                        <div className="bg-emerald-50 rounded-xl p-3 text-center">
                            <p className="text-xs text-emerald-500 font-medium">Good</p>
                            <p className="text-2xl font-bold text-emerald-700">{order.quantityGood || 0}</p>
                        </div>
                        <div className="bg-red-50 rounded-xl p-3 text-center">
                            <p className="text-xs text-red-500 font-medium">Scrap</p>
                            <p className="text-2xl font-bold text-red-700">{order.quantityScrap || 0}</p>
                        </div>
                        <div className="bg-purple-50 rounded-xl p-3 text-center">
                            <p className="text-xs text-purple-500 font-medium">% Complete</p>
                            <p className="text-2xl font-bold text-purple-700">{order.percentComplete || 0}%</p>
                        </div>
                    </div>
                </div>

                {/* ─── Timeline ─── */}
                <div className="p-5 grid grid-cols-2 md:grid-cols-4 gap-x-6 gap-y-1 border-b border-slate-100">
                    <InfoRow label="Planned Start" value={formatDate(order.plannedStartDate)} icon={Calendar} />
                    <InfoRow label="Planned End" value={formatDate(order.plannedEndDate)} icon={Calendar} />
                    <InfoRow label="Actual Start" value={formatDate(order.actualStartDate)} icon={Clock} 
                        valueClass={order.actualStartDate ? 'text-blue-600' : ''} />
                    <InfoRow label="Actual End" value={formatDate(order.actualEndDate)} icon={Clock}
                        valueClass={order.actualEndDate ? 'text-green-600' : ''} />
                </div>

                {/* ─── Reservation & Status Info ─── */}
                <div className="p-5 grid grid-cols-2 md:grid-cols-4 gap-x-6 gap-y-1 border-b border-slate-100">
                    <div className="py-2">
                        <p className="text-xs text-slate-400 uppercase font-medium">Reservation Status</p>
                        <span className={`inline-block mt-1 px-2.5 py-0.5 rounded-full text-xs font-semibold ${RESERVATION_COLORS[order.reservationStatus] || 'bg-gray-100'}`}>
                            {order.reservationStatus || '-'}
                        </span>
                    </div>
                    <InfoRow label="Reservation Attempts" value={order.reservationAttempts} icon={Info} />
                    <InfoRow label="Released At" value={formatDate(order.releasedAt)} icon={Calendar} />
                    <InfoRow label="Created At" value={formatDate(order.createdAt)} icon={Calendar} />
                </div>

                {/* ─── Cancel Info (if cancelled) ─── */}
                {order.status === 'Cancelled' && (
                    <div className="p-5 bg-red-50 border-b border-red-100">
                        <h3 className="text-sm font-semibold text-red-700 mb-2 flex items-center gap-2">
                            <AlertTriangle size={16} /> Cancellation Info
                        </h3>
                        <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                            <div>
                                <p className="text-xs text-red-400">Reason</p>
                                <p className="font-medium text-red-700">{order.cancelReason || '-'}</p>
                            </div>
                            <div>
                                <p className="text-xs text-red-400">Cancelled At</p>
                                <p className="font-medium text-red-700">{formatDate(order.cancelledAt)}</p>
                            </div>
                            <div>
                                <p className="text-xs text-red-400">Materials Returned</p>
                                <p className="font-medium text-red-700">{order.materialsReturned ? '✅ Yes' : '❌ No'}</p>
                            </div>
                        </div>
                    </div>
                )}

                {/* ─── Notes ─── */}
                {order.notes && (
                    <div className="px-5 py-3 border-b border-slate-100">
                        <p className="text-xs text-slate-400 uppercase font-medium">Notes</p>
                        <p className="text-sm text-slate-600 mt-1">{order.notes}</p>
                    </div>
                )}

                {/* ─── Material Requirements ─── */}
                <div className="p-5">
                    <h3 className="text-sm font-semibold text-slate-700 mb-3 flex items-center gap-2">
                        <Layers size={16} className="text-indigo-500" />
                        Material Requirements ({materials.length})
                    </h3>
                    {materials.length === 0 ? (
                        <p className="text-sm text-slate-400 text-center py-4">No material requirements</p>
                    ) : (
                        <div className="overflow-x-auto">
                            <table className="w-full text-sm">
                                <thead className="bg-slate-50 border-y border-slate-200">
                                    <tr>
                                        <th className="text-left px-3 py-2 text-xs font-semibold text-slate-500">Material</th>
                                        <th className="text-right px-3 py-2 text-xs font-semibold text-slate-500">Required</th>
                                        <th className="text-right px-3 py-2 text-xs font-semibold text-slate-500">Reserved</th>
                                        <th className="text-right px-3 py-2 text-xs font-semibold text-slate-500">Consumed</th>
                                        <th className="text-center px-3 py-2 text-xs font-semibold text-slate-500">Unit</th>
                                        <th className="text-center px-3 py-2 text-xs font-semibold text-slate-500">Status</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-slate-100">
                                    {materials.map((mat) => (
                                        <tr key={mat.id} className="hover:bg-slate-50">
                                            <td className="px-3 py-2.5">
                                                <p className="font-medium text-slate-700">{mat.materialName}</p>
                                                <p className="text-xs text-slate-400">{mat.materialCode}</p>
                                            </td>
                                            <td className="px-3 py-2.5 text-right font-semibold">{mat.quantityRequired}</td>
                                            <td className="px-3 py-2.5 text-right font-medium text-blue-600">{mat.quantityReserved}</td>
                                            <td className="px-3 py-2.5 text-right font-medium text-green-600">{mat.quantityConsumed}</td>
                                            <td className="px-3 py-2.5 text-center text-slate-500">{mat.unit}</td>
                                            <td className="px-3 py-2.5 text-center">
                                                <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                                                    mat.status === 'Reserved' ? 'bg-green-100 text-green-700' :
                                                    mat.status === 'PartiallyReserved' ? 'bg-orange-100 text-orange-700' :
                                                    mat.status === 'Consumed' ? 'bg-blue-100 text-blue-700' :
                                                    mat.status === 'PartiallyConsumed' ? 'bg-cyan-100 text-cyan-700' :
                                                    mat.status === 'Failed' ? 'bg-red-100 text-red-700' :
                                                    'bg-yellow-100 text-yellow-700'
                                                }`}>
                                                    {mat.status}
                                                </span>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>

                {/* ─── Footer ─── */}
                <div className="flex justify-end p-5 border-t border-slate-100 sticky bottom-0 bg-white rounded-b-2xl">
                    <button
                        onClick={onClose}
                        className="px-5 py-2.5 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm font-medium transition-colors"
                    >
                        Close
                    </button>
                </div>
            </div>
        </div>
    );
};

export default ProductionOrderDetailModal;
