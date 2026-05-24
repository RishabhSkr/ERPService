import React, { useEffect, useState } from 'react';
import { getProducts, createProduct, updateProduct, deleteProduct, restoreProduct } from '../../api/master/product';
import { getStorageLocations } from '../../api/master/storageLocation';
import { getCategories, getUnits } from '../../api/inventoryService';
import { Package, Plus, Edit2, Trash2, X, Save, RotateCcw, ArrowUpDown, Eye } from 'lucide-react';
import toast from 'react-hot-toast';
import SearchSelect from '../../components/common/SearchSelect';
import StockTransferModal from '../../components/common/StockTransferModal';
import LocationStockModal from '../../components/common/LocationStockModal';

/**
 * Product Master (Finished Goods) — Full CRUD + Add Stock + Restore
 * 
 * CreateProductDto: { productCode, productName, description, categoryId, unitId, price, minStockLevel }
 * UpdateProductDto: { productName?, description?, price?, minStockLevel?, isActive? }
 * ProductListDto: { id, productCode, productName, categoryName, price, currentStock, reservedStock, availableStock, unitName, isActive }
 * AddStock: POST /{id}/add-stock { warehouseId, quantity, batchNumber }
 */
const Products = () => {
    const [products, setProducts] = useState([]);
    const [categories, setCategories] = useState([]);
    const [units, setUnits] = useState([]);
    const [storageLocations, setStorageLocations] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showForm, setShowForm] = useState(false);
    const [editItem, setEditItem] = useState(null);
    const [formData, setFormData] = useState(emptyForm());
    // Transfer Stock modal
    const [locationItem, setLocationItem] = useState(null);

    function emptyForm() {
        return { productCode: '', productName: '', description: '', categoryId: '', unitId: '', price: '', minStockLevel: 0, defaultStorageLocationId: '' };
    }

    const loadData = async () => {
        setLoading(true);
        try {
            const [pRes, catRes, unitRes, locRes] = await Promise.all([getProducts(), getCategories(), getUnits(), getStorageLocations()]);
            // getProducts returns: { success, data: { data: [...items], pageNumber, pageSize, totalRecords } }
            const unwrapPaged = (res) => {
                const d = res?.data || res;
                return d?.data || (Array.isArray(d) ? d : []);
            };
            const unwrapList = (res) => {
                if (Array.isArray(res)) return res;
                const d = res?.data?.data || res?.data || res || [];
                return Array.isArray(d) ? d : (d?.data || []);
            };
            setProducts(unwrapPaged(pRes));
            setCategories(unwrapList(catRes));
            setUnits(unwrapList(unitRes));
            setStorageLocations(unwrapList(locRes).filter(loc => loc.allowProducts !== false));
        } catch (error) {
            console.error("Failed to load data", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { loadData(); }, []);

    const openCreate = () => { setEditItem(null); setFormData(emptyForm()); setShowForm(true); };
    const openEdit = (item) => {
        setEditItem(item);
        setFormData({
            productCode: item.productCode || '',
            productName: item.productName || '',
            description: item.description || '',
            categoryId: item.categoryId || '',
            unitId: item.unitId || '',
            price: item.price || '',
            minStockLevel: item.minStockLevel || 0,
            defaultStorageLocationId: item.defaultStorageLocationId || '',
        });
        setShowForm(true);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            if (editItem) {
                await updateProduct(editItem.id, {
                    productName: formData.productName,
                    description: formData.description || null,
                    price: parseFloat(formData.price) || 0,
                    minStockLevel: parseFloat(formData.minStockLevel) || 0,
                    defaultStorageLocationId: formData.defaultStorageLocationId || null,
                });
                toast.success('Product updated!');
            } else {
                await createProduct({
                    productCode: formData.productCode,
                    productName: formData.productName,
                    description: formData.description || null,
                    categoryId: formData.categoryId,
                    unitId: formData.unitId,
                    price: parseFloat(formData.price) || 0,
                    minStockLevel: parseFloat(formData.minStockLevel) || 0,
                    defaultStorageLocationId: formData.defaultStorageLocationId || null,
                });
                toast.success('Product created!');
            }
            setShowForm(false);
            loadData();
        } catch (error) {
            toast.error(error.response?.data?.message || 'Failed');
        }
    };

    const handleDelete = async (item) => {
        if (!confirm(`Deactivate "${item.productName}"?`)) return;
        try {
            await deleteProduct(item.id);
            toast.success('Deactivated!');
            loadData();
        } catch (err) { toast.error('Failed'); }
    };

    const handleRestore = async (item) => {
        try {
            await restoreProduct(item.id);
            toast.success('Restored!');
            loadData();
        } catch (err) { toast.error('Failed'); }
    };



    return (
        <div className="p-6">
            {/* Header */}
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <Package className="text-blue-500" size={28} />
                    <h1 className="text-2xl font-bold text-slate-800">Products (Finished Goods)</h1>
                    <span className="text-sm text-slate-400">({products.length})</span>
                </div>
                <button onClick={openCreate} className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium">
                    <Plus size={16} /> Add Product
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
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Price</th>
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Stock</th>
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Reserved</th>
                            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Available</th>
                            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Status</th>
                            <th className="text-center px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                        {loading ? (
                            <tr><td colSpan="10" className="text-center py-10 text-slate-400">Loading...</td></tr>
                        ) : products.length === 0 ? (
                            <tr><td colSpan="10" className="text-center py-10 text-slate-400">No products found</td></tr>
                        ) : products.map(p => (
                            <tr key={p.id} className={`hover:bg-slate-50 transition-colors ${!p.isActive ? 'opacity-50' : ''}`}>
                                <td className="px-4 py-3 text-sm font-mono font-semibold text-slate-700">{p.productCode}</td>
                                <td className="px-4 py-3 text-sm font-medium text-slate-700">{p.productName}</td>
                                <td className="px-4 py-3 text-sm text-slate-600">{p.categoryName || '-'}</td>
                                <td className="px-4 py-3 text-sm text-slate-600">{p.unitName || '-'}</td>
                                <td className="px-4 py-3 text-sm text-right text-slate-600">₹{p.price?.toLocaleString('en-IN')}</td>
                                <td className="px-4 py-3 text-sm text-right font-medium">{p.currentStock ?? 0}</td>
                                <td className="px-4 py-3 text-sm text-right text-orange-600">{p.reservedStock ?? 0}</td>
                                <td className="px-4 py-3 text-sm text-right font-bold text-green-600">{p.availableStock ?? 0}</td>
                                <td className="px-4 py-3 text-sm">
                                    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${p.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                                        {p.isActive ? 'Active' : 'Inactive'}
                                    </span>
                                </td>
                                <td className="px-4 py-3 text-center">
                                    <div className="flex items-center justify-center gap-1">
                                        <button onClick={() => setLocationItem(p)}
                                            className="p-1.5 rounded hover:bg-slate-100 text-slate-500 hover:text-slate-700" title="View Locations">
                                            <Eye size={15} />
                                        </button>
                                        <button onClick={() => openEdit(p)} className="p-1.5 rounded hover:bg-blue-50 text-slate-500 hover:text-blue-600" title="Edit">
                                            <Edit2 size={15} />
                                        </button>
                                        {p.isActive ? (
                                            <button onClick={() => handleDelete(p)} className="p-1.5 rounded hover:bg-red-50 text-slate-500 hover:text-red-600" title="Deactivate">
                                                <Trash2 size={15} />
                                            </button>
                                        ) : (
                                            <button onClick={() => handleRestore(p)} className="p-1.5 rounded hover:bg-green-50 text-slate-500 hover:text-green-600" title="Restore">
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
                            <h2 className="text-lg font-bold text-slate-800">{editItem ? 'Edit' : 'Add'} Product</h2>
                            <button onClick={() => setShowForm(false)} className="p-1.5 rounded-lg hover:bg-slate-100"><X size={20} /></button>
                        </div>
                        <form onSubmit={handleSubmit} className="p-5 space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Product Code *</label>
                                    <input type="text" value={formData.productCode} onChange={(e) => setFormData({...formData, productCode: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" required disabled={!!editItem} placeholder="PROD-001" />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Product Name *</label>
                                    <input type="text" value={formData.productName} onChange={(e) => setFormData({...formData, productName: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" required placeholder="Battery Type A" />
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-1">Description</label>
                                <input type="text" value={formData.description} onChange={(e) => setFormData({...formData, description: e.target.value})}
                                    className="w-full px-3 py-2.5 border rounded-lg text-sm" placeholder="High capacity battery" />
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
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Price (₹) *</label>
                                    <input type="number" step="any" value={formData.price} onChange={(e) => setFormData({...formData, price: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" required placeholder="50.00" />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Min Stock Level</label>
                                    <input type="number" step="any" value={formData.minStockLevel} onChange={(e) => setFormData({...formData, minStockLevel: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm" placeholder="100" />
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
                itemType="Product"
            />
        </div>
    );
};

export default Products;
