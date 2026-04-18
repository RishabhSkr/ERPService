import React, { useEffect, useState } from 'react';
import { createOrder } from '../../api/productionService'; 
import { getProducts } from '../../api/master/product';
import useApi from '../../hooks/useApi';
import { Save, ArrowLeft, Factory } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

/**
 * Create Manual Production Order (not from Sales)
 * 
 * Backend expects CreateProductionOrderDto:
 * { productId, productCode, productName, bomId, quantityPlanned, plannedStartDate, plannedEndDate, priority, notes }
 * 
 * Backend auto-finds active BOM by productId, does BOM explosion, creates PO with MaterialRequirements
 */
const CreateOrder = () => {
    const navigate = useNavigate();
    const { loading, requestHandlerFunction } = useApi();
    
    const [products, setProducts] = useState([]);
    const [formData, setFormData] = useState({
        productId: '',
        quantityPlanned: '',
        plannedStartDate: '',
        plannedEndDate: '',
        priority: 3,
        notes: '',
    });

    // Load products for dropdown
    useEffect(() => {
        const loadProducts = async () => {
            try {
                const res = await getProducts();
                const data = res.data?.data || res.data || [];
                setProducts(Array.isArray(data) ? data : []);
            } catch (err) {
                console.error('Failed to load products', err);
            }
        };
        loadProducts();
    }, []);

    const selectedProduct = products.find(p => p.id === formData.productId || p.productId === formData.productId);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!formData.productId || !formData.quantityPlanned || !formData.plannedStartDate || !formData.plannedEndDate) return;

        const payload = {
            productId: formData.productId,
            productCode: selectedProduct?.productCode || selectedProduct?.code || '',
            productName: selectedProduct?.productName || selectedProduct?.name || '',
            bomId: selectedProduct?.activeBomId || '00000000-0000-0000-0000-000000000000', // Backend will find active BOM
            quantityPlanned: parseFloat(formData.quantityPlanned),
            plannedStartDate: new Date(formData.plannedStartDate).toISOString(),
            plannedEndDate: new Date(formData.plannedEndDate).toISOString(),
            priority: parseInt(formData.priority),
            notes: formData.notes || null,
        };

        const response = await requestHandlerFunction(
            () => createOrder(payload),
            'Production Order Created!'
        );

        if (response.success) {
            navigate('/production-plan');
        }
    };

    const today = new Date().toISOString().split('T')[0];

    return (
        <div className="max-w-3xl mx-auto p-6">
            {/* Header */}
            <div className="flex items-center gap-4 mb-6">
                <button onClick={() => navigate(-1)} className="p-2 hover:bg-gray-100 rounded-full">
                    <ArrowLeft size={24} className="text-gray-600" />
                </button>
                <div>
                    <h1 className="text-2xl font-bold text-slate-800 flex items-center gap-2">
                        <Factory className="text-blue-600" /> Create Manual Production Order
                    </h1>
                    <p className="text-sm text-gray-500">Create a PO manually (not from Sales Order)</p>
                </div>
            </div>

            <div className="bg-white p-6 rounded-xl shadow border border-gray-200">
                <form onSubmit={handleSubmit} className="space-y-5">
                    {/* Product Selection */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Select Product *</label>
                        <select 
                            value={formData.productId}
                            onChange={(e) => setFormData({...formData, productId: e.target.value})}
                            className="w-full border rounded-lg px-3 py-2.5 text-sm bg-white" required>
                            <option value="">-- Select Product --</option>
                            {products.map(p => (
                                <option key={p.id || p.productId} value={p.id || p.productId}>
                                    {p.productName || p.name} ({p.productCode || p.code})
                                </option>
                            ))}
                        </select>
                        {products.length === 0 && !loading && (
                            <p className="text-xs text-amber-600 mt-1">No products found. Add products first.</p>
                        )}
                    </div>

                    {/* Selected Product Info */}
                    {selectedProduct && (
                        <div className="bg-blue-50 rounded-lg p-3 text-sm">
                            <span className="font-bold text-blue-700">{selectedProduct.productName || selectedProduct.name}</span>
                            <span className="text-slate-500 ml-2">({selectedProduct.productCode || selectedProduct.code})</span>
                        </div>
                    )}

                    {/* Quantity */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Quantity to Produce *</label>
                        <input 
                            type="number" min="1"
                            className="w-full border rounded-lg px-3 py-2.5 text-sm"
                            value={formData.quantityPlanned}
                            onChange={(e) => setFormData({...formData, quantityPlanned: e.target.value})}
                            placeholder="Enter quantity"
                            required
                        />
                    </div>

                    {/* Dates */}
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Start Date *</label>
                            <input 
                                type="date"
                                className="w-full border rounded-lg px-3 py-2.5 text-sm"
                                value={formData.plannedStartDate}
                                min={today}
                                onChange={(e) => setFormData({...formData, plannedStartDate: e.target.value})}
                                required
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">End Date *</label>
                            <input 
                                type="date"
                                className="w-full border rounded-lg px-3 py-2.5 text-sm"
                                value={formData.plannedEndDate}
                                min={formData.plannedStartDate || today}
                                onChange={(e) => setFormData({...formData, plannedEndDate: e.target.value})}
                                required
                            />
                        </div>
                    </div>

                    {/* Priority */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Priority</label>
                        <select 
                            value={formData.priority}
                            onChange={(e) => setFormData({...formData, priority: e.target.value})}
                            className="w-full border rounded-lg px-3 py-2.5 text-sm bg-white">
                            <option value="1">1 — Urgent</option>
                            <option value="2">2 — High</option>
                            <option value="3">3 — Normal</option>
                            <option value="4">4 — Low</option>
                            <option value="5">5 — Lowest</option>
                        </select>
                    </div>

                    {/* Notes */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Notes</label>
                        <textarea 
                            className="w-full border rounded-lg px-3 py-2.5 text-sm"
                            value={formData.notes}
                            onChange={(e) => setFormData({...formData, notes: e.target.value})}
                            placeholder="Optional notes"
                            rows={2}
                        />
                    </div>

                    {/* Submit */}
                    <div className="pt-3 border-t">
                        <button 
                            type="submit" 
                            disabled={loading || !formData.productId || !formData.quantityPlanned}
                            className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2.5 rounded-lg flex justify-center items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                        >
                            {loading ? 'Creating...' : <><Save size={18} /> Create Production Order</>}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CreateOrder;