import React, { useState } from 'react';
import { Building2, Eye, X, Package, Box, MapPin, Layers } from 'lucide-react';
import MasterCRUDPage from '../../components/common/MasterCRUDPage';
import { getWarehouses, getWarehouseById, createWarehouse, updateWarehouse, deleteWarehouse } from '../../api/master/warehouse';

const columns = [
    { key: 'warehouseCode', label: 'Code' },
    { key: 'warehouseName', label: 'Name' },
    { key: 'city', label: 'City' },
    { key: 'address', label: 'Address' },
    { key: 'isActive', label: 'Status', render: (v) => (
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${v ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
            {v ? 'Active' : 'Inactive'}
        </span>
    )},
];

const formFields = [
    { key: 'warehouseCode', label: 'Warehouse Code', required: true },
    { key: 'warehouseName', label: 'Warehouse Name', required: true },
    { key: 'city', label: 'City' },
    { key: 'address', label: 'Address' },
];

const WarehouseLayoutModal = ({ warehouse, onClose }) => {
    if (!warehouse) return null;
    const locations = warehouse.storageLocations || [];

    return (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-2xl w-full max-w-4xl max-h-[85vh] overflow-hidden">
                {/* Header */}
                <div className="bg-gradient-to-r from-slate-800 to-slate-700 text-white px-6 py-4 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <Building2 size={22} />
                        <div>
                            <h2 className="text-lg font-bold">{warehouse.warehouseName}</h2>
                            <p className="text-sm text-slate-300">{warehouse.warehouseCode} • {warehouse.city || 'N/A'}</p>
                        </div>
                    </div>
                    <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-white/10 transition-colors">
                        <X size={20} />
                    </button>
                </div>

                {/* Body */}
                <div className="p-6 overflow-y-auto max-h-[calc(85vh-80px)]">
                    {locations.length === 0 ? (
                        <div className="text-center py-12 text-gray-400">
                            <MapPin size={48} className="mx-auto mb-3 opacity-40" />
                            <p className="text-lg font-medium">No Storage Locations</p>
                            <p className="text-sm">This warehouse has no storage locations configured yet.</p>
                        </div>
                    ) : (
                        <div className="space-y-4">
                            {locations.map((loc) => (
                                <div key={loc.id} className="border border-gray-200 rounded-xl overflow-hidden hover:shadow-md transition-shadow">
                                    {/* Location Header */}
                                    <div className="bg-gray-50 px-5 py-3 flex items-center justify-between border-b">
                                        <div className="flex items-center gap-3">
                                            <MapPin size={16} className="text-blue-500" />
                                            <span className="font-mono font-bold text-sm text-slate-800">{loc.locationCode}</span>
                                            <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                                                loc.locationTypeName === 'Raw Materials' ? 'bg-blue-100 text-blue-700' :
                                                loc.locationTypeName === 'Finished Goods' ? 'bg-purple-100 text-purple-700' :
                                                'bg-gray-100 text-gray-600'
                                            }`}>
                                                {loc.locationTypeName || 'Untyped'}
                                            </span>
                                        </div>
                                        <div className="flex items-center gap-2 text-xs text-gray-500">
                                            {loc.zone && <span>Zone: {loc.zone}</span>}
                                            {loc.rack && <span>• Rack: {loc.rack}</span>}
                                        </div>
                                    </div>

                                    {/* Stored Items */}
                                    <div className="px-5 py-3">
                                        {(!loc.storedItems || loc.storedItems.length === 0) ? (
                                            <p className="text-xs text-gray-400 italic py-1">No items currently stored here</p>
                                        ) : (
                                            <table className="w-full text-sm">
                                                <thead>
                                                    <tr className="text-xs text-gray-500 uppercase">
                                                        <th className="text-left py-1 font-medium">Type</th>
                                                        <th className="text-left py-1 font-medium">Code</th>
                                                        <th className="text-left py-1 font-medium">Name</th>
                                                        <th className="text-right py-1 font-medium">Current</th>
                                                        <th className="text-right py-1 font-medium">Reserved</th>
                                                        <th className="text-right py-1 font-medium">Available</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {loc.storedItems.map((item, idx) => (
                                                        <tr key={idx} className="border-t border-gray-100">
                                                            <td className="py-2">
                                                                {item.itemType === 'Product' ? (
                                                                    <span className="flex items-center gap-1 text-purple-600">
                                                                        <Package size={13} /> FG
                                                                    </span>
                                                                ) : (
                                                                    <span className="flex items-center gap-1 text-blue-600">
                                                                        <Box size={13} /> RM
                                                                    </span>
                                                                )}
                                                            </td>
                                                            <td className="py-2 font-mono text-xs">{item.itemCode}</td>
                                                            <td className="py-2">{item.itemName}</td>
                                                            <td className="py-2 text-right font-medium">{item.currentStock}</td>
                                                            <td className="py-2 text-right text-orange-600">{item.reservedStock}</td>
                                                            <td className="py-2 text-right text-green-600 font-bold">{item.availableStock}</td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                            </table>
                                        )}
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

const Warehouses = () => {
    const [layoutWarehouse, setLayoutWarehouse] = useState(null);
    const [loading, setLoading] = useState(false);

    const handleViewLayout = async (row) => {
        setLoading(true);
        try {
            const data = await getWarehouseById(row.id);
            setLayoutWarehouse(data);
        } catch (err) {
            console.error('Failed to fetch warehouse details:', err);
        } finally {
            setLoading(false);
        }
    };

    const customActions = [
        {
            icon: Eye,
            label: 'View Layout',
            onClick: handleViewLayout,
            className: 'text-blue-600 hover:text-blue-800',
        },
    ];

    return (
        <>
            <MasterCRUDPage
                title="Warehouses"
                icon={Building2}
                columns={columns}
                formFields={formFields}
                fetchAll={getWarehouses}
                createFn={createWarehouse}
                updateFn={updateWarehouse}
                deleteFn={deleteWarehouse}
                idKey="id"
                customActions={customActions}
            />
            {layoutWarehouse && (
                <WarehouseLayoutModal
                    warehouse={layoutWarehouse}
                    onClose={() => setLayoutWarehouse(null)}
                />
            )}
        </>
    );
};

export default Warehouses;
