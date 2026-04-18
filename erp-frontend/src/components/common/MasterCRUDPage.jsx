import React, { useState, useEffect, useCallback } from 'react';
import { Plus, Edit2, Trash2, X, RefreshCw, Save } from 'lucide-react';
import toast from 'react-hot-toast';

/**
 * Reusable Master Data CRUD Page
 * 
 * Props:
 * - title: string — page title
 * - icon: React component — page icon
 * - columns: [{ key, label, render? }] — table columns
 * - formFields: [{ key, label, type, required?, options? }] — form fields
 * - fetchAll: () => Promise — fetch all records
 * - createFn: (data) => Promise — create record
 * - updateFn: (id, data) => Promise — update record
 * - deleteFn?: (id) => Promise — delete record
 * - idKey: string — primary key field name (default: 'id')
 * - dataPath: string — path to data in response (default: 'data.data')
 */
const MasterCRUDPage = ({
    title,
    icon: Icon,
    columns,
    formFields,
    fetchAll,
    createFn,
    updateFn,
    deleteFn,
    idKey = 'id',
    dataPath = 'data.data',
}) => {
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showForm, setShowForm] = useState(false);
    const [editItem, setEditItem] = useState(null);
    const [formData, setFormData] = useState({});

    const extractData = (response) => {
        const paths = dataPath.split('.');
        let data = response;
        for (const p of paths) data = data?.[p];
        return Array.isArray(data) ? data : (data?.items || data || []);
    };

    const load = useCallback(async () => {
        setLoading(true);
        try {
            const res = await fetchAll();
            setItems(extractData(res));
        } catch (err) {
            toast.error(`Failed to load ${title}`);
        } finally {
            setLoading(false);
        }
    }, [fetchAll, title]);

    useEffect(() => { load(); }, [load]);

    const openCreate = () => {
        setEditItem(null);
        const empty = {};
        formFields.forEach(f => { empty[f.key] = f.defaultValue || ''; });
        setFormData(empty);
        setShowForm(true);
    };

    const openEdit = (item) => {
        setEditItem(item);
        const data = {};
        formFields.forEach(f => { data[f.key] = item[f.key] ?? ''; });
        setFormData(data);
        setShowForm(true);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            if (editItem) {
                await updateFn(editItem[idKey], formData);
                toast.success('Updated!');
            } else {
                await createFn(formData);
                toast.success('Created!');
            }
            setShowForm(false);
            load();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Failed');
        }
    };

    const handleDelete = async (item) => {
        if (!deleteFn) return;
        if (!confirm(`Delete "${item[columns[1]?.key] || item[columns[0]?.key]}"?`)) return;
        try {
            await deleteFn(item[idKey]);
            toast.success('Deleted!');
            load();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Failed to delete');
        }
    };

    return (
        <div className="p-6">
            {/* Header */}
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    {Icon && <Icon className="text-blue-500" size={28} />}
                    <h1 className="text-2xl font-bold text-slate-800">{title}</h1>
                    <span className="text-sm text-slate-400">({items.length})</span>
                </div>
                <div className="flex gap-3">
                    <button onClick={load} className="flex items-center gap-2 px-4 py-2 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm">
                        <RefreshCw size={16} /> Refresh
                    </button>
                    <button onClick={openCreate} className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium">
                        <Plus size={16} /> Add New
                    </button>
                </div>
            </div>

            {/* Table */}
            <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
                <table className="w-full">
                    <thead className="bg-slate-50 border-b border-slate-200">
                        <tr>
                            {columns.map(col => (
                                <th key={col.key} className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase">
                                    {col.label}
                                </th>
                            ))}
                            <th className="text-center px-4 py-3 text-xs font-semibold text-slate-500 uppercase">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                        {loading ? (
                            <tr><td colSpan={columns.length + 1} className="text-center py-10 text-slate-400">Loading...</td></tr>
                        ) : items.length === 0 ? (
                            <tr><td colSpan={columns.length + 1} className="text-center py-10 text-slate-400">No data found</td></tr>
                        ) : items.map((item, i) => (
                            <tr key={item[idKey] || i} className="hover:bg-slate-50 transition-colors">
                                {columns.map(col => (
                                    <td key={col.key} className="px-4 py-3 text-sm text-slate-700">
                                        {col.render ? col.render(item[col.key], item) : (item[col.key] ?? '-')}
                                    </td>
                                ))}
                                <td className="px-4 py-3 text-center">
                                    <div className="flex items-center justify-center gap-1">
                                        <button onClick={() => openEdit(item)} className="p-1.5 rounded hover:bg-blue-50 text-slate-500 hover:text-blue-600" title="Edit">
                                            <Edit2 size={15} />
                                        </button>
                                        {deleteFn && (
                                            <button onClick={() => handleDelete(item)} className="p-1.5 rounded hover:bg-red-50 text-slate-500 hover:text-red-600" title="Delete">
                                                <Trash2 size={15} />
                                            </button>
                                        )}
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {/* Form Modal */}
            {showForm && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg">
                        <div className="flex items-center justify-between p-5 border-b border-slate-200">
                            <h2 className="text-lg font-bold text-slate-800">{editItem ? 'Edit' : 'Create'} {title}</h2>
                            <button onClick={() => setShowForm(false)} className="p-1.5 rounded-lg hover:bg-slate-100">
                                <X size={20} />
                            </button>
                        </div>
                        <form onSubmit={handleSubmit} className="p-5 space-y-4">
                            {formFields.map(field => (
                                <div key={field.key}>
                                    <label className="block text-sm font-medium text-slate-700 mb-1.5">{field.label}</label>
                                    {field.type === 'select' ? (
                                        <select
                                            value={formData[field.key] || ''}
                                            onChange={(e) => setFormData({ ...formData, [field.key]: e.target.value })}
                                            className="w-full px-3 py-2.5 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                                            required={field.required}
                                        >
                                            <option value="">Select...</option>
                                            {(field.options || []).map(o => (
                                                <option key={o.value} value={o.value}>{o.label}</option>
                                            ))}
                                        </select>
                                    ) : (
                                        <input
                                            type={field.type || 'text'}
                                            value={formData[field.key] || ''}
                                            onChange={(e) => setFormData({ ...formData, [field.key]: field.type === 'number' ? parseFloat(e.target.value) || '' : e.target.value })}
                                            className="w-full px-3 py-2.5 border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                                            required={field.required}
                                            step={field.type === 'number' ? 'any' : undefined}
                                        />
                                    )}
                                </div>
                            ))}
                            <div className="flex justify-end gap-3 pt-2">
                                <button type="button" onClick={() => setShowForm(false)} className="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 rounded-lg text-sm font-medium">
                                    Cancel
                                </button>
                                <button type="submit" className="flex items-center gap-2 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium">
                                    <Save size={16} /> {editItem ? 'Update' : 'Create'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default MasterCRUDPage;
