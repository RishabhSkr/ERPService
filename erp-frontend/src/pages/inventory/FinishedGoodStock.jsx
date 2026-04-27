import React, { useState, useEffect, useCallback } from 'react';
import { Box, Search, RefreshCw, AlertCircle, PackagePlus, X, ArrowUpDown, Eye, ArrowDownUp } from 'lucide-react';
import useApi from '../../hooks/useApi';
import { getFinishedGoodsStock } from '../../api/finishedGoodsInventoryServices';
import { getStorageLocations } from '../../api/master/storageLocation';
import { recordStockMovement } from '../../api/inventoryService';
import toast from 'react-hot-toast';
import SearchSelect from '../../components/common/SearchSelect';
import StockTransferModal from '../../components/common/StockTransferModal';
import LocationStockModal from '../../components/common/LocationStockModal';

/**
 * Finished Goods Inventory — Stock overview + Add Stock per item
 * 
 * ProductListDto: { id, productCode, productName, categoryName, price, currentStock, reservedStock, availableStock, unitName, isActive }
 * RecordStockMovement: POST /stock-movements/record { movementType: 'IN', itemType: 'Product', itemId, warehouseId, quantity, notes }
 */
const FinishedGoodStock = () => {
    const [stockData, setStockData] = useState([]);
    const [searchTerm, setSearchTerm] = useState('');
    const { loading, requestHandlerFunction } = useApi();
    const [stockItem, setStockItem] = useState(null);
    const [storageLocations, setStorageLocations] = useState([]);
    const [stockForm, setStockForm] = useState({ storageLocationId: '', quantity: '', batchNumber: '', adjustmentType: 'IN' });
    const [transferItem, setTransferItem] = useState(null);
    const [showTransferModal, setShowTransferModal] = useState(false);
    const [locationItem, setLocationItem] = useState(null);

    const fetchStock = useCallback(async () => {
        const [response, locResponse] = await Promise.all([
            requestHandlerFunction(() => getFinishedGoodsStock()),
            getStorageLocations()
        ]);
        if (response.success) {
            const pagedData = response.data?.data || response.data || {};
            const items = pagedData?.data || (Array.isArray(pagedData) ? pagedData : []);
            setStockData(Array.isArray(items) ? items : []);
        }
        if (locResponse) {
            const unwrap = (res) => {
                if (Array.isArray(res)) return res;
                const d = res?.data?.data || res?.data || res || [];
                return Array.isArray(d) ? d : (d?.data || []);
            };
            setStorageLocations(unwrap(locResponse));
        }
    }, [requestHandlerFunction]);   



    useEffect(() => { const initLoad = async () => {
            await fetchStock();
        };
        initLoad(); 
    }, [fetchStock]);

    const filteredStock = stockData.filter(item =>
        (item.productName || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
        (item.productCode || '').toLowerCase().includes(searchTerm.toLowerCase())
    );

    const handleAdjustStock = async (e) => {
        e.preventDefault();
        
        const qty = parseFloat(stockForm.quantity);
        // Note: Set exact quantity me 0 bhi valid ho sakta hai agar stock khali karna ho
        if (isNaN(qty) || qty < 0) return toast.error('Enter a valid non-negative quantity');
        if (!stockForm.storageLocationId) return toast.error('Select a storage location');

        try {
            let movementTypeStr = '';
            let actionNote = '';

            // Map UI selection to Backend MovementType
            if (stockForm.adjustmentType === 'IN') {
                movementTypeStr = 'IN';
                actionNote = 'Manual stock addition';
            } else if (stockForm.adjustmentType === 'OUT') {
                movementTypeStr = 'ADJUST_OUT';
                actionNote = 'Manual stock reduction';
            } else if (stockForm.adjustmentType === 'ADJUST') {
                movementTypeStr = 'ADJUST';
                actionNote = 'Exact stock adjustment (Overwrite)';
            }

            await recordStockMovement({
                movementType: movementTypeStr,
                itemType: 'Product',
                itemId: stockItem.id,
                storageLocationId: stockForm.storageLocationId,
                quantity: qty,
                notes: stockForm.batchNumber
                    ? `${actionNote} — Batch: ${stockForm.batchNumber}`
                    : actionNote,
            });

            // Dynamic success message
            const successMsg = 
                stockForm.adjustmentType === 'IN' ? 'Stock added!' : 
                stockForm.adjustmentType === 'OUT' ? 'Stock reduced!' : 
                'Stock exactly updated!';
                
            toast.success(successMsg);
            setStockItem(null);
            fetchStock();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Failed to adjust stock');
        }
    };

    const getStockStatusColor = (qty) => {
        if (qty === 0) return 'bg-red-100 text-red-700 border-red-200';
        if (qty < 10) return 'bg-yellow-100 text-yellow-700 border-yellow-200';
        return 'bg-green-100 text-green-700 border-green-200';
    };

    return (
        <div className="p-6 max-w-7xl mx-auto space-y-6">
            {/* Header */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-slate-800 flex items-center gap-2">
                        <Box className="text-blue-600" /> Finished Goods Inventory
                    </h1>
                    <p className="text-sm text-gray-500 mt-1">Stock levels and inventory management for finished products</p>
                </div>
                <div className="flex gap-2 w-full md:w-auto">
                    <div className="relative flex-1 md:w-64">
                        <Search className="absolute left-3 top-2.5 text-gray-400" size={18} />
                        <input
                            type="text"
                            placeholder="Search by name or code..."
                            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                    <button onClick={fetchStock} disabled={loading}
                        className="p-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 text-gray-600 disabled:opacity-50">
                        <RefreshCw size={20} className={loading ? "animate-spin" : ""} />
                    </button>
                </div>
            </div>

            {/* Summary Cards */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="bg-white rounded-xl border border-slate-200 p-4">
                    <p className="text-xs text-slate-500 uppercase font-semibold">Total Products</p>
                    <p className="text-2xl font-bold text-slate-800 mt-1">{stockData.length}</p>
                </div>
                <div className="bg-white rounded-xl border border-slate-200 p-4">
                    <p className="text-xs text-slate-500 uppercase font-semibold">Active</p>
                    <p className="text-2xl font-bold text-green-600 mt-1">{stockData.filter(i => i.isActive).length}</p>
                </div>
                <div className="bg-white rounded-xl border border-slate-200 p-4">
                    <p className="text-xs text-slate-500 uppercase font-semibold">Low Stock</p>
                    <p className="text-2xl font-bold text-yellow-600 mt-1">{stockData.filter(i => (i.availableStock ?? 0) < 10 && (i.availableStock ?? 0) > 0).length}</p>
                </div>
                <div className="bg-white rounded-xl border border-slate-200 p-4">
                    <p className="text-xs text-slate-500 uppercase font-semibold">Out of Stock</p>
                    <p className="text-2xl font-bold text-red-600 mt-1">{stockData.filter(i => (i.availableStock ?? 0) === 0).length}</p>
                </div>
            </div>

            {/* Table */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
                <table className="w-full text-left border-collapse">
                    <thead className="bg-slate-50 text-slate-600 uppercase text-xs font-bold border-b border-gray-200">
                        <tr>
                            <th className="px-4 py-3">Code</th>
                            <th className="px-4 py-3">Product Name</th>
                            <th className="px-4 py-3">Category</th>
                            <th className="px-4 py-3">Unit</th>
                            <th className="px-4 py-3 text-right">Price</th>
                            <th className="px-4 py-3 text-center">Current Stock</th>
                            <th className="px-4 py-3 text-center">Reserved</th>
                            <th className="px-4 py-3 text-center">Available</th>
                            <th className="px-4 py-3 text-center">Action</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100 text-sm">
                        {loading && stockData.length === 0 ? (
                            <tr><td colSpan="9" className="p-8 text-center text-gray-500">
                                <div className="flex justify-center items-center gap-2">
                                    <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-500"></div> Loading...
                                </div>
                            </td></tr>
                        ) : filteredStock.length === 0 ? (
                            <tr><td colSpan="9" className="p-10 text-center text-gray-400">
                                <Box size={40} className="opacity-20 mx-auto mb-2" />
                                <p>No products found in stock.</p>
                            </td></tr>
                        ) : filteredStock.map(item => (
                            <tr key={item.id} className={`hover:bg-slate-50 transition-colors ${!item.isActive ? 'opacity-50' : ''}`}>
                                <td className="px-4 py-3 font-mono font-semibold text-slate-700">{item.productCode}</td>
                                <td className="px-4 py-3 font-medium text-slate-800">{item.productName}</td>
                                <td className="px-4 py-3 text-slate-600">{item.categoryName || '-'}</td>
                                <td className="px-4 py-3 text-slate-600">{item.unitName || '-'}</td>
                                <td className="px-4 py-3 text-right text-slate-600">₹{item.price?.toLocaleString('en-IN')}</td>
                                <td className="px-4 py-3 text-center font-medium">{item.currentStock ?? 0}</td>
                                <td className="px-4 py-3 text-center text-orange-600">{item.reservedStock ?? 0}</td>
                                <td className="px-4 py-3 text-center">
                                    <span className={`px-3 py-1 rounded-full font-bold text-sm border ${getStockStatusColor(item.availableStock ?? 0)}`}>
                                        {(item.availableStock ?? 0).toLocaleString()}
                                    </span>
                                </td>
                                <td className="px-4 py-3 text-center">
                                    <div className="flex items-center justify-center gap-2">
                                        <button onClick={() => setLocationItem(item)}
                                            className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-slate-50 hover:bg-slate-100 text-slate-600 text-xs font-medium rounded-lg transition-colors"
                                            title="View stock by location">
                                            <Eye size={14} /> Locations
                                        </button>
                                        <button onClick={() => { setStockItem(item); setStockForm({ storageLocationId: item.defaultStorageLocationId || '', quantity: '', batchNumber: '', adjustmentType: 'IN' }); }}
                                            className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-green-50 hover:bg-green-100 text-green-700 text-xs font-medium rounded-lg transition-colors">
                                            <ArrowDownUp size={14} /> Adjust Stock
                                        </button>
                                        <button onClick={() => { setTransferItem(item); setShowTransferModal(true); }}
                                            className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-purple-50 hover:bg-purple-100 text-purple-700 text-xs font-medium rounded-lg transition-colors">
                                            <ArrowUpDown size={14} /> Transfer
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                <div className="bg-gray-50 p-3 border-t border-gray-200 text-xs text-gray-500 flex justify-between items-center">
                    <span>Showing {filteredStock.length} of {stockData.length} products</span>
                    <span className="flex items-center gap-1"><AlertCircle size={12} /> Stock updates automatically on production completion</span>
                </div>
            </div>

            {/* Stock Adjustment Modal */}
            {stockItem && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
                        <div className="flex items-center justify-between p-5 border-b">
                            <h2 className="text-lg font-bold text-slate-800 flex items-center gap-2">
                                <ArrowDownUp className="text-blue-500" size={20} /> Adjust Stock
                            </h2>
                            <button onClick={() => setStockItem(null)} className="p-1.5 rounded-lg hover:bg-slate-100"><X size={20} /></button>
                        </div>
                        <form onSubmit={handleAdjustStock} className="p-5 space-y-4">
                            <div className="bg-blue-50 rounded-lg p-3 text-sm">
                                <span className="font-bold text-blue-700">{stockItem.productCode}</span>
                                <span className="text-slate-500 ml-2">{stockItem.productName}</span>
                                <p className="text-xs text-slate-400 mt-1">Current: {stockItem.currentStock ?? 0} | Available: {stockItem.availableStock ?? 0} {stockItem.unitName}</p>
                            </div>
                            {/* IN/OUT/ADJUST Toggle */}
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-1.5">Adjustment Type *</label>
                                <div className="grid grid-cols-3 gap-2">
                                    <button type="button" onClick={() => setStockForm({...stockForm, adjustmentType: 'IN'})}
                                        className={`py-2.5 rounded-lg text-sm font-semibold border-2 transition-all ${
                                            stockForm.adjustmentType === 'IN'
                                                ? 'bg-green-50 border-green-500 text-green-700'
                                                : 'bg-white border-slate-200 text-slate-500 hover:border-slate-300'
                                        }`}>
                                        ↗ Add (+)
                                    </button>
                                    <button type="button" onClick={() => setStockForm({...stockForm, adjustmentType: 'OUT'})}
                                        className={`py-2.5 rounded-lg text-sm font-semibold border-2 transition-all ${
                                            stockForm.adjustmentType === 'OUT'
                                                ? 'bg-red-50 border-red-500 text-red-700'
                                                : 'bg-white border-slate-200 text-slate-500 hover:border-slate-300'
                                        }`}>
                                        ↙ Deduct (−)
                                    </button>
                                    <button type="button" onClick={() => setStockForm({...stockForm, adjustmentType: 'ADJUST'})}
                                        className={`py-2.5 rounded-lg text-sm font-semibold border-2 transition-all ${
                                            stockForm.adjustmentType === 'ADJUST'
                                                ? 'bg-blue-50 border-blue-500 text-blue-700'
                                                : 'bg-white border-slate-200 text-slate-500 hover:border-slate-300'
                                        }`}>
                                        = Set Exact
                                    </button>
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-1">Quantity *</label>
                                <input type="number" step="any" min="0" value={stockForm.quantity} onChange={(e) => setStockForm({...stockForm, quantity: e.target.value})}
                                    className="w-full px-3 py-2.5 border rounded-lg text-sm" required placeholder="e.g. 500" autoFocus />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-1">Storage Location *</label>
                                <SearchSelect
                                    value={stockForm.storageLocationId}
                                    displayValue={(() => { const l = storageLocations.find(l => l.id === stockForm.storageLocationId); return l ? `${l.locationCode} (${l.warehouseName})` : ''; })()}
                                    placeholder="Select storage location..."
                                    items={storageLocations}
                                    title="Select Location"
                                    displayFields={[
                                        { key: 'locationCode', label: 'Location', width: '40%', bold: true },
                                        { key: 'warehouseName', label: 'Warehouse', width: '60%' },
                                    ]}
                                    searchKeys={['locationCode', 'warehouseName']}
                                    valueKey="id"
                                    onSelect={(l) => setStockForm({...stockForm, storageLocationId: l.id})}
                                    size="sm"
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-1">Notes / Batch Number</label>
                                <input type="text" value={stockForm.batchNumber} onChange={(e) => setStockForm({...stockForm, batchNumber: e.target.value})}
                                    className="w-full px-3 py-2.5 border rounded-lg text-sm" placeholder="Reason or batch number" />
                            </div>
                            <div className="flex justify-end gap-3 pt-2">
                                <button type="button" onClick={() => setStockItem(null)} className="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">Cancel</button>
                                <button type="submit" className={`flex items-center gap-2 px-5 py-2.5 text-white rounded-lg text-sm font-medium ${
                                    stockForm.adjustmentType === 'IN' ? 'bg-green-600 hover:bg-green-700' : 
                                    stockForm.adjustmentType === 'OUT' ? 'bg-red-600 hover:bg-red-700' : 
                                    'bg-blue-600 hover:bg-blue-700'
                                }`}>
                                    {stockForm.adjustmentType === 'IN' ? '↗ Add Stock' : 
                                     stockForm.adjustmentType === 'OUT' ? '↙ Remove Stock' : 
                                     '= Set Exact Stock'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Stock Transfer Modal */}
            <StockTransferModal 
                isOpen={showTransferModal}
                onClose={() => { setShowTransferModal(false); setTransferItem(null); }}
                item={transferItem}
                itemType="Product"
                onSuccess={() => { fetchStock(); }}
            />

            {/* Location Stock Modal */}
            <LocationStockModal
                isOpen={!!locationItem}
                onClose={() => setLocationItem(null)}
                item={locationItem}
                itemType="Product"
            />
        </div>
    );
};

export default FinishedGoodStock;