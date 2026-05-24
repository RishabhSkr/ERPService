import React, { useState, useEffect } from 'react';
import { MapPin } from 'lucide-react';
import MasterCRUDPage from '../../components/common/MasterCRUDPage';
import { getStorageLocations, createStorageLocation, updateStorageLocation, deleteStorageLocation } from '../../api/master/storageLocation';
import { getWarehouses } from '../../api/master/warehouse';
import { getStorageLocationTypes } from '../../api/inventoryService';

const unwrapList = (res) => {
    if (Array.isArray(res)) return res;
    if (res?.data && Array.isArray(res.data)) return res.data;
    if (res?.data?.data && Array.isArray(res.data.data)) return res.data.data;
    return [];
};

const StorageLocations = () => {
    const [warehouses, setWarehouses] = useState([]);
    const [locationTypes, setLocationTypes] = useState([]);

    useEffect(() => {
        getWarehouses().then(res => {
            const data = unwrapList(res);
            setWarehouses(data.map(w => ({ value: w.id, label: w.warehouseName })));
        }).catch(console.error);

        getStorageLocationTypes().then(res => {
            const data = unwrapList(res);
            setLocationTypes(data.map(t => ({ value: t.id, label: `${t.typeCode} - ${t.typeName}` })));
        }).catch(console.error);
    }, []);

    const columns = [
        { key: 'locationCode', label: 'Location Code' },
        { key: 'warehouseName', label: 'Warehouse' },
        { key: 'locationTypeName', label: 'Location Type', render: (v) => (
            <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                v === 'Raw Materials' ? 'bg-blue-100 text-blue-700' :
                v === 'Finished Goods' ? 'bg-purple-100 text-purple-700' :
                'bg-gray-100 text-gray-600'
            }`}>
                {v || 'Not Set'}
            </span>
        )},
        { key: 'zone', label: 'Zone' },
        { key: 'aisle', label: 'Aisle' },
        { key: 'rack', label: 'Rack' },
        { key: 'bin', label: 'Bin' }
    ];

    const formFields = [
        { key: 'warehouseId', label: 'Warehouse', type: 'select', options: warehouses, required: true },
        { key: 'locationTypeId', label: 'Location Type', type: 'select', options: locationTypes },
        { key: 'zone', label: 'Zone', required: true },
        { key: 'aisle', label: 'Aisle' },
        { key: 'rack', label: 'Rack' },
        { key: 'bin', label: 'Bin' },
    ];

    return (
        <MasterCRUDPage
            title="Storage Locations"
            icon={MapPin}
            columns={columns}
            formFields={formFields}
            fetchAll={getStorageLocations}
            createFn={createStorageLocation}
            updateFn={updateStorageLocation}
            deleteFn={deleteStorageLocation}
            idKey="id"
        />
    );
};

export default StorageLocations;
