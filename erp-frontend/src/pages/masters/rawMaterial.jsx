import React, { useState, useEffect } from 'react';
import { useRawMaterials } from '../../hooks/useRawMaterials';
import { getStorageLocations } from '../../api/master/storageLocation';
import { getCategories, getUnits } from '../../api/inventoryService';
import { Plus, Trash2, Edit2, X, Save, RefreshCw, Package, RotateCcw, PackagePlus, ArrowUpDown, Eye } from 'lucide-react';
import toast from 'react-hot-toast';
import SearchSelect from '../../components/common/SearchSelect';
import LocationStockModal from '../../components/common/LocationStockModal';

/**
 * Raw Material Management — Full CRUD + Add Stock + Restore
 * 
 * CreateRawMaterialDto: { materialCode, materialName, description, categoryId, unitId, cost, minStockLevel, supplier }
 * UpdateRawMaterialDto: { materialName?, description?, cost?, minStockLevel?, supplier?, isActive? }
 * RawMaterialListDto: { id, materialCode, materialName, categoryName, cost, currentStock, reservedStock, availableStock, unitName, supplier, isActive }
 * AddStock: POST /{id}/add-stock { warehouseId, quantity, batchNumber }
 */
const RawMaterial = () => {
    const { materials, loading, addMaterial, updateRM, deleteRM, restoreRM, addRawMaterialStock } = useRawMaterials();
    const [categories, setCategories] = useState([]);
    const [units, setUnits] = useState([]);
    const [storageLocations, setStorageLocations] = useState([]);
    const [showForm, setShowForm] = useState(false);
    const [editItem, setEditItem] = useState(null);
    const [formData, setFormData] = useState(emptyForm());
    //  Stock modal
    const [locationItem, setLocationItem] = useState(null);
    const [stockForm, setStockForm] = useState({ storageLocationId: '', quantity: '', batchNumber: '' });

    function emptyForm() {
        return { materialCode: '', materialName: '', description: '', categoryId: '', unitId: '', cost: '', minStockLevel: 0, supplier: '', defaultStorageLocationId: '' };
    }

    useEffect(() => {
        const loadLookups = async () => {
            try {
                const [catRes, unitRes, locRes] = await Promise.all([getCategories(), getUnits(), getStorageLocations()]);
                const unwrap = (res) => {
                    if (Array.isArray(res)) return res;
                    const d = res?.data?.data || res?.data || res || [];
                    return Array.isArray(d) ? d : (d?.data || []);
                };
                setCategories(unwrap(catRes));
                setUnits(unwrap(unitRes));
                setStorageLocations(unwrap(locRes).filter(loc => loc.allowRawMaterials !== false));
            } catch (err) {
                console.error('Failed to load lookups', err);
            }
        };
        loadLookups();
    }, []);

    const openCreate = () => { setEditItem(null); setFormData(emptyForm()); setShowForm(true); };
    const openEdit = (item) => {
        setEditItem(item);
        setFormData({
            materialCode: item.materialCode || '',
            materialName: item.materialName || '',
            description: item.description || '',
            categoryId: item.categoryId || '',
            unitId: item.unitId || '',
            cost: item.cost || '',
            minStockLevel: item.minStockLevel || 0,
            supplier: item.supplier || '',
            defaultStorageLocationId: item.defaultStorageLocationId || '',
        });
        setShowForm(true);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        let success;
        if (editItem) {
            success = await updateRM(editItem.id, {
                materialName: formData.materialName,
                description: formData.description || null,
                cost: parseFloat(formData.cost) || 0,
                minStockLevel: parseFloat(formData.minStockLevel) || 0,
                supplier: formData.supplier || null,
                defaultStorageLocationId: formData.defaultStorageLocationId || null,
            });
        } else {
            success = await addMaterial({
                materialCode: formData.materialCode,
                materialName: formData.materialName,
                description: formData.description || null,
                categoryId: formData.categoryId,
                unitId: formData.unitId,
                cost: parseFloat(formData.cost) || 0,
                minStockLevel: parseFloat(formData.minStockLevel) || 0,
                supplier: formData.supplier || null,
                defaultStorageLocationId: formData.defaultStorageLocationId || null,
            });
        }
        if (success) setShowForm(false);
    };

    const handleDelete = async (item) => {
        if (!confirm(`Deactivate "${item.materialName}"?`)) return;
        await deleteRM(item.id);
    };

    const handleRestore = async (item) => {
        await restoreRM(item.id);
    };

    const handleAddStock = async (e) => {
        e.preventDefault();
        if (!stockForm.quantity || parseFloat(stockForm.quantity) <= 0) return toast.error('Enter valid quantity');
        const success = await addRawMaterialStock(stockItem.id, {
            storageLocationId: stockForm.storageLocationId,
            quantity: parseFloat(stockForm.quantity),
            batchNumber: stockForm.batchNumber || null,
        });
        if (success) setStockItem(null);
    };

    return (
        <div className="p-6">
            {/* Header */}
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <Package className="text-orange-500" size={28} />
                    <h1 className="text-2xl font-bold text-slate-800">Raw Materials</h1>
                    <span className="text-sm text-slate-400">({materials.length})</span>
                </div>
                <button onClick={openCreate} className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium">
                    <Plus size={16} /> Add Material
                </button>
            </div>

            {/* Table */}
            <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
                <table className="w-full">
                    <thead className="bg-slate-50 border-b border-slate-200">
                        <tr>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Code</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Name</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Category</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Unit</th>
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Cost</th>
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Stock</th>
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Reserved</th>
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Available</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Supplier</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Status</th>
                            <th className="text-center px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                        {loading ? (
                            <tr><td colSpan="11" className="text-center py-10 text-slate-400">Loading...</td></tr>
                        ) : materials.length === 0 ? (
                            <tr><td colSpan="11" className="text-center py-10 text-slate-400">No raw materials found</td></tr>
                        ) : materials.map(rm => (
                            <tr key={rm.id} className={`hover:bg-slate-50 transition-colors ${!rm.isActive ? 'opacity-50' : ''}`}>
                                <td className="px-4 py-3 text-sm font-mono font-semibold text-slate-700">{rm.materialCode}</td>
                                <td className="px-4 py-3 text-sm font-medium text-slate-700">{rm.materialName}</td>
                                <td className="px-4 py-3 text-sm text-slate-600">{rm.categoryName || '-'}</td>
                                <td className="px-4 py-3 text-sm text-slate-600">{rm.unitName || '-'}</td>
                                <td className="px-4 py-3 text-sm text-right text-slate-600">₹{rm.cost?.toLocaleString('en-IN')}</td>
                                <td className="px-4 py-3 text-sm text-right font-medium">{rm.currentStock ?? 0}</td>
                                <td className="px-4 py-3 text-sm text-right text-orange-600">{rm.reservedStock ?? 0}</td>
                                <td className="px-4 py-3 text-sm text-right font-bold text-green-600">{rm.availableStock ?? 0}</td>
                                <td className="px-4 py-3 text-sm text-slate-500">{rm.supplier || '-'}</td>
                                <td className="px-4 py-3 text-sm">
                                    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${rm.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                                        {rm.isActive ? 'Active' : 'Inactive'}
                                    </span>
                                </td>
                                <td className="px-4 py-3 text-center">
                                    <div className="flex items-center justify-center gap-1">
                                        <button onClick={() => setLocationItem(rm)}
                                            className="p-1.5 rounded hover:bg-slate-100 text-slate-500 hover:text-slate-700" title="View Locations">
                                            <Eye size={15} />
                                        </button>
                                        <button onClick={() => openEdit(rm)} className="p-1.5 rounded hover:bg-blue-50 text-slate-500 hover:text-blue-600" title="Edit">
                                            <Edit2 size={15} />
                                        </button>
                                        {rm.isActive ? (
                                            <button onClick={() => handleDelete(rm)} className="p-1.5 rounded hover:bg-red-50 text-slate-500 hover:text-red-600" title="Deactivate">
                                                <Trash2 size={15} />
                                            </button>
                                        ) : (
                                            <button onClick={() => handleRestore(rm)} className="p-1.5 rounded hover:bg-green-50 text-slate-500 hover:text-green-600" title="Restore">
                                                <RotateCcw size={15} />
                                            </button>
                                        )}
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {/* Create/Edit Modal */}
            {showForm && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg">
                        <div className="flex items-center justify-between p-5 border-b">
                            <h2 className="text-lg font-bold text-slate-800">{editItem ? 'Edit' : 'Add'} Raw Material</h2>
                            <button onClick={() => setShowForm(false)} className="p-1.5 rounded-lg hover:bg-slate-100"><X size={20} /></button>
                        </div>
                        <form onSubmit={handleSubmit} className="p-5 space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Material Code *</label>
                                    <input type="text" value={formData.materialCode} onChange={(e) => setFormData({...formData, materialCode: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" required disabled={!!editItem} placeholder="RM-LEAD-001" />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Material Name *</label>
                                    <input type="text" value={formData.materialName} onChange={(e) => setFormData({...formData, materialName: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" required placeholder="Lead Acid" />
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-1">Description</label>
                                <input type="text" value={formData.description} onChange={(e) => setFormData({...formData, description: e.target.value})}
                                    className="w-full px-3 py-2.5 border rounded-lg text-sm" />
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Category *</label>
                                    <SearchSelect
                                        value={formData.categoryId}
                                        displayValue={(() => { const c = categories.find(c => c.id === formData.categoryId); return c ? `${c.categoryName} (${c.categoryCode})` : ''; })()}
                                        placeholder="Search category..."
                                        items={categories}
                                        title="Select Category"
                                        displayFields={[
                                            { key: 'categoryCode', label: 'Code', width: '30%', bold: true },
                                            { key: 'categoryName', label: 'Name', width: '70%' },
                                        ]}
                                        searchKeys={['categoryCode', 'categoryName']}
                                        valueKey="id"
                                        onSelect={(c) => setFormData({...formData, categoryId: c.id})}
                                        size="sm"
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Unit *</label>
                                    <SearchSelect
                                        value={formData.unitId}
                                        displayValue={(() => { const u = units.find(u => u.id === formData.unitId); return u ? `${u.unitName} (${u.unitCode})` : ''; })()}
                                        placeholder="Search unit..."
                                        items={units}
                                        title="Select Unit"
                                        displayFields={[
                                            { key: 'unitCode', label: 'Code', width: '30%', bold: true },
                                            { key: 'unitName', label: 'Name', width: '70%' },
                                        ]}
                                        searchKeys={['unitCode', 'unitName']}
                                        valueKey="id"
                                        onSelect={(u) => setFormData({...formData, unitId: u.id})}
                                        size="sm"
                                    />
                                </div>
                            </div>
                            <div className="grid grid-cols-3 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Cost (₹) *</label>
                                    <input type="number" step="any" value={formData.cost} onChange={(e) => setFormData({...formData, cost: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" required placeholder="25.50" />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Min Stock</label>
                                    <input type="number" step="any" value={formData.minStockLevel} onChange={(e) => setFormData({...formData, minStockLevel: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" placeholder="500" />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Supplier</label>
                                    <input type="text" value={formData.supplier} onChange={(e) => setFormData({...formData, supplier: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" placeholder="Chemical Corp" />
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-1">Default Storage Location</label>
                                <SearchSelect
                                    value={formData.defaultStorageLocationId}
                                    displayValue={(() => { const l = storageLocations.find(l => l.id === formData.defaultStorageLocationId); return l ? `${l.locationCode} (${l.warehouseName})` : ''; })()}
                                    placeholder="Search location..."
                                    items={storageLocations}
                                    title="Select Default Location"
                                    displayFields={[
                                        { key: 'locationCode', label: 'Location Code', width: '40%', bold: true },
                                        { key: 'warehouseName', label: 'Warehouse', width: '60%' },
                                    ]}
                                    searchKeys={['locationCode', 'warehouseName']}
                                    valueKey="id"
                                    onSelect={(l) => setFormData({...formData, defaultStorageLocationId: l.id})}
                                    size="sm"
                                />
                            </div>
                            <div className="flex justify-end gap-3 pt-2">
                                <button type="button" onClick={() => setShowForm(false)} className="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">Cancel</button>
                                <button type="submit" className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium">
                                    <Save size={16} /> {editItem ? 'Update' : 'Create'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Location Stock Modal */}
            <LocationStockModal
                isOpen={!!locationItem}
                onClose={() => setLocationItem(null)}
                item={locationItem}
                itemType="RawMaterial"
            />
        </div>
    );
};
export default RawMaterial;