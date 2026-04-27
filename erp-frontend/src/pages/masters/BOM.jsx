import React, { useEffect, useState } from 'react';
import { getProducts } from '../../api/master/product';
import { getRawMaterials } from '../../api/master/rawMaterial';
import { getProcesses } from '../../api/productionService';
import { Layers, Save, Plus, Trash2, Edit2, AlertCircle, Calendar, Info, ArrowLeft, Search, RefreshCw, Loader } from 'lucide-react';
import { useBom } from '../../hooks/useBom';
import SearchSelect from '../../components/common/SearchSelect';
import toast from 'react-hot-toast';

const BOM = () => {
    const [products, setProducts] = useState([]);
    const [materials, setMaterials] = useState([]);
    const [processes, setProcesses] = useState([]);
    const [searchQuery, setSearchQuery] = useState('');

    // View: 'list' or 'editor'
    const [view, setView] = useState('list');
    const [selectedProduct, setSelectedProduct] = useState('');
    const [bomItems, setBomItems] = useState([]);
    const [bomMetadata, setBomMetadata] = useState(null);
    const [isEditMode, setIsEditMode] = useState(false);

    const { loading, createBOM, updateBOM, deleteBOM, getBOMByProductId, boms, fetchBoms } = useBom();

    useEffect(() => {
        const fetchMasters = async () => {
            try {
                const [pData, mData, procData] = await Promise.all([getProducts(), getRawMaterials(), getProcesses()]);
                const prodList = pData?.data?.data || pData || [];
                const matList = mData?.data?.data || mData || [];
                const procList = procData?.data?.data || procData?.data || [];
                setProducts(Array.isArray(prodList) ? prodList : []);
                setMaterials(Array.isArray(matList) ? matList : []);
                setProcesses(Array.isArray(procList) ? procList : []);
            } catch (error) { console.error('Error loading master data', error); }
        };
        fetchMasters();
    }, []);

    // Filter BOMs by search
    const filteredBoms = boms.filter(b => {
        if (!searchQuery) return true;
        const q = searchQuery.toLowerCase();
        return (b.bomCode || '').toLowerCase().includes(q) ||
            (b.productName || '').toLowerCase().includes(q);
    });

    // Open editor for existing BOM
    const openBOM = async (bom) => {
        setSelectedProduct(bom.productId);
        try {
            const data = await getBOMByProductId(bom.productId);
            if (data?.lines?.length > 0) {
                setIsEditMode(true);
                setBomMetadata({ bomId: data.bomId, bomCode: data.bomCode, version: data.version, createdAt: data.createdAt, description: data.description });
                setBomItems(data.lines.map(line => ({
                    bomLineId: line.bomLineId, rawMaterialId: line.rawMaterialId,
                    materialCode: line.materialCode, materialName: line.materialName,
                    quantity: line.quantity, unit: line.unit, scrapPercentage: line.scrapPercentage || 0,
                    processId: line.processId || '', processCode: line.processCode || '', processName: line.processName || '',
                })));
            }
        } catch { toast.error('Failed to load BOM'); }
        setView('editor');
    };

    // Open editor for new BOM
    const openCreateNew = () => {
        setSelectedProduct('');
        setBomItems([]);
        setBomMetadata(null);
        setIsEditMode(false);
        setView('editor');
    };

    const goBack = () => { setView('list'); setSelectedProduct(''); setBomItems([]); setBomMetadata(null); setIsEditMode(false); };

    // Product selection for new BOM
    const handleProductSelect = async (product) => {
        const productId = product.id || product.productId;
        setSelectedProduct(productId);
        try {
            const data = await getBOMByProductId(productId);
            if (data?.lines?.length > 0) {
                setIsEditMode(true);
                setBomMetadata({ bomId: data.bomId, bomCode: data.bomCode, version: data.version, createdAt: data.createdAt, description: data.description });
                setBomItems(data.lines.map(line => ({
                    bomLineId: line.bomLineId, rawMaterialId: line.rawMaterialId,
                    materialCode: line.materialCode, materialName: line.materialName,
                    quantity: line.quantity, unit: line.unit, scrapPercentage: line.scrapPercentage || 0,
                    processId: line.processId || '', processCode: line.processCode || '', processName: line.processName || '',
                })));
            } else { setIsEditMode(false); setBomMetadata(null); setBomItems([]); }
        } catch { setBomItems([]); setBomMetadata(null); setIsEditMode(false); }
    };

    const getProductDisplayValue = () => {
        const p = products.find(p => (p.id || p.productId) === selectedProduct);
        return p ? `${p.productName || p.name} (${p.productCode || p.code})` : '';
    };
    const getProductName = (pid) => {
        const p = products.find(pr => (pr.id || pr.productId) === pid);
        return p ? (p.productName || p.name) : pid;
    };

    // Row handlers
    const addRow = () => setBomItems([...bomItems, { rawMaterialId: '', materialCode: '', materialName: '', quantity: 1, unit: 'pcs', scrapPercentage: 0, processId: '', processCode: '', processName: '' }]);
    const removeRow = (i) => setBomItems(bomItems.filter((_, idx) => idx !== i));
    const updateRow = (i, field, value) => { const n = [...bomItems]; n[i] = { ...n[i], [field]: value }; setBomItems(n); };
    const handleMaterialSelect = (i, mat) => {
        const n = [...bomItems];
        n[i] = { ...n[i], rawMaterialId: mat.id || mat.rawMaterialId, materialCode: mat.materialCode || mat.code || '', materialName: mat.materialName || mat.name || '', unit: mat.unit || mat.uom || 'pcs' };
        setBomItems(n);
    };
    const handleProcessSelect = (i, proc) => {
        const n = [...bomItems];
        n[i] = { ...n[i], processId: proc.processId, processCode: proc.processCode, processName: proc.processName };
        setBomItems(n);
    };

    // Save
    const handleSave = async () => {
        if (!selectedProduct) return toast.error('Select a product first.');
        const validItems = bomItems.filter(i => i.rawMaterialId && i.quantity > 0);
        if (validItems.length === 0) return toast.error('Add at least one material.');
        const ids = validItems.map(i => i.rawMaterialId);
        if (ids.filter((id, idx) => ids.indexOf(id) !== idx).length > 0) return toast.error('Duplicate materials found.');

        const mapLines = (item) => ({
            bomLineId: item.bomLineId || null, rawMaterialId: item.rawMaterialId,
            materialCode: item.materialCode, materialName: item.materialName,
            quantity: parseFloat(item.quantity), unit: item.unit || 'pcs',
            scrapPercentage: parseFloat(item.scrapPercentage) || 0,
            processId: item.processId || null, processCode: item.processCode || null, processName: item.processName || null,
        });

        if (isEditMode && bomMetadata?.bomId) {
            await updateBOM(bomMetadata.bomId, { description: bomMetadata.description || null, lines: validItems.map(mapLines) });
        } else {
            const prod = products.find(p => (p.id || p.productId) === selectedProduct);
            await createBOM({
                productId: selectedProduct,
                bomCode: `BOM-${(prod?.productCode || prod?.code || 'ITEM').toUpperCase()}-001`,
                productName: prod?.productName || prod?.name || '',
                description: null, lines: validItems.map(mapLines),
            });
            setIsEditMode(true);
        }
        await handleProductSelect({ id: selectedProduct });
    };

    const handleDelete = async () => {
        if (!bomMetadata?.bomId) return toast.error('No BOM to delete');
        if (confirm("Are you sure you want to deactivate this BOM?")) {
            await deleteBOM(bomMetadata.bomId);
            goBack();
        }
    };

    // ═══════════════ LIST VIEW ═══════════════
    if (view === 'list') return (
        <div className="p-6">
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <Layers className="text-purple-600" size={28} />
                    <h1 className="text-2xl font-bold text-slate-800">Bill of Materials</h1>
                    <span className="text-sm text-slate-400">({boms.length})</span>
                </div>
                <div className="flex gap-3">
                    <button onClick={fetchBoms} className="flex items-center gap-2 px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">
                        <RefreshCw size={16} /> Refresh
                    </button>
                    <button onClick={openCreateNew} className="flex items-center gap-2 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg text-sm font-medium">
                        <Plus size={16} /> Create BOM
                    </button>
                </div>
            </div>

            {/* Search */}
            <div className="relative mb-4">
                <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                <input type="text" value={searchQuery} onChange={e => setSearchQuery(e.target.value)}
                    placeholder="Search by BOM code or product name..."
                    className="w-full pl-10 pr-4 py-2.5 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent" />
            </div>

            {/* BOM Table */}
            <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
                <table className="w-full">
                    <thead className="bg-slate-50 border-b">
                        <tr>
                            <th className="text-left px-5 py-3 text-xs font-semibold text-slate-500 uppercase">BOM Code</th>
                            <th className="text-left px-5 py-3 text-xs font-semibold text-slate-500 uppercase">Product</th>
                            <th className="text-center px-5 py-3 text-xs font-semibold text-slate-500 uppercase">Version</th>
                            <th className="text-center px-5 py-3 text-xs font-semibold text-slate-500 uppercase">Materials</th>
                            <th className="text-center px-5 py-3 text-xs font-semibold text-slate-500 uppercase">Status</th>
                            <th className="text-left px-5 py-3 text-xs font-semibold text-slate-500 uppercase">Created</th>
                            <th className="text-center px-5 py-3 text-xs font-semibold text-slate-500 uppercase">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                        {loading ? (
                            <tr><td colSpan="7" className="text-center py-10 text-slate-400"><Loader className="inline animate-spin mr-2" size={16} />Loading...</td></tr>
                        ) : filteredBoms.length === 0 ? (
                            <tr><td colSpan="7" className="text-center py-10 text-slate-400">No BOMs found</td></tr>
                        ) : filteredBoms.map(bom => (
                            <tr key={bom.bomId} className="hover:bg-slate-50 transition-colors cursor-pointer" onClick={() => openBOM(bom)}>
                                <td className="px-5 py-3.5 text-sm font-mono font-semibold text-purple-700">{bom.bomCode}</td>
                                <td className="px-5 py-3.5 text-sm text-slate-700 font-medium">{bom.productName || getProductName(bom.productId)}</td>
                                <td className="px-5 py-3.5 text-center"><span className="bg-slate-100 px-2.5 py-0.5 rounded font-mono text-xs font-bold">v{bom.version || 1}</span></td>
                                <td className="px-5 py-3.5 text-center"><span className="bg-blue-50 text-blue-700 px-2.5 py-0.5 rounded-full text-xs font-bold">{bom.lines?.length || 0} items</span></td>
                                <td className="px-5 py-3.5 text-center">
                                    <span className={`px-2.5 py-0.5 rounded-full text-xs font-semibold ${bom.isActive !== false ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                                        {bom.isActive !== false ? 'Active' : 'Inactive'}
                                    </span>
                                </td>
                                <td className="px-5 py-3.5 text-sm text-slate-500">{bom.createdAt ? new Date(bom.createdAt).toLocaleDateString() : '-'}</td>
                                <td className="px-5 py-3.5 text-center" onClick={e => e.stopPropagation()}>
                                    <button onClick={() => openBOM(bom)} className="p-1.5 rounded hover:bg-purple-50 text-slate-500 hover:text-purple-600" title="Edit">
                                        <Edit2 size={15} />
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );

    // ═══════════════ EDITOR VIEW ═══════════════
    return (
        <div className="p-6 max-w-6xl mx-auto">
            {/* Back + Title */}
            <div className="flex items-center gap-3 mb-6">
                <button onClick={goBack} className="p-2 rounded-lg hover:bg-slate-100 text-slate-500 hover:text-slate-700 transition">
                    <ArrowLeft size={20} />
                </button>
                <Layers className="text-purple-600" size={24} />
                <h1 className="text-xl font-bold text-slate-800">{isEditMode ? 'Edit BOM' : 'Create New BOM'}</h1>
            </div>

            {/* Product Selector */}
            <div className="bg-white p-5 rounded-xl shadow-sm border border-slate-200 mb-6">
                <label className="block text-xs font-semibold text-slate-500 uppercase mb-2">Select Product *</label>
                <SearchSelect
                    value={selectedProduct} displayValue={getProductDisplayValue()}
                    placeholder="Search and select product..."
                    items={products} title="Select Product"
                    displayFields={[
                        { key: 'productCode', label: 'Code', width: '30%', bold: true },
                        { key: 'productName', label: 'Name', width: '50%' },
                        { key: 'price', label: 'Price', width: '20%' },
                    ]}
                    searchKeys={['productCode', 'productName', 'code', 'name']}
                    valueKey="id"
                    onSelect={handleProductSelect}
                    onClear={() => { setSelectedProduct(''); setBomItems([]); setBomMetadata(null); setIsEditMode(false); }}
                />
            </div>

            {/* Editor */}
            {selectedProduct ? (
                <div className="bg-white p-5 rounded-xl shadow-sm border border-slate-200">
                    <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
                        <h3 className="text-lg font-semibold flex items-center gap-2">
                            {isEditMode ? 'Edit Recipe' : 'Create New Recipe'}
                            {loading && <span className="text-xs text-gray-500 animate-pulse">(Saving...)</span>}
                        </h3>
                        <div className="flex gap-2">
                            {isEditMode && (
                                <button onClick={handleDelete} className="text-red-500 hover:bg-red-50 px-3 py-1.5 rounded-lg flex items-center gap-1 text-sm border border-red-100">
                                    <Trash2 size={16} /> Delete
                                </button>
                            )}
                            <button onClick={addRow} className="bg-blue-50 text-blue-600 hover:bg-blue-100 px-4 py-1.5 rounded-lg flex items-center gap-1 text-sm font-medium transition">
                                <Plus size={16} /> Add Material
                            </button>
                        </div>
                    </div>

                    {/* Metadata */}
                    {bomMetadata && isEditMode && (
                        <div className="bg-slate-50 p-4 rounded-lg mb-6 border border-slate-100 grid grid-cols-2 md:grid-cols-4 gap-4 text-xs text-slate-600">
                            <div><span className="block text-slate-400 mb-1 flex items-center gap-1"><Layers size={12} /> BOM Code</span><span className="font-bold text-purple-700 text-sm">{bomMetadata.bomCode || '-'}</span></div>
                            <div><span className="block text-slate-400 mb-1">Version</span><span className="font-bold text-blue-600 text-sm">v{bomMetadata.version || 1}</span></div>
                            <div><span className="block text-slate-400 mb-1 flex items-center gap-1"><Calendar size={12} /> Created</span><span className="font-medium">{bomMetadata.createdAt ? new Date(bomMetadata.createdAt).toLocaleDateString() : '-'}</span></div>
                            <div><span className="block text-slate-400 mb-1">BOM ID</span><span className="font-medium font-mono">{bomMetadata.bomId ? String(bomMetadata.bomId).slice(0, 8) + '...' : '-'}</span></div>
                        </div>
                    )}

                    {/* Materials Table */}
                    <div className="overflow-x-auto rounded-lg border border-slate-200 mb-6">
                        <table className="w-full text-sm text-left">
                            <thead className="text-xs text-slate-700 uppercase bg-slate-50 border-b">
                                <tr>
                                    <th className="px-3 py-3 w-10">#</th>
                                    <th className="px-3 py-3 min-w-[170px]">Material</th>
                                    <th className="px-3 py-3 w-20">Code</th>
                                    <th className="px-3 py-3 w-16 text-center">Qty</th>
                                    <th className="px-3 py-3 w-14">Unit</th>
                                    <th className="px-3 py-3 w-16 text-center">Scrap%</th>
                                    <th className="px-3 py-3 min-w-[150px]">Process</th>
                                    <th className="px-3 py-3 w-12"></th>
                                </tr>
                            </thead>
                            <tbody>
                                {bomItems.map((item, index) => (
                                    <tr key={index} className="bg-white border-b hover:bg-slate-50 last:border-0">
                                        <td className="px-3 py-3 text-slate-400 font-mono">{index + 1}</td>
                                        <td className="px-3 py-3">
                                            <SearchSelect value={item.rawMaterialId} displayValue={item.materialName || 'Select...'} placeholder="Select material..."
                                                items={materials} title="Select Raw Material"
                                                displayFields={[{ key: 'materialCode', label: 'Code', width: '30%', bold: true }, { key: 'materialName', label: 'Name', width: '50%' }, { key: 'unit', label: 'Unit', width: '20%' }]}
                                                searchKeys={['materialCode', 'materialName', 'code', 'name']} valueKey="id"
                                                onSelect={(mat) => handleMaterialSelect(index, mat)} size="sm" />
                                        </td>
                                        <td className="px-3 py-3 text-slate-500 font-mono text-xs">{item.materialCode || '-'}</td>
                                        <td className="px-3 py-3">
                                            <input type="number" className="w-full border p-1.5 rounded text-center focus:outline-none focus:ring-1 focus:ring-blue-500"
                                                placeholder="0" min="0" step="any" value={item.quantity} onChange={e => updateRow(index, 'quantity', e.target.value)} />
                                        </td>
                                        <td className="px-3 py-3 text-slate-500 text-xs">{item.unit || '-'}</td>
                                        <td className="px-3 py-3">
                                            <input type="number" className="w-full border p-1.5 rounded text-center focus:outline-none focus:ring-1 focus:ring-blue-500"
                                                placeholder="0" min="0" step="0.1" value={item.scrapPercentage} onChange={e => updateRow(index, 'scrapPercentage', e.target.value)} />
                                        </td>
                                        <td className="px-3 py-3">
                                            <SearchSelect value={item.processId} displayValue={item.processId ? `${item.processCode} - ${item.processName}` : ''} placeholder="Select process..."
                                                items={processes} title="Select Process"
                                                displayFields={[{ key: 'processCode', label: 'Code', width: '25%', bold: true }, { key: 'processName', label: 'Name', width: '45%' }, { key: 'category', label: 'Category', width: '30%' }]}
                                                searchKeys={['processCode', 'processName', 'category']} valueKey="processId"
                                                onSelect={(proc) => handleProcessSelect(index, proc)}
                                                onClear={() => { updateRow(index, 'processId', ''); updateRow(index, 'processCode', ''); updateRow(index, 'processName', ''); }}
                                                size="sm" />
                                        </td>
                                        <td className="px-3 py-3 text-right">
                                            <button onClick={() => removeRow(index)} className="text-slate-400 hover:text-red-500 p-1.5 rounded hover:bg-red-50 transition" title="Remove">
                                                <Trash2 size={16} />
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                                {bomItems.length === 0 && (
                                    <tr><td colSpan="8" className="px-4 py-8 text-center text-slate-400 bg-slate-50/30">
                                        <div className="flex flex-col items-center gap-2">
                                            <Info size={24} className="opacity-20" />
                                            <span>No raw materials added yet.</span>
                                            <button onClick={addRow} className="text-blue-600 hover:underline text-xs">Add First Item</button>
                                        </div>
                                    </td></tr>
                                )}
                            </tbody>
                        </table>
                    </div>

                    <div className="flex justify-end">
                        <button onClick={handleSave} disabled={loading}
                            className={`px-6 py-2.5 rounded-lg flex items-center gap-2 shadow-lg text-white font-medium transition transform active:scale-95
                                ${loading ? 'bg-purple-400 cursor-not-allowed' : 'bg-purple-600 hover:bg-purple-700'}`}>
                            <Save size={18} /> {loading ? 'Saving...' : 'Save Recipe'}
                        </button>
                    </div>
                </div>
            ) : (
                <div className="text-center py-12 text-slate-400 bg-white/50 rounded-xl border-2 border-dashed border-slate-200">
                    <AlertCircle size={48} className="mx-auto mb-2 opacity-20" />
                    <p>Select a product above to view or create its recipe.</p>
                </div>
            )}
        </div>
    );
};

export default BOM;