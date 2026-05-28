import React, { useState, useEffect, useCallback } from 'react';
import { ArrowUpDown, RefreshCw, ArrowUpRight, ArrowDownRight, Lock, Unlock, Trash2, Settings, Search, X, Calendar } from 'lucide-react';
import toast from 'react-hot-toast';
import { getStockMovements } from '../../api/inventoryService';

/**
 * Stock Movements Viewer
 * GET /stock-movements?pageNumber=1&pageSize=20&movementType=IN&itemType=Product&startDate=...&endDate=...
 * Returns: ApiResponse<PagedResponse<StockMovementResponseDto>>
 */

const TYPE_COLORS = {
    IN:      'bg-green-100 text-green-700',
    OUT:     'bg-red-100 text-red-700',
    RESERVE: 'bg-blue-100 text-blue-700',
    RELEASE: 'bg-purple-100 text-purple-700',
    ADJUST:  'bg-cyan-100 text-cyan-700',
    SCRAP:   'bg-orange-100 text-orange-700',
    RETURN:  'bg-yellow-100 text-yellow-700',
};

const TYPE_ICONS = {
    IN:      <ArrowDownRight size={13} />,
    OUT:     <ArrowUpRight size={13} />,
    RESERVE: <Lock size={13} />,
    RELEASE: <Unlock size={13} />,
    ADJUST:  <Settings size={13} />,
    SCRAP:   <Trash2 size={13} />,
};

const PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

const StockMovements = () => {
    const [movements, setMovements]       = useState([]);
    const [loading, setLoading]           = useState(true);
    const [totalRecords, setTotalRecords] = useState(0);
    const [totalPages, setTotalPages]     = useState(1);
    const [searchTerm, setSearchTerm]     = useState('');         // client-side text search

    const [filter, setFilter] = useState({
        pageNumber:   1,
        pageSize:     20,
        movementType: '',
        itemType:     '',
        startDate:    '',
        endDate:      '',
    });

    const fetchData = useCallback(async () => {
        setLoading(true);
        try {
            const params = { ...filter };
            if (!params.movementType) delete params.movementType;
            if (!params.itemType)     delete params.itemType;

            // Fix: startDate → T00:00:00, endDate → T23:59:59 (cover full day)
            if (params.startDate) params.startDate = `${params.startDate}T00:00:00`;
            else delete params.startDate;

            if (params.endDate) params.endDate = `${params.endDate}T00:00:00`;
            else delete params.endDate;

            const res   = await getStockMovements(params);
            const paged = res.data?.data;
            setMovements(Array.isArray(paged?.data) ? paged.data : []);
            setTotalRecords(paged?.totalRecords || 0);
            setTotalPages(paged?.totalPages    || 1);
        } catch (err) {
            console.error('Stock movements error:', err);
            setMovements([]);
            toast.error('Failed to load stock movements');
        } finally {
            setLoading(false);
        }
    }, [filter]);

    useEffect(() => { fetchData(); }, [fetchData]);

    // Client-side text search (on current page data)
    const displayedMovements = searchTerm.trim()
        ? movements.filter(m => {
            const s = searchTerm.toLowerCase();
            return (m.itemName  || '').toLowerCase().includes(s)
                || (m.itemCode  || '').toLowerCase().includes(s)
                || (m.notes     || '').toLowerCase().includes(s)
                || (m.referenceId || '').toLowerCase().includes(s);
        })
        : movements;

    const formatDate = (d) => {
        if (!d) return '-';
        const dateString = d.endsWith('Z') || d.includes('+') ? d : `${d}Z`;
        return new Date(dateString).toLocaleString('en-IN', {
            day: '2-digit', month: 'short', year: 'numeric',
            hour: '2-digit', minute: '2-digit', hour12: true,
        });
    };

    const setFilterField = (key, value) =>
        setFilter(prev => ({ ...prev, [key]: value, pageNumber: 1 }));

    const clearFilters = () => {
        setFilter({ pageNumber: 1, pageSize: filter.pageSize, movementType: '', itemType: '', startDate: '', endDate: '' });
        setSearchTerm('');
    };

    const hasActiveFilters = filter.movementType || filter.itemType || filter.startDate || filter.endDate || searchTerm;

    return (
        <div className="p-6 space-y-4">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                    <ArrowUpDown className="text-blue-500" size={26} />
                    <div>
                        <h1 className="text-2xl font-bold text-slate-800">Stock Movements</h1>
                        <p className="text-xs text-slate-400 mt-0.5">
                            {loading ? 'Loading...' : `${totalRecords} total records`}
                            {displayedMovements.length !== movements.length && ` · ${displayedMovements.length} matched`}
                        </p>
                    </div>
                </div>
                <button onClick={fetchData} className="flex items-center gap-2 px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm font-medium">
                    <RefreshCw size={15} /> Refresh
                </button>
            </div>

            {/* Filter Bar */}
            <div className="bg-white rounded-xl border border-slate-200 p-4 space-y-3">
                {/* Row 1: Text search + Date range */}
                <div className="flex flex-wrap gap-3 items-center">
                    {/* Text Search */}
                    <div className="relative flex-1 min-w-[200px] max-w-sm">
                        <Search size={14} className="absolute left-3 top-2.5 text-slate-400" />
                        <input
                            type="text"
                            value={searchTerm}
                            onChange={e => setSearchTerm(e.target.value)}
                            placeholder="Search item, code, notes, WO#, PO#..."
                            className="w-full pl-8 pr-8 py-2 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-300"
                        />
                        {searchTerm && (
                            <button onClick={() => setSearchTerm('')}
                                className="absolute right-2.5 top-2.5 text-slate-400 hover:text-slate-600">
                                <X size={13} />
                            </button>
                        )}
                    </div>

                    {/* Date Range */}
                    <div className="flex items-center gap-2">
                        <Calendar size={14} className="text-slate-400" />
                        <input
                            type="date"
                            value={filter.startDate}
                            onChange={e => setFilterField('startDate', e.target.value)}
                            className="text-sm border border-slate-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-300"
                        />
                        <span className="text-slate-400 text-sm">→</span>
                        <input
                            type="date"
                            value={filter.endDate}
                            onChange={e => setFilterField('endDate', e.target.value)}
                            className="text-sm border border-slate-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-300"
                        />
                    </div>

                    {/* Clear all */}
                    {hasActiveFilters && (
                        <button onClick={clearFilters}
                            className="flex items-center gap-1 px-3 py-2 text-xs text-red-500 border border-red-200 rounded-lg hover:bg-red-50">
                            <X size={12} /> Clear All
                        </button>
                    )}
                </div>

                {/* Row 2: Type + Item Type pills */}
                <div className="flex flex-wrap gap-4 items-center">
                    {/* Movement Type */}
                    <div className="flex gap-1.5 items-center flex-wrap">
                        <span className="text-xs text-slate-500 font-semibold">Type:</span>
                        {['', 'IN', 'OUT', 'RESERVE', 'RELEASE', 'ADJUST', 'SCRAP'].map(type => (
                            <button key={type}
                                onClick={() => setFilterField('movementType', type)}
                                className={`px-2.5 py-1 rounded-full text-xs font-medium transition-colors ${
                                    filter.movementType === type
                                        ? 'bg-blue-600 text-white shadow-sm'
                                        : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                                }`}>
                                {type || 'All'}
                            </button>
                        ))}
                    </div>

                    {/* Item Type */}
                    <div className="flex gap-1.5 items-center">
                        <span className="text-xs text-slate-500 font-semibold">Item:</span>
                        {['', 'Product', 'RawMaterial'].map(type => (
                            <button key={type}
                                onClick={() => setFilterField('itemType', type)}
                                className={`px-2.5 py-1 rounded-full text-xs font-medium transition-colors ${
                                    filter.itemType === type
                                        ? 'bg-indigo-600 text-white shadow-sm'
                                        : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                                }`}>
                                {type || 'All'}
                            </button>
                        ))}
                    </div>

                    {/* Page size */}
                    <div className="flex items-center gap-2 ml-auto">
                        <span className="text-xs text-slate-500">Show:</span>
                        <select
                            value={filter.pageSize}
                            onChange={e => setFilter(prev => ({ ...prev, pageSize: Number(e.target.value), pageNumber: 1 }))}
                            className="text-xs border border-slate-200 rounded px-2 py-1 focus:outline-none">
                            {PAGE_SIZE_OPTIONS.map(s => <option key={s} value={s}>{s} / page</option>)}
                        </select>
                    </div>
                </div>
            </div>

            {/* Table */}
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
                            <tr><td colSpan={9} className="text-center py-12 text-slate-400">
                                <div className="flex items-center justify-center gap-2">
                                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-500" />
                                    Loading movements...
                                </div>
                            </td></tr>
                        ) : displayedMovements.length === 0 ? (
                            <tr><td colSpan={9} className="text-center py-12 text-slate-400">
                                <ArrowUpDown size={32} className="mx-auto mb-2 opacity-20" />
                                {hasActiveFilters || searchTerm ? 'No movements match your search/filter.' : 'No movements found.'}
                            </td></tr>
                        ) : displayedMovements.map((m, i) => {
                            // Highlight PO-xxxx / WO-xxxx / BOM-xxxx in notes
                            const formattedNotes = m.notes
                                ? m.notes.split(/(PO-\d{4}-\d{4}|WO-\d{4}-\d{4}|BOM-\S+)/).map((part, idx) => {
                                    if (part.startsWith('PO-'))  return <span key={idx} className="font-bold text-blue-600 bg-blue-50 px-1 py-0.5 rounded mx-0.5">{part}</span>;
                                    if (part.startsWith('WO-'))  return <span key={idx} className="font-bold text-indigo-600 bg-indigo-50 px-1 py-0.5 rounded mx-0.5">{part}</span>;
                                    if (part.startsWith('BOM-')) return <span key={idx} className="font-bold text-emerald-600 bg-emerald-50 px-1 py-0.5 rounded mx-0.5">{part}</span>;
                                    return part;
                                })
                                : '-';

                            const isPositive = ['IN', 'RETURN', 'RELEASE'].includes(m.movementType);

                            return (
                                <tr key={m.id || i} className="hover:bg-slate-50 transition-colors">
                                    <td className="px-4 py-3 text-xs text-slate-500 whitespace-nowrap">{formatDate(m.createdAt)}</td>
                                    <td className="px-4 py-3">
                                        <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold ${TYPE_COLORS[m.movementType] || 'bg-gray-100'}`}>
                                            {TYPE_ICONS[m.movementType]} {m.movementType}
                                        </span>
                                    </td>
                                    <td className="px-4 py-3 text-xs">
                                        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${m.itemType === 'Product' ? 'bg-blue-50 text-blue-600' : 'bg-orange-50 text-orange-600'}`}>
                                            {m.itemType}
                                        </span>
                                    </td>
                                    <td className="px-4 py-3">
                                        <p className="text-sm font-medium text-slate-700">{m.itemName}</p>
                                        <p className="text-xs text-slate-400 font-mono">{m.itemCode}</p>
                                    </td>
                                    <td className="px-4 py-3 text-sm text-right font-bold">
                                        <span className={isPositive ? 'text-green-600' : 'text-red-600'}>
                                            {isPositive ? '+' : '-'}{m.quantity}
                                        </span>
                                    </td>
                                    <td className="px-4 py-3 text-sm text-right text-slate-500">{m.stockBefore}</td>
                                    <td className="px-4 py-3 text-sm text-right font-medium text-slate-700">{m.stockAfter}</td>
                                    <td className="px-4 py-3 text-xs">
                                        <div className="flex flex-col gap-0.5">
                                            {m.fromLocationCode && <span className="text-slate-500"><span className="text-[10px] uppercase font-bold text-slate-400">From: </span>{m.fromLocationCode}</span>}
                                            {m.toLocationCode   && <span className="text-slate-500"><span className="text-[10px] uppercase font-bold text-slate-400">To: </span>{m.toLocationCode}</span>}
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
            <div className="flex items-center justify-between">
                <p className="text-xs text-slate-500">
                    Page <span className="font-semibold text-slate-700">{filter.pageNumber}</span> of{' '}
                    <span className="font-semibold text-slate-700">{totalPages}</span>
                    {' '}· <span className="font-semibold text-slate-700">{totalRecords}</span> total
                </p>
                <div className="flex items-center gap-2">
                    <button
                        onClick={() => setFilter(p => ({ ...p, pageNumber: 1 }))}
                        disabled={filter.pageNumber <= 1}
                        className="px-2.5 py-1.5 rounded-lg bg-slate-100 text-sm disabled:opacity-40 hover:bg-slate-200">
                        «
                    </button>
                    <button
                        onClick={() => setFilter(p => ({ ...p, pageNumber: Math.max(1, p.pageNumber - 1) }))}
                        disabled={filter.pageNumber <= 1}
                        className="px-3 py-1.5 rounded-lg bg-slate-100 text-sm disabled:opacity-40 hover:bg-slate-200">
                        ← Prev
                    </button>
                    <span className="px-3 py-1.5 bg-blue-600 text-white rounded-lg text-sm font-semibold min-w-[40px] text-center">
                        {filter.pageNumber}
                    </span>
                    <button
                        onClick={() => setFilter(p => ({ ...p, pageNumber: Math.min(totalPages, p.pageNumber + 1) }))}
                        disabled={filter.pageNumber >= totalPages}
                        className="px-3 py-1.5 rounded-lg bg-slate-100 text-sm disabled:opacity-40 hover:bg-slate-200">
                        Next →
                    </button>
                    <button
                        onClick={() => setFilter(p => ({ ...p, pageNumber: totalPages }))}
                        disabled={filter.pageNumber >= totalPages}
                        className="px-2.5 py-1.5 rounded-lg bg-slate-100 text-sm disabled:opacity-40 hover:bg-slate-200">
                        »
                    </button>
                </div>
            </div>
        </div>
    );
};

export default StockMovements;
