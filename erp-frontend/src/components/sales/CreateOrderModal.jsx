import React, { useState, useEffect } from 'react';
import { X, Plus, Trash2 } from 'lucide-react';
import toast from 'react-hot-toast';
import { createOrder, getCustomers } from '../../api/salesOrderService';
import { getProducts } from '../../api/master/product';

// Fallback hardcoded customers (in case API fails)
const FALLBACK_CUSTOMERS = [
    { id: '6aa86594-9396-4031-9650-f55c3507903b', customerName: 'Test Company Ltd', customerCode: 'CUST-003' },
    { id: 'd1111111-1111-1111-1111-111111111111', customerName: 'ABC Manufacturing Ltd', customerCode: 'CUST-001' },
    { id: 'd2222222-2222-2222-2222-222222222222', customerName: 'XYZ Industries', customerCode: 'CUST-002' },
];

const CreateOrderModal = ({ onClose, onCreated }) => {
    const [customerId, setCustomerId] = useState('');
    const [notes, setNotes] = useState('');
    const [items, setItems] = useState([{ productId: '', quantity: 1 }]);
    const [products, setProducts] = useState([]);
    const [customers, setCustomers] = useState([]);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        // Fetch products from Inventory service
        const fetchProducts = async () => {
            try {
                const res = await getProducts();
                // getProducts() already does response.data, so res = { success, data: { data: [...] } }
                const list = res?.data?.data || res?.data || [];
                setProducts(Array.isArray(list) ? list : []);
            } catch (err) {
                console.error('Failed to load products', err);
                setProducts([]);
            }
        };

        // Fetch customers from Sales service
        const fetchCustomers = async () => {
            try {
                const res = await getCustomers();
                // getCustomers returns axios response, so res.data = { data: { data: [...] }, success }
                const list = res?.data?.data?.data || res?.data?.data || [];
                setCustomers(Array.isArray(list) ? list : []);
            } catch (err) {
                console.error('Failed to load customers, using fallback', err);
                setCustomers(FALLBACK_CUSTOMERS);
            }
        };

        fetchProducts();
        fetchCustomers();
    }, []);

    // Use fallback if API returned empty
    const displayCustomers = customers.length > 0 ? customers : FALLBACK_CUSTOMERS;

    const addItem = () => {
        setItems([...items, { productId: '', quantity: 1 }]);
    };

    const removeItem = (index) => {
        if (items.length <= 1) return;
        setItems(items.filter((_, i) => i !== index));
    };

    const updateItem = (index, field, value) => {
        const updated = [...items];
        updated[index][field] = field === 'quantity' ? parseInt(value) || 0 : value;
        setItems(updated);
    };

    const getProductPrice = (productId) => {
        const product = products.find(p => p.id === productId);
        return product?.price || 0;
    };

    const totalAmount = items.reduce((sum, item) => {
        return sum + (getProductPrice(item.productId) * item.quantity);
    }, 0);

    const handleSubmit = async (e) => {
        e.preventDefault();
        
        if (!customerId) return toast.error('Select a customer');
        if (items.some(i => !i.productId)) return toast.error('Select product for all items');
        if (items.some(i => i.quantity <= 0)) return toast.error('Quantity must be > 0');

        setSubmitting(true);
        try {
            await createOrder({
                customerId,
                notes,
                items: items.map(i => ({
                    productId: i.productId,
                    quantity: i.quantity
                }))
            });
            toast.success('Sales order created!');
            onCreated();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Failed to create order');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="flex items-center justify-between p-5 border-b border-slate-200">
                    <h2 className="text-lg font-bold text-slate-800">Create Sales Order</h2>
                    <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-slate-100">
                        <X size={20} />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-5 space-y-5">
                    {/* Customer */}
                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1.5">Customer</label>
                        <select
                            value={customerId}
                            onChange={(e) => setCustomerId(e.target.value)}
                            className="w-full px-3 py-2.5 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                        >
                            <option value="">Select Customer...</option>
                            {displayCustomers.map(c => (
                                <option key={c.id} value={c.id}>
                                    {c.customerName || c.name} ({c.customerCode || c.code})
                                </option>
                            ))}
                        </select>
                    </div>

                    {/* Notes */}
                    <div>
                        <label className="block text-sm font-medium text-slate-700 mb-1.5">Notes (optional)</label>
                        <input
                            type="text"
                            value={notes}
                            onChange={(e) => setNotes(e.target.value)}
                            placeholder="Urgent, priority delivery..."
                            className="w-full px-3 py-2.5 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                        />
                    </div>

                    {/* Items */}
                    <div>
                        <div className="flex items-center justify-between mb-2">
                            <label className="text-sm font-medium text-slate-700">Order Items</label>
                            <button
                                type="button"
                                onClick={addItem}
                                className="flex items-center gap-1 text-xs text-blue-600 hover:text-blue-700 font-medium"
                            >
                                <Plus size={14} /> Add Item
                            </button>
                        </div>

                        {products.length === 0 && (
                            <p className="text-xs text-amber-600 mb-2">⚠ Loading products...</p>
                        )}

                        <div className="space-y-3">
                            {items.map((item, index) => (
                                <div key={index} className="flex gap-3 items-end bg-slate-50 p-3 rounded-lg">
                                    <div className="flex-1">
                                        <label className="text-xs text-slate-500 mb-1 block">Product</label>
                                        <select
                                            value={item.productId}
                                            onChange={(e) => updateItem(index, 'productId', e.target.value)}
                                            className="w-full px-2.5 py-2 border border-slate-300 rounded-lg text-sm"
                                        >
                                            <option value="">Select product...</option>
                                            {products.map(p => (
                                                <option key={p.id} value={p.id}>
                                                    {p.productName || p.productCode} — ₹{p.price}
                                                </option>
                                            ))}
                                        </select>
                                    </div>
                                    <div className="w-24">
                                        <label className="text-xs text-slate-500 mb-1 block">Qty</label>
                                        <input
                                            type="number"
                                            min="1"
                                            value={item.quantity}
                                            onChange={(e) => updateItem(index, 'quantity', e.target.value)}
                                            className="w-full px-2.5 py-2 border border-slate-300 rounded-lg text-sm"
                                        />
                                    </div>
                                    <div className="w-24 text-right">
                                        <label className="text-xs text-slate-500 mb-1 block">Subtotal</label>
                                        <p className="py-2 text-sm font-medium">
                                            ₹{(getProductPrice(item.productId) * item.quantity).toLocaleString('en-IN')}
                                        </p>
                                    </div>
                                    {items.length > 1 && (
                                        <button
                                            type="button"
                                            onClick={() => removeItem(index)}
                                            className="p-2 text-red-400 hover:text-red-600"
                                        >
                                            <Trash2 size={16} />
                                        </button>
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Total */}
                    <div className="flex justify-end px-3 py-3 bg-slate-50 rounded-lg">
                        <p className="text-lg font-bold text-slate-800">
                            Total: ₹{totalAmount.toLocaleString('en-IN')}
                        </p>
                    </div>

                    {/* Submit */}
                    <div className="flex justify-end gap-3 pt-2">
                        <button
                            type="button"
                            onClick={onClose}
                            className="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm font-medium transition-colors"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={submitting}
                            className="px-6 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors disabled:opacity-50"
                        >
                            {submitting ? 'Creating...' : 'Create Order'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CreateOrderModal;
