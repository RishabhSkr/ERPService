import React, { useEffect, useState } from 'react';
import { getProducts} from '../../api/master/product';       
import { getRawMaterials } from '../../api/master/rawMaterial';
import { Layers, Save, Plus, Trash2, Edit2, AlertCircle, Calendar, User, Info } from 'lucide-react';
import { useBom } from '../../hooks/useBom';
import toast from 'react-hot-toast';

/**
 * BOM (Bill of Materials) Manager
 * 
 * Backend DTOs:
 *   BOMDto: { bomId, productId, bomCode, productName, version, isActive, description, createdAt, lines[] }
 *   BOMLineDto: { bomLineId, lineNumber, rawMaterialId, materialCode, materialName, quantity, unit, scrapPercentage }
 *   CreateBOMDto: { productId, bomCode, productName, description, lines[] }
 *   CreateBOMLineDto: { rawMaterialId, materialCode, materialName, quantity, unit, scrapPercentage }
 *   UpdateBOMDto: { description, lines[] }
 *   UpdateBOMLineDto: { bomLineId?, rawMaterialId, materialCode, materialName, quantity, unit, scrapPercentage }
 */
const BOM = () => {
    const [products, setProducts] = useState([]);
    const [materials, setMaterials] = useState([]);
    const [selectedProduct, setSelectedProduct] = useState('');
    
    // Editor State
    const [bomItems, setBomItems] = useState([]); 
    const [bomMetadata, setBomMetadata] = useState(null); 
    const [isEditMode, setIsEditMode] = useState(false);

    const { loading, createBOM, updateBOM, deleteBOM, getBOMByProductId, boms } = useBom();
    
    // Load Masters
    useEffect(() => {
        const fetchMasters = async () => {
            try {
                const [pData, mData] = await Promise.all([
                    getProducts(), 
                    getRawMaterials()
                ]);
                // These return response.data from axios — unwrap
                const prodList = pData?.data?.data || pData || [];
                const matList = mData?.data?.data || mData || [];
                
                setProducts(Array.isArray(prodList) ? prodList : []);
                setMaterials(Array.isArray(matList) ? matList : []);
            } catch (error) {
                console.error('Error loading master data', error);
            }
        };
        fetchMasters();
    }, []);

    // Handle Product Selection
    const handleProductChange = (e) => {
        const productId = e.target.value;
        loadRecipe(productId);
    };

    const loadRecipe = async (productId) => {
        setSelectedProduct(productId);
        if (!productId) {
            setBomItems([]);
            setBomMetadata(null);
            setIsEditMode(false);
            return;
        }

        try {
            const data = await getBOMByProductId(productId);
            // data is BOMDto: { bomId, bomCode, productName, version, lines[] }
            if (data && data.lines && data.lines.length > 0) {
                setIsEditMode(true);
                setBomMetadata({
                    bomId: data.bomId,
                    bomCode: data.bomCode,
                    version: data.version,
                    createdAt: data.createdAt,
                    description: data.description,
                });
                setBomItems(data.lines.map(line => ({
                    bomLineId: line.bomLineId,
                    rawMaterialId: line.rawMaterialId,
                    materialCode: line.materialCode,
                    materialName: line.materialName,
                    quantity: line.quantity,
                    unit: line.unit,
                    scrapPercentage: line.scrapPercentage || 0,
                })));
            } else {
                setIsEditMode(false);
                setBomMetadata(null);
                setBomItems([]); 
            }
        } catch (error) {
            console.log('No BOM found for this product', error);
            setBomItems([]);
            setBomMetadata(null);
            setIsEditMode(false);
        }
    };

    // UI Handlers
    const addRow = () => {
        setBomItems([...bomItems, { rawMaterialId: '', materialCode: '', materialName: '', quantity: 1, unit: 'pcs', scrapPercentage: 0 }]);
    };
    const removeRow = (index) => {
        setBomItems(bomItems.filter((_, i) => i !== index));
    };

    const updateRow = (index, field, value) => {
        const newItems = [...bomItems];
        newItems[index] = { ...newItems[index], [field]: value };
        
        // Auto-populate materialCode, materialName, unit when rawMaterialId changes
        if (field === 'rawMaterialId') {
            const mat = materials.find(m => (m.id || m.rawMaterialId) === value);
            if (mat) {
                newItems[index].materialCode = mat.materialCode || mat.code || '';
                newItems[index].materialName = mat.materialName || mat.name || '';
                newItems[index].unit = mat.unit || mat.uom || 'pcs';
            }
        }
        
        setBomItems(newItems);
    };

    // Get product info for create payload
    const getSelectedProductInfo = () => {
        return products.find(p => (p.id || p.productId) === selectedProduct);
    };

    // Save Logic
    const handleSave = async () => {
        if (!selectedProduct) return toast.error('Select a product first.');

        const validItems = bomItems.filter(i => i.rawMaterialId && i.quantity > 0);
        if (validItems.length === 0) return toast.error('Add at least one material.');

        // Duplicate check
        const materialIds = validItems.map(i => i.rawMaterialId);
        const duplicates = materialIds.filter((id, idx) => materialIds.indexOf(id) !== idx);
        if (duplicates.length > 0) {
            return toast.error('Duplicate materials found. Each material can only appear once.');
        }

        if (isEditMode && bomMetadata?.bomId) {
            // UPDATE — UpdateBOMDto { description, lines[] }
            const payload = {
                description: bomMetadata.description || null,
                lines: validItems.map(i => ({
                    bomLineId: i.bomLineId || null,
                    rawMaterialId: i.rawMaterialId,
                    materialCode: i.materialCode,
                    materialName: i.materialName,
                    quantity: parseFloat(i.quantity),
                    unit: i.unit || 'pcs',
                    scrapPercentage: parseFloat(i.scrapPercentage) || 0,
                })),
            };
            await updateBOM(bomMetadata.bomId, payload);
        } else {
            // CREATE — CreateBOMDto { productId, bomCode, productName, description, lines[] }
            const prod = getSelectedProductInfo();
            const payload = {
                productId: selectedProduct,
                bomCode: `BOM-${(prod?.productCode || prod?.code || 'ITEM').toUpperCase()}-001`,
                productName: prod?.productName || prod?.name || '',
                description: null,
                lines: validItems.map(i => ({
                    rawMaterialId: i.rawMaterialId,
                    materialCode: i.materialCode,
                    materialName: i.materialName,
                    quantity: parseFloat(i.quantity),
                    unit: i.unit || 'pcs',
                    scrapPercentage: parseFloat(i.scrapPercentage) || 0,
                })),
            };
            await createBOM(payload);
            setIsEditMode(true);
        }
        // Re-fetch
        await loadRecipe(selectedProduct);
    };

    // Delete Logic — now uses bomId
    const handleDelete = async () => {
        if (!bomMetadata?.bomId) return toast.error('No BOM to delete');
        if (confirm("Are you sure you want to deactivate this BOM?")) {
            await deleteBOM(bomMetadata.bomId);
            setSelectedProduct('');
            setBomItems([]);
            setBomMetadata(null);
            setIsEditMode(false);
        }
    };

    // Helper: get material display info
    const getMaterialName = (id) => {
        const m = materials.find(mat => (mat.id || mat.rawMaterialId) === id);
        return m ? `${m.materialName || m.name} (${m.unit || m.uom || ''})` : id;
    };

    return (
        <div className="p-4 lg:p-6 max-w-7xl mx-auto flex flex-col lg:flex-row gap-6 items-start">
            
            {/* Left Column: BOM Editor */}
            <div className="flex-1 w-full min-w-0">
                <h2 className="text-2xl font-bold mb-6 flex items-center gap-2">
                    <Layers className="text-purple-600" />
                    BOM / Recipe Manager
                </h2>

                {/* Product Selector */}
                <div className="bg-white p-4 lg:p-6 rounded shadow mb-6">
                    <label className="block text-sm font-medium mb-2">Select Product</label>
                    <select
                        className="w-full border p-2 rounded text-lg font-medium focus:ring-2 focus:ring-purple-500"
                        value={selectedProduct}
                        onChange={handleProductChange} 
                    >
                        <option value="">-- Choose Product --</option>
                        {products.map(p => (
                            <option key={p.id || p.productId} value={p.id || p.productId}>
                                {p.productName || p.name} ({p.productCode || p.code})
                            </option>
                        ))}
                    </select>
                </div>

                {/* Recipe Editor */}
                {selectedProduct && (
                    <div className="bg-white p-4 lg:p-6 rounded shadow">
                        
                        {/* Header & Actions */}
                        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
                            <h3 className="text-lg font-semibold flex items-center gap-2">
                                {isEditMode ? 'Edit Recipe' : 'Create New Recipe'}
                                {loading && <span className="text-xs text-gray-500 animate-pulse">(Saving...)</span>}
                            </h3>
                            
                            <div className="flex gap-2 w-full sm:w-auto">
                                {isEditMode && (
                                    <button 
                                        onClick={handleDelete}
                                        className="text-red-500 hover:bg-red-50 px-3 py-1 rounded flex items-center gap-1 text-sm border border-red-100"
                                    >
                                        <Trash2 size={16} /> Delete
                                    </button>
                                )}
                                <button
                                    onClick={addRow}
                                    className="bg-blue-50 text-blue-600 hover:bg-blue-100 px-4 py-1.5 rounded flex items-center gap-1 text-sm font-medium transition ml-auto sm:ml-0"
                                >
                                    <Plus size={16} /> Add Material
                                </button>
                            </div>
                        </div>

                        {/* Metadata Panel */}
                        {bomMetadata && isEditMode && (
                            <div className="bg-gray-50 p-4 rounded-lg mb-6 border border-gray-100 grid grid-cols-2 md:grid-cols-4 gap-4 text-xs text-gray-600">
                                <div>
                                    <span className="block text-gray-400 mb-1 flex items-center gap-1"><Layers size={12}/> BOM Code</span>
                                    <span className="font-bold text-purple-700 text-sm">{bomMetadata.bomCode || '-'}</span>
                                </div>
                                <div>
                                    <span className="block text-gray-400 mb-1">Version</span>
                                    <span className="font-bold text-blue-600 text-sm">v{bomMetadata.version || 1}</span>
                                </div>
                                <div>
                                    <span className="block text-gray-400 mb-1 flex items-center gap-1"><Calendar size={12}/> Created At</span>
                                    <span className="font-medium">{bomMetadata.createdAt ? new Date(bomMetadata.createdAt).toLocaleDateString() : '-'}</span>
                                </div>
                                <div>
                                    <span className="block text-gray-400 mb-1">BOM ID</span>
                                    <span className="font-medium font-mono">{bomMetadata.bomId ? String(bomMetadata.bomId).slice(0,8) + '...' : '-'}</span>
                                </div>
                            </div>
                        )}

                        {/* Materials Table */}
                        <div className="overflow-x-auto rounded-lg border border-gray-200 mb-6">
                            <table className="w-full text-sm text-left">
                                <thead className="text-xs text-gray-700 uppercase bg-gray-50 border-b">
                                    <tr>
                                        <th className="px-4 py-3 w-10">#</th>
                                        <th className="px-4 py-3 min-w-[180px]">Material</th>
                                        <th className="px-4 py-3 w-24">Code</th>
                                        <th className="px-4 py-3 w-20 text-center">Qty</th>
                                        <th className="px-4 py-3 w-16">Unit</th>
                                        <th className="px-4 py-3 w-20 text-center">Scrap %</th>
                                        <th className="px-4 py-3 w-16"></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {bomItems.map((item, index) => (
                                        <tr key={index} className="bg-white border-b hover:bg-gray-50 last:border-0">
                                            <td className="px-4 py-3 text-gray-400 font-mono">{index + 1}</td>
                                            <td className="px-4 py-3">
                                                 <select
                                                    className="w-full border p-2 rounded bg-white focus:outline-none focus:ring-1 focus:ring-blue-500"
                                                    value={item.rawMaterialId}
                                                    onChange={e => updateRow(index, 'rawMaterialId', e.target.value)}
                                                >
                                                    <option value="">Select Material</option>
                                                    {materials.map(m => (
                                                        <option key={m.id || m.rawMaterialId} value={m.id || m.rawMaterialId}>
                                                            {m.materialName || m.name} ({m.unit || m.uom || ''})
                                                        </option>
                                                    ))}
                                                </select>
                                            </td>
                                            <td className="px-4 py-3 text-gray-500 font-mono text-xs">
                                                {item.materialCode || '-'}
                                            </td>
                                            <td className="px-4 py-3">
                                                 <input
                                                    type="number"
                                                    className="w-full border p-1.5 rounded text-center focus:outline-none focus:ring-1 focus:ring-blue-500"
                                                    placeholder="0"
                                                    min="0"
                                                    step="any"
                                                    value={item.quantity}
                                                    onChange={e => updateRow(index, 'quantity', e.target.value)}
                                                />
                                            </td>
                                            <td className="px-4 py-3 text-gray-500 text-xs">
                                                {item.unit || '-'}
                                            </td>
                                            <td className="px-4 py-3">
                                                <input
                                                    type="number"
                                                    className="w-full border p-1.5 rounded text-center focus:outline-none focus:ring-1 focus:ring-blue-500"
                                                    placeholder="0"
                                                    min="0"
                                                    step="0.1"
                                                    value={item.scrapPercentage}
                                                    onChange={e => updateRow(index, 'scrapPercentage', e.target.value)}
                                                />
                                            </td>
                                            <td className="px-4 py-3 text-right">
                                                 <button 
                                                    onClick={() => removeRow(index)} 
                                                    className="text-gray-400 hover:text-red-500 p-1.5 rounded hover:bg-red-50 transition"
                                                    title="Remove Item"
                                                >
                                                    <Trash2 size={16} />
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                    {bomItems.length === 0 && (
                                        <tr>
                                            <td colSpan="7" className="px-4 py-8 text-center text-gray-400 bg-gray-50/30">
                                                <div className="flex flex-col items-center gap-2">
                                                    <Info size={24} className="opacity-20"/>
                                                    <span>No raw materials added yet.</span>
                                                    <button onClick={addRow} className="text-blue-600 hover:underline text-xs">Add First Item</button>
                                                </div>
                                            </td>
                                        </tr>
                                    )}
                                </tbody>
                            </table>
                        </div>

                        <div className="flex justify-end">
                            <button
                                onClick={handleSave}
                                disabled={loading}
                                className={`px-6 py-2.5 rounded flex items-center gap-2 shadow-lg text-white font-medium transition transform active:scale-95
                                    ${loading ? 'bg-purple-400 cursor-not-allowed' : 'bg-purple-600 hover:bg-purple-700'}`}
                            >
                                <Save size={18} /> 
                                {loading ? 'Saving...' : 'Save Recipe'}
                            </button>
                        </div>
                    </div>
                )}

                 {!selectedProduct && (
                     <div className="text-center py-12 text-gray-400 bg-white/50 rounded border-2 border-dashed border-gray-200">
                         <AlertCircle size={48} className="mx-auto mb-2 opacity-20" />
                         <p>Select a product to view or create its recipe.</p>
                     </div>
                 )}
            </div>

            {/* Right Column: Existing Recipes List */}
            <div className="w-full lg:w-80 bg-white rounded shadow h-fit border border-gray-100 flex-shrink-0">
                <div className="p-4 border-b bg-gray-50 flex justify-between items-center">
                    <h3 className="font-semibold text-gray-700">Existing Recipes</h3>
                    <span className="bg-gray-200 text-gray-600 px-2 py-0.5 rounded-full text-xs font-bold">{boms.length}</span>
                </div>
                <div className="max-h-[500px] overflow-y-auto">
                    {boms.length === 0 ? (
                        <div className="p-8 text-center text-gray-400 text-sm">
                            <p>No recipes found.</p>
                            <p className="text-xs mt-1 opacity-70">Create one to get started.</p>
                        </div>
                    ) : (
                        boms.map((bom) => (
                            <button 
                                key={bom.bomId || bom.productId}
                                onClick={() => loadRecipe(bom.productId)}
                                className={`w-full text-left p-4 border-b hover:bg-purple-50 transition flex justify-between items-center group
                                    ${selectedProduct == bom.productId ? 'bg-purple-50 border-l-4 border-purple-500' : 'border-l-4 border-transparent'}`}
                            >
                                <div className="min-w-0">
                                    <h4 className="font-medium text-gray-800 truncate">{bom.productName}</h4>
                                    <p className="text-xs text-purple-600 font-mono">{bom.bomCode || ''}</p>
                                    <p className="text-xs text-gray-500 flex items-center gap-1">
                                        <Layers size={10}/> {bom.lines?.length || 0} Lines · v{bom.version || 1}
                                    </p>
                                </div>
                                <Edit2 size={16} className={`text-gray-300 group-hover:text-purple-500 transition ${selectedProduct == bom.productId ? 'text-purple-500' : ''}`} />
                            </button>
                        ))
                    )}
                </div>
            </div>
        </div>
    );
};

export default BOM;