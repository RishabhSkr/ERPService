import React, { useState, useEffect } from 'react';
import { ArrowUpDown, RefreshCw, ArrowUpRight, ArrowDownRight, Lock, Unlock, Trash2, Settings } from 'lucide-react';
import toast from 'react-hot-toast';
import { getStockMovements } from '../../api/inventoryService';

/**
 * Stock Movements Viewer
 * 
 * GET /stock-movements?pageNumber=1&pageSize=50&movementType=IN&itemType=Product
 * Returns: ApiResponse<PagedResponse<StockMovementResponseDto>>
 *   → { success, data: { data: [...movements], pageNumber, pageSize, totalRecords, totalPages } }
 * 
 * StockMovementResponseDto: { id, movementType, itemType, itemId, itemCode, itemName,
 *   quantity, stockBefore, stockAfter, referenceType, referenceId, notes, createdBy, createdAt }
 */
const TYPE_COLORS = {
    IN: 'bg-green-100 text-green-700',
    OUT: 'bg-red-100 text-red-700',
    RESERVE: 'bg-blue-100 text-blue-700',
    RELEASE: 'bg-purple-100 text-purple-700',
    ADJUST: 'bg-cyan-100 text-cyan-700',
    SCRAP: 'bg-orange-100 text-orange-700',
    RETURN: 'bg-yellow-100 text-yellow-700',
};

const TYPE_ICONS = {
    IN: <ArrowDownRight size={14} />,
    OUT: <ArrowUpRight size={14} />,
    RESERVE: <Lock size={14} />,
    RELEASE: <Unlock size={14} />,
    ADJUST: <Settings size={14} />,
    SCRAP: <Trash2 size={14} />,
};

const StockMovements = () => {
    const [movements, setMovements] = useState([]);
    const [loading, setLoading] = useState(true);
    const [totalRecords, setTotalRecords] = useState(0);
    const [filter, setFilter] = useState({ pageNumber: 1, pageSize: 50, movementType: '', itemType: '' });

    const fetchData = async () => {
        setLoading(true);
        try {
            const params = { ...filter };
            if (!params.movementType) delete params.movementType;
            if (!params.itemType) delete params.itemType;
            const res = await getStockMovements(params);
            // Unwrap: ApiResponse<PagedResponse> → { data: { data: { data: [...], totalRecords } } }
            const apiData = res.data?.data || res.data || {};
            const pagedData = apiData?.data || apiData;
            // PagedResponse has .data (array) and .totalRecords
            if (Array.isArray(pagedData)) {
                setMovements(pagedData);
                setTotalRecords(pagedData.length);
            } else {
                const items = pagedData?.data || [];
                setMovements(Array.isArray(items) ? items : []);
                setTotalRecords(pagedData?.totalRecords || items.length || 0);
            }
        } catch (err) {
            console.error('Stock movements error:', err);
            setMovements([]);
            toast.error('Failed to load stock movements');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchData(); }, [filter]);

    const formatDate = (d) => {
        if (!d) return '-';
        // Ensure it is parsed as UTC if backend string is missing 'Z'
        const dateString = d.endsWith('Z') || d.includes('+') ? d : `${d}Z`;
        return new Date(dateString).toLocaleString('en-IN', { 
            day: '2-digit', month: 'short', year: 'numeric', 
            hour: '2-digit', minute: '2-digit', hour12: true 
        });
    };

    return (
        <div className="p-6">
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <ArrowUpDown className="text-blue-500" size={28} />
                    <h1 className="text-2xl font-bold text-slate-800">Stock Movements</h1>
                    <span className="text-sm text-slate-400">({totalRecords} total)</span>
                </div>
                <button onClick={fetchData} className="flex items-center gap-2 px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">
                    <RefreshCw size={16} /> Refresh
                </button>
            </div>

            {/* Filters */}
            <div className="flex flex-wrap gap-4 mb-4">
                {/* Movement Type */}
                <div className="flex gap-1.5 items-center">
                    <span className="text-xs text-slate-500 font-medium mr-1">Type:</span>
                    {['', 'IN', 'OUT', 'RESERVE', 'RELEASE', 'ADJUST', 'SCRAP'].map(type => (
                        <button
                            key={type}
                            onClick={() => setFilter({ ...filter, movementType: type, pageNumber: 1 })}
                            className={`px-2.5 py-1 rounded-full text-xs font-medium transition-colors ${
                                filter.movementType === type ? 'bg-blue-600 text-white' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                            }`}
                        >
                            {type || 'All'}
                        </button>
                    ))}
                </div>
                {/* Item Type */}
                <div className="flex gap-1.5 items-center">
                    <span className="text-xs text-slate-500 font-medium mr-1">Item:</span>
                    {['', 'Product', 'RawMaterial'].map(type => (
                        <button
                            key={type}
                            onClick={() => setFilter({ ...filter, itemType: type, pageNumber: 1 })}
                            className={`px-2.5 py-1 rounded-full text-xs font-medium transition-colors ${
                                filter.itemType === type ? 'bg-indigo-600 text-white' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                            }`}
                        >
                            {type || 'All'}
                        </button>
                    ))}
                </div>
            </div>

            <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
                <table className="w-full">
                    <thead className="bg-slate-50 border-b border-slate-200">
                        <tr>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Date</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Type</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Item Type</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Item</th>
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Qty</th>
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Before</th>
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">After</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Location</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Notes</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                        {loading ? (
                            <tr><td colSpan={8} className="text-center py-10 text-slate-400">Loading...</td></tr>
                        ) : movements.length === 0 ? (
                            <tr><td colSpan={8} className="text-center py-10 text-slate-400">No movements found</td></tr>
                        ) : movements.map((m, i) => {
                            // Extract PO and WO numbers to style them nicely in notes
                            let formattedNotes = m.notes || '-';
                            if (m.notes) {
                                // Simple string replacement to wrap PO-xxxx, WO-xxxx, and BOM-xxxx in styled spans
                                formattedNotes = m.notes.split(/(PO-\d{4}-\d{4}|WO-\d{4}-\d{4}|BOM-\d{4}-\d{4} v\d+)/).map((part, index) => {
                                    if (part.startsWith('PO-')) {
                                        return <span key={index} className="font-bold text-blue-600 bg-blue-50 px-1 py-0.5 rounded mx-0.5">{part}</span>;
                                    }
                                    if (part.startsWith('WO-')) {
                                        return <span key={index} className="font-bold text-indigo-600 bg-indigo-50 px-1 py-0.5 rounded mx-0.5">{part}</span>;
                                    }
                                    if (part.startsWith('BOM-')) {
                                        return <span key={index} className="font-bold text-emerald-600 bg-emerald-50 px-1 py-0.5 rounded mx-0.5">{part}</span>;
                                    }
                                    return part;
                                });
                            }

                            return (
                            <tr key={m.id || i} className="hover:bg-slate-50">
                                <td className="px-4 py-3 text-xs text-slate-500">{formatDate(m.createdAt)}</td>
                                <td className="px-4 py-3">
                                    <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${TYPE_COLORS[m.movementType] || 'bg-gray-100'}`}>
                                        {TYPE_ICONS[m.movementType]} {m.movementType}
                                    </span>
                                </td>
                                <td className="px-4 py-3 text-xs">
                                    <span className={`px-1.5 py-0.5 rounded text-xs ${m.itemType === 'Product' ? 'bg-blue-50 text-blue-600' : 'bg-orange-50 text-orange-600'}`}>
                                        {m.itemType}
                                    </span>
                                </td>
                                <td className="px-4 py-3">
                                    <p className="text-sm font-medium text-slate-700">{m.itemName}</p>
                                    <p className="text-xs text-slate-400 font-mono">{m.itemCode}</p>
                                </td>
                                <td className="px-4 py-3 text-sm text-right font-bold">
                                    <span className={m.movementType === 'IN' || m.movementType === 'RETURN' || m.movementType === 'RELEASE' ? 'text-green-600' : 'text-red-600'}>
                                        {m.movementType === 'IN' || m.movementType === 'RETURN' || m.movementType === 'RELEASE' ? '+' : '-'}{m.quantity}
                                    </span>
                                </td>
                                <td className="px-4 py-3 text-sm text-right text-slate-500">{m.stockBefore}</td>
                                <td className="px-4 py-3 text-sm text-right font-medium text-slate-700">{m.stockAfter}</td>
                                <td className="px-4 py-3 text-xs">
                                    <div className="flex flex-col gap-1">
                                        {m.fromLocationCode && (
                                            <span className="text-slate-500"><span className="text-[10px] uppercase font-bold text-slate-400">From:</span> {m.fromLocationCode}</span>
                                        )}
                                        {m.toLocationCode && (
                                            <span className="text-slate-500"><span className="text-[10px] uppercase font-bold text-slate-400">To:</span> {m.toLocationCode}</span>
                                        )}
                                        {!m.fromLocationCode && !m.toLocationCode && <span className="text-slate-400">-</span>}
                                    </div>
                                </td>
                                <td className="px-4 py-3 text-xs text-slate-500 max-w-xs break-words leading-relaxed">
                                    {formattedNotes}
                                </td>
                            </tr>
                            );
                        })}
                    </tbody>
                </table>
            </div>

            {/* Pagination */}
            {totalRecords > filter.pageSize && (
                <div className="flex items-center justify-center gap-2 mt-4">
                    <button
                        onClick={() => setFilter({ ...filter, pageNumber: Math.max(1, filter.pageNumber - 1) })}
                        disabled={filter.pageNumber <= 1}
                        className="px-3 py-1.5 rounded bg-slate-100 text-sm disabled:opacity-40"
                    >
                        ← Prev
                    </button>
                    <span className="text-sm text-slate-500">Page {filter.pageNumber}</span>
                    <button
                        onClick={() => setFilter({ ...filter, pageNumber: filter.pageNumber + 1 })}
                        disabled={movements.length < filter.pageSize}
                        className="px-3 py-1.5 rounded bg-slate-100 text-sm disabled:opacity-40"
                    >
                        Next →
                    </button>
                </div>
            )}
        </div>
    );
};

export default StockMovements;
