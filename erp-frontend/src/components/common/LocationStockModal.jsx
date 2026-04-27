import React from 'react';
import { X, MapPin, Package } from 'lucide-react';

/**
 * LocationStockModal — Shows location-wise stock breakdown for a Raw Material or Product
 * 
 * Props:
 *   isOpen: boolean
 *   onClose: () => void
 *   item: { materialCode/productCode, materialName/productName, defaultStorageLocationCode, locationStocks, currentStock, reservedStock, availableStock }
 *   itemType: 'RawMaterial' | 'Product'
 */
const LocationStockModal = ({ isOpen, onClose, item, itemType }) => {
    if (!isOpen || !item) return null;

    const code = itemType === 'RawMaterial' ? item.materialCode : item.productCode;
    const name = itemType === 'RawMaterial' ? item.materialName : item.productName;
    const locations = item.locationStocks || [];

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-2xl max-h-[80vh] flex flex-col">
                {/* Header */}
                <div className="flex items-center justify-between p-5 border-b">
                    <div>
                        <h2 className="text-lg font-bold text-slate-800 flex items-center gap-2">
                            <MapPin className="text-purple-500" size={20} /> Stock by Location
                        </h2>
                        <p className="text-sm text-slate-500 mt-0.5">
                            <span className="font-mono font-semibold text-slate-700">{code}</span>
                            <span className="ml-2">{name}</span>
                        </p>
                    </div>
                    <button onClick={onClose} className="p-1.5 rounded-lg hover:bg-slate-100">
                        <X size={20} />
                    </button>
                </div>

                {/* Summary Bar */}
                <div className="px-5 py-3 bg-slate-50 border-b flex items-center gap-6 text-sm">
                    <div>
                        <span className="text-slate-500">Total Stock: </span>
                        <span className="font-bold text-slate-800">{item.currentStock ?? 0}</span>
                    </div>
                    <div>
                        <span className="text-slate-500">Reserved: </span>
                        <span className="font-bold text-orange-600">{item.reservedStock ?? 0}</span>
                    </div>
                    <div>
                        <span className="text-slate-500">Available: </span>
                        <span className="font-bold text-green-600">{item.availableStock ?? 0}</span>
                    </div>
                    <div className="ml-auto">
                        <span className="text-slate-500">Default Location: </span>
                        <span className="font-bold text-purple-600">
                            {item.defaultStorageLocationCode || '— Not Set —'}
                        </span>
                    </div>
                </div>

                {/* Table */}
                <div className="flex-1 overflow-auto p-5">
                    {locations.length === 0 ? (
                        <div className="text-center py-10 text-slate-400">
                            <Package size={40} className="opacity-20 mx-auto mb-2" />
                            <p>No stock in any location</p>
                        </div>
                    ) : (
                        <table className="w-full">
                            <thead className="bg-slate-50 border-b border-slate-200">
                                <tr>
                                    <th className="text-left px-4 py-2.5 text-xs font-semibold text-slate-500 uppercase">Location Code</th>
                                    <th className="text-left px-4 py-2.5 text-xs font-semibold text-slate-500 uppercase">Warehouse</th>
                                    <th className="text-right px-4 py-2.5 text-xs font-semibold text-slate-500 uppercase">Current</th>
                                    <th className="text-right px-4 py-2.5 text-xs font-semibold text-slate-500 uppercase">Reserved</th>
                                    <th className="text-right px-4 py-2.5 text-xs font-semibold text-slate-500 uppercase">Available</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-slate-100">
                                {locations.map((loc, idx) => {
                                    const isDefault = item.defaultStorageLocationId === loc.storageLocationId;
                                    return (
                                        <tr key={loc.storageLocationId || idx} 
                                            className={`hover:bg-slate-50 transition-colors ${isDefault ? 'bg-purple-50/50' : ''}`}>
                                            <td className="px-4 py-3 text-sm font-mono font-semibold text-slate-700">
                                                {loc.locationCode}
                                                {isDefault && (
                                                    <span className="ml-2 px-1.5 py-0.5 bg-purple-100 text-purple-700 text-[10px] rounded-full font-medium">
                                                        DEFAULT
                                                    </span>
                                                )}
                                            </td>
                                            <td className="px-4 py-3 text-sm text-slate-600">{loc.warehouseName}</td>
                                            <td className="px-4 py-3 text-sm text-right font-medium text-slate-700">
                                                {loc.currentStock?.toLocaleString('en-IN')}
                                            </td>
                                            <td className="px-4 py-3 text-sm text-right text-orange-600">
                                                {loc.reservedStock?.toLocaleString('en-IN')}
                                            </td>
                                            <td className="px-4 py-3 text-sm text-right">
                                                <span className={`px-2.5 py-1 rounded-full font-bold text-xs border ${
                                                    loc.availableStock === 0 ? 'bg-red-100 text-red-700 border-red-200' :
                                                    loc.availableStock < 10 ? 'bg-yellow-100 text-yellow-700 border-yellow-200' :
                                                    'bg-green-100 text-green-700 border-green-200'
                                                }`}>
                                                    {loc.availableStock?.toLocaleString('en-IN')}
                                                </span>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    )}
                </div>

                {/* Footer */}
                <div className="px-5 py-3 border-t bg-slate-50 text-xs text-slate-400 flex justify-between">
                    <span>{locations.length} location{locations.length !== 1 ? 's' : ''} with stock</span>
                    <span>Use Stock Transfer to move inventory between locations</span>
                </div>
            </div>
        </div>
    );
};

export default LocationStockModal;
