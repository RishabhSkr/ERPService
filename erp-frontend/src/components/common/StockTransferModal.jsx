import React, { useState, useEffect } from 'react';
import { X, ArrowRight } from 'lucide-react';
import { toast } from 'react-hot-toast';
import { transferStock } from '../../api/inventoryService';
import { getStorageLocations } from '../../api/master/storageLocation';
import SearchSelect from './SearchSelect';

const StockTransferModal = ({ isOpen, onClose, item, itemType, onSuccess }) => {
    const [locations, setLocations] = useState([]);
    const [loading, setLoading] = useState(false);
    
    const [formData, setFormData] = useState({
        fromStorageLocationId: '',
        toStorageLocationId: '',
        quantity: '',
        notes: ''
    });

    useEffect(() => {
        if (isOpen) {
            fetchLocations();
            setFormData({
                fromStorageLocationId: '',
                toStorageLocationId: '',
                quantity: '',
                notes: ''
            });
        }
    }, [isOpen]);

    const fetchLocations = async () => {
        try {
            const res = await getStorageLocations();
            const locs = Array.isArray(res) ? res : (res?.value || res?.data?.data || res?.data || []);
            setLocations(locs);
        } catch (err) {
            console.error(err);
        }
    };

    if (!isOpen || !item) return null;

    // Source locations: locations where this item actually has available stock
    const warehouseStock = item.warehouseStock || [];
    // Extract location IDs that have positive available stock
    // Wait, WarehouseStockInfo only returns warehouseId, not storageLocationId in our current DTO.
    // Let's just show all locations that allow this item type for simplicity, or we can use the default.
    // We will let the backend validate if there is enough stock in the source location.
    
    // Actually, we can filter source locations if we want, but for now we'll show all locations 
    // and rely on backend validation, OR we can show the item's warehouse stock locations.
    // In our backend, ProductResponseDto.WarehouseStock is grouped by Warehouse, not Storage Location.
    // So we'll just show all locations.
    const allowedLocations = locations.filter(loc => 
        itemType === 'RawMaterial' ? loc.allowRawMaterials !== false : loc.allowProducts !== false
    );

    const handleSubmit = async (e) => {
        e.preventDefault();
        
        if (!formData.fromStorageLocationId || !formData.toStorageLocationId) {
            return toast.error("Please select both source and destination locations");
        }
        if (formData.fromStorageLocationId === formData.toStorageLocationId) {
            return toast.error("Source and destination cannot be the same");
        }
        if (!formData.quantity || parseFloat(formData.quantity) <= 0) {
            return toast.error("Please enter a valid quantity");
        }

        setLoading(true);
        try {
            await transferStock({
                itemType: itemType,
                itemId: item.id,
                fromStorageLocationId: formData.fromStorageLocationId,
                toStorageLocationId: formData.toStorageLocationId,
                quantity: parseFloat(formData.quantity),
                notes: formData.notes
            });
            toast.success("Stock transferred successfully");
            if (onSuccess) onSuccess();
            onClose();
        } catch (error) {
            toast.error(error.response?.data?.message || "Failed to transfer stock");
        } finally {
            setLoading(false);
        }
    };

    const itemName = itemType === 'RawMaterial' ? item.materialName : item.productName;
    const itemCode = itemType === 'RawMaterial' ? item.materialCode : item.productCode;

    return (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-xl shadow-xl w-full max-w-lg overflow-hidden flex flex-col max-h-[90vh]">
                <div className="p-4 border-b border-slate-200 flex justify-between items-center bg-slate-50">
                    <h3 className="font-semibold text-slate-800 flex items-center gap-2">
                        <ArrowRight size={18} className="text-blue-500" />
                        Transfer Stock
                    </h3>
                    <button onClick={onClose} className="text-slate-400 hover:text-slate-600">
                        <X size={20} />
                    </button>
                </div>
                
                <form onSubmit={handleSubmit} className="p-6 overflow-y-auto">
                    <div className="mb-6 p-4 bg-blue-50 border border-blue-100 rounded-lg">
                        <p className="text-sm text-blue-800 font-medium">Item to Transfer</p>
                        <p className="text-lg font-bold text-blue-900">{itemName} ({itemCode})</p>
                        <p className="text-xs text-blue-600 mt-1">Total Available: {item.availableStock || 0} {item.unitName}</p>
                    </div>

                    <div className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium text-slate-700 mb-1">From Location *</label>
                            <SearchSelect
                                value={formData.fromStorageLocationId}
                                displayValue={(() => { 
                                    const l = locations.find(l => l.id === formData.fromStorageLocationId); 
                                    return l ? `${l.locationCode} (${l.warehouseName})` : ''; 
                                })()}
                                placeholder="Select source location..."
                                items={locations}
                                title="Select Source Location"
                                displayFields={[
                                    { key: 'locationCode', label: 'Code', width: '50%', bold: true },
                                    { key: 'warehouseName', label: 'Warehouse', width: '50%' },
                                ]}
                                searchKeys={['locationCode', 'warehouseName']}
                                valueKey="id"
                                onSelect={(l) => setFormData({...formData, fromStorageLocationId: l.id})}
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-slate-700 mb-1">To Location *</label>
                            <SearchSelect
                                value={formData.toStorageLocationId}
                                displayValue={(() => { 
                                    const l = locations.find(l => l.id === formData.toStorageLocationId); 
                                    return l ? `${l.locationCode} (${l.warehouseName})` : ''; 
                                })()}
                                placeholder="Select destination location..."
                                items={allowedLocations}
                                title="Select Destination Location"
                                displayFields={[
                                    { key: 'locationCode', label: 'Code', width: '50%', bold: true },
                                    { key: 'warehouseName', label: 'Warehouse', width: '50%' },
                                ]}
                                searchKeys={['locationCode', 'warehouseName']}
                                valueKey="id"
                                onSelect={(l) => setFormData({...formData, toStorageLocationId: l.id})}
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-slate-700 mb-1">Quantity *</label>
                            <input 
                                type="number" 
                                step="any"
                                value={formData.quantity}
                                onChange={e => setFormData({...formData, quantity: e.target.value})}
                                className="w-full px-3 py-2 border rounded-lg text-sm"
                                placeholder="0.00"
                                required
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-slate-700 mb-1">Notes</label>
                            <input 
                                type="text" 
                                value={formData.notes}
                                onChange={e => setFormData({...formData, notes: e.target.value})}
                                className="w-full px-3 py-2 border rounded-lg text-sm"
                                placeholder="Optional transfer notes"
                            />
                        </div>
                    </div>
                    
                    <div className="mt-8 flex gap-3 justify-end">
                        <button 
                            type="button" 
                            onClick={onClose}
                            className="px-4 py-2 text-sm font-medium text-slate-600 bg-slate-100 hover:bg-slate-200 rounded-lg transition-colors"
                        >
                            Cancel
                        </button>
                        <button 
                            type="submit" 
                            disabled={loading}
                            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors flex items-center gap-2 disabled:opacity-50"
                        >
                            {loading ? 'Transferring...' : 'Transfer Stock'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default StockTransferModal;
