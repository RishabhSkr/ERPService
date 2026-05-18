import React, { useEffect, useState } from 'react';
import { createOrder, getWorkCenters } from '../../api/productionService'; 
import { getProducts } from '../../api/master/product';
import { useBom } from '../../hooks/useBom';
import useApi from '../../hooks/useApi';
import { Save, ArrowLeft, Factory, AlertCircle, Layers, CheckCircle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import SearchSelect from '../../components/common/SearchSelect';

/**
 * Create Manual Production Order (not from Sales)
 * 
 * Backend expects CreateProductionOrderDto:
 * { productId, productCode, productName, bomId, quantityPlanned, plannedStartDate, plannedEndDate, priority, notes, workCenterId, workCenterName }
 * 
 * BOM is REQUIRED — user selects product → BOM auto-fetched → if no BOM exists, show error
 */
const CreateOrder = () => {
    const navigate = useNavigate();
    const { loading, requestHandlerFunction } = useApi();
    const { getBOMByProductId } = useBom();
    
    const [products, setProducts] = useState([]);
    const [workCenters, setWorkCenters] = useState([]);
    const [bomInfo, setBomInfo] = useState(null);   // Active BOM for selected product
    const [bomError, setBomError] = useState('');     // Error if no BOM
    const [bomLoading, setBomLoading] = useState(false);

    const [formData, setFormData] = useState({
        productId: '',
        quantityPlanned: '',
        plannedStartDate: '',
        plannedEndDate: '',
        priority: 3,
        notes: '',
        workCenterId: '',
    });

    // Load master data
    useEffect(() => {
        const loadData = async () => {
            try {
                const [prodRes, wcRes] = await Promise.all([
                    getProducts(),
                    getWorkCenters()
                ]);
                const prodData = prodRes.data?.data || prodRes.data || [];
                const wcData = wcRes.data?.data || wcRes.data || [];
                setProducts(Array.isArray(prodData) ? prodData : []);
                setWorkCenters(Array.isArray(wcData) ? wcData : []);
            } catch (err) {
                console.error('Failed to load data', err);
            }
        };
        loadData();
    }, []);

    // When product changes → auto-fetch BOM
    const handleProductSelect = async (product) => {
        const productId = product.id || product.productId;
        setFormData({ ...formData, productId });
        setBomInfo(null);
        setBomError('');
        setBomLoading(true);

        try {
            const bom = await getBOMByProductId(productId);
            if (bom && bom.bomId) {
                setBomInfo(bom);
                setBomError('');
            } else {
                setBomInfo(null);
                setBomError('No active BOM found for this product. Create a BOM first.');
            }
        } catch (err) {
            setBomInfo(null);
            setBomError('BOM doesn\'t exist for this product. Please create BOM first.');
        } finally {
            setBomLoading(false);
        }
    };

    const selectedProduct = products.find(p => (p.id || p.productId) === formData.productId);
    const selectedWC = workCenters.find(w => w.workCenterId === formData.workCenterId);

    const canSubmit = formData.productId && bomInfo && formData.quantityPlanned > 0 
        && formData.plannedStartDate && formData.plannedEndDate && !bomLoading;

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!canSubmit) return;

        const payload = {
            productId: formData.productId,
            productCode: selectedProduct?.productCode || selectedProduct?.code || '',
            productName: selectedProduct?.productName || selectedProduct?.name || '',
            bomId: bomInfo.bomId,
            quantityPlanned: parseFloat(formData.quantityPlanned),
            plannedStartDate: new Date(formData.plannedStartDate).toISOString(),
            plannedEndDate: new Date(formData.plannedEndDate).toISOString(),
            priority: parseInt(formData.priority),
            notes: formData.notes || null,
            workCenterId: formData.workCenterId || null,
            workCenterName: selectedWC?.centerName || null,
        };

        const response = await requestHandlerFunction(
            () => createOrder(payload),
            'Production Order Created!'
        );

        if (response.success) {
            navigate('/app/production-plan');
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
                    
                    {/* Product Selection — SearchSelect */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Select Product *</label>
                        <SearchSelect
                            value={formData.productId}
                            displayValue={selectedProduct ? `${selectedProduct.productName || selectedProduct.name} (${selectedProduct.productCode || selectedProduct.code})` : ''}
                            placeholder="Search and select product..."
                            items={products}
                            title="Select Product"
                            displayFields={[
                                { key: 'productCode', label: 'Code', width: '30%', bold: true },
                                { key: 'productName', label: 'Name', width: '50%' },
                                { key: 'price', label: 'Price', width: '20%' },
                            ]}
                            searchKeys={['productCode', 'productName', 'code', 'name']}
                            valueKey="id"
                            onSelect={handleProductSelect}
                        />
                        {products.length === 0 && !loading && (
                            <p className="text-xs text-amber-600 mt-1">No products found. Add products first.</p>
                        )}
                    </div>

                    {/* BOM Info / Error */}
                    {bomLoading && (
                        <div className="flex items-center gap-2 text-sm text-blue-600 bg-blue-50 p-3 rounded-lg">
                            <div className="animate-spin h-4 w-4 border-2 border-blue-500 border-t-transparent rounded-full" />
                            Looking up BOM...
                        </div>
                    )}

                    {bomError && !bomLoading && (
                        <div className="flex items-center gap-2 text-sm text-red-600 bg-red-50 p-3 rounded-lg border border-red-200">
                            <AlertCircle size={16} className="flex-shrink-0" />
                            <span>{bomError}</span>
                        </div>
                    )}

                    {bomInfo && !bomLoading && (
                        <div className="bg-green-50 border border-green-200 rounded-xl p-4">
                            <div className="flex items-center gap-2 mb-3">
                                <CheckCircle size={16} className="text-green-600" />
                                <span className="text-sm font-bold text-green-700">BOM Found</span>
                                <span className="ml-auto text-xs font-mono text-green-600 bg-green-100 px-2 py-0.5 rounded">
                                    {bomInfo.bomCode} · v{bomInfo.version}
                                </span>
                            </div>
                            {/* BOM Materials preview */}
                            {bomInfo.lines && bomInfo.lines.length > 0 && (
                                <div className="border border-green-200 rounded-lg overflow-hidden">
                                    <div className="bg-green-100/50 px-3 py-1.5 text-xs font-semibold text-green-700 uppercase flex items-center gap-1">
                                        <Layers size={10} /> Recipe: {bomInfo.lines.length} materials
                                    </div>
                                    <table className="w-full text-xs">
                                        <thead>
                                            <tr className="bg-white text-slate-400 uppercase">
                                                <th className="text-left px-3 py-1">Material</th>
                                                <th className="text-left px-3 py-1">Process</th>
                                                <th className="text-right px-3 py-1">Qty</th>
                                                <th className="text-right px-3 py-1">Unit</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {bomInfo.lines.map((l, i) => (
                                                <tr key={i} className="border-t border-green-100">
                                                    <td className="px-3 py-1.5 text-slate-700">{l.materialName} <span className="text-slate-400">({l.materialCode})</span></td>
                                                    <td className="px-3 py-1.5 text-slate-500">{l.processName ? `${l.processCode} — ${l.processName}` : '-'}</td>
                                                    <td className="px-3 py-1.5 text-right font-mono font-bold text-slate-700">{l.quantity}</td>
                                                    <td className="px-3 py-1.5 text-right text-slate-400">{l.unit}</td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                </div>
                            )}
                        </div>
                    )}

                    {/* Work Center Selection */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Work Center</label>
                        <SearchSelect
                            value={formData.workCenterId}
                            displayValue={selectedWC ? `${selectedWC.centerName} (${selectedWC.centerCode})` : ''}
                            placeholder="Select work center..."
                            items={workCenters}
                            title="Select Work Center"
                            displayFields={[
                                { key: 'centerCode', label: 'Code', width: '30%', bold: true },
                                { key: 'centerName', label: 'Name', width: '50%' },
                                { key: 'location', label: 'Location', width: '20%' },
                            ]}
                            searchKeys={['centerCode', 'centerName', 'location']}
                            valueKey="workCenterId"
                            onSelect={(wc) => setFormData({...formData, workCenterId: wc.workCenterId})}
                            onClear={() => setFormData({...formData, workCenterId: ''})}
                        />
                    </div>

                    {/* Quantity */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Quantity to Produce *</label>
                        <input 
                            type="number" min="1"
                            className="w-full border rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
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
                                className="w-full border rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
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
                                className="w-full border rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
                                value={formData.plannedEndDate}
                                min={formData.plannedStartDate || today}
                                onChange={(e) => setFormData({...formData, plannedEndDate: e.target.value})}
                                required
                            />
                        </div>
                    </div>

                    {/* Priority — kept as simple select (5 static options) */}
                    <div>
                        <label className="block text-xs font-semibold text-slate-500 uppercase mb-1">Priority</label>
                        <select 
                            value={formData.priority}
                            onChange={(e) => setFormData({...formData, priority: e.target.value})}
                            className="w-full border rounded-lg px-3 py-2.5 text-sm bg-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none">
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
                            className="w-full border rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
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
                            disabled={loading || !canSubmit}
                            className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2.5 rounded-lg flex justify-center items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                        >
                            {loading ? 'Creating...' : <><Save size={18} /> Create Production Order</>}
                        </button>
                        {!bomInfo && formData.productId && !bomLoading && (
                            <p className="text-xs text-red-500 text-center mt-2">Cannot create PO without an active BOM</p>
                        )}
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CreateOrder;