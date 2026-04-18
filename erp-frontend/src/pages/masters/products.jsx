import React, { useEffect, useState } from 'react';
import { getProducts, createProduct, updateProduct, deleteProduct, restoreProduct, addProductStock } from '../../api/master/product';
import { getCategories, getUnits } from '../../api/inventoryService';
import { Package, Plus, Edit2, Trash2, X, Save, RotateCcw, PackagePlus } from 'lucide-react';
import toast from 'react-hot-toast';

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
    const [loading, setLoading] = useState(true);
    const [showForm, setShowForm] = useState(false);
    const [editItem, setEditItem] = useState(null);
    const [formData, setFormData] = useState(emptyForm());
    // Add Stock modal
    const [stockItem, setStockItem] = useState(null);
    const [stockForm, setStockForm] = useState({ warehouseId: 'b1111111-1111-1111-1111-111111111111', quantity: '', batchNumber: '' });

    function emptyForm() {
        return { productCode: '', productName: '', description: '', categoryId: '', unitId: '', price: '', minStockLevel: 0 };
    }

    const loadData = async () => {
        setLoading(true);
        try {
            const [pRes, catRes, unitRes] = await Promise.all([getProducts(), getCategories(), getUnits()]);
            // getProducts returns: { success, data: { data: [...items], pageNumber, pageSize, totalRecords } }
            const unwrapPaged = (res) => {
                const d = res?.data || res;
                return d?.data || (Array.isArray(d) ? d : []);
            };
            const unwrapList = (res) => {
                const d = res.data?.data || res.data || [];
                return Array.isArray(d) ? d : (d?.data || []);
            };
            setProducts(unwrapPaged(pRes));
            setCategories(unwrapList(catRes));
            setUnits(unwrapList(unitRes));
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

    const handleAddStock = async (e) => {
        e.preventDefault();
        if (!stockForm.quantity || parseFloat(stockForm.quantity) <= 0) return toast.error('Enter valid quantity');
        try {
            await addProductStock(stockItem.id, {
                warehouseId: stockForm.warehouseId,
                quantity: parseFloat(stockForm.quantity),
                batchNumber: stockForm.batchNumber || null,
            });
            toast.success('Stock added!');
            setStockItem(null);
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
                                        <button onClick={() => { setStockItem(p); setStockForm({ warehouseId: 'b1111111-1111-1111-1111-111111111111', quantity: '', batchNumber: '' }); }}
                                            className="p-1.5 rounded hover:bg-green-50 text-slate-500 hover:text-green-600" title="Add Stock">
                                            <PackagePlus size={15} />
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
                                    <select value={formData.categoryId} onChange={(e) => setFormData({...formData, categoryId: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm bg-white" required={!editItem}>
                                        <option value="">Select...</option>
                                        {categories.map(c => (
                                            <option key={c.id} value={c.id}>{c.categoryName} ({c.categoryCode})</option>
                                        ))}
                                    </select>
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 mb-1">Unit *</label>
                                    <select value={formData.unitId} onChange={(e) => setFormData({...formData, unitId: e.target.value})}
                                        className="w-full px-3 py-2.5 border rounded-lg text-sm bg-white" required={!editItem}>
                                        <option value="">Select...</option>
                                        {units.map(u => (
                                            <option key={u.id} value={u.id}>{u.unitName} ({u.unitCode})</option>
                                        ))}
                                    </select>
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

            {/* Add Stock Modal */}
            {stockItem && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
                        <div className="flex items-center justify-between p-5 border-b">
                            <h2 className="text-lg font-bold text-slate-800 flex items-center gap-2">
                                <PackagePlus className="text-green-500" size={20} /> Add Stock
                            </h2>
                            <button onClick={() => setStockItem(null)} className="p-1.5 rounded-lg hover:bg-slate-100"><X size={20} /></button>
                        </div>
                        <form onSubmit={handleAddStock} className="p-5 space-y-4">
                            <div className="bg-blue-50 rounded-lg p-3 text-sm">
                                <span className="font-bold text-blue-700">{stockItem.productCode}</span>
                                <span className="text-slate-500 ml-2">{stockItem.productName}</span>
                                <p className="text-xs text-slate-400 mt-1">Current Stock: {stockItem.currentStock ?? 0} {stockItem.unitName}</p>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-1">Quantity *</label>
                                <input type="number" step="any" value={stockForm.quantity} onChange={(e) => setStockForm({...stockForm, quantity: e.target.value})}
                                    className="w-full px-3 py-2.5 border rounded-lg text-sm" required placeholder="500" />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-slate-700 mb-1">Batch Number</label>
                                <input type="text" value={stockForm.batchNumber} onChange={(e) => setStockForm({...stockForm, batchNumber: e.target.value})}
                                    className="w-full px-3 py-2.5 border rounded-lg text-sm" placeholder="BATCH-2024-001" />
                            </div>
                            <div className="flex justify-end gap-3 pt-2">
                                <button type="button" onClick={() => setStockItem(null)} className="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">Cancel</button>
                                <button type="submit" className="flex items-center gap-2 px-5 py-2.5 bg-green-600 hover:bg-green-700 text-white rounded-lg text-sm font-medium">
                                    <PackagePlus size={16} /> Add Stock
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Products;
