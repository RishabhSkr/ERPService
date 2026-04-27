import React from 'react';
import { Layers } from 'lucide-react';
import MasterCRUDPage from '../../components/common/MasterCRUDPage';
import { getStorageLocationTypes, createStorageLocationType, updateStorageLocationType, deleteStorageLocationType } from '../../api/inventoryService';

const columns = [
    { key: 'typeCode', label: 'Type Code' },
    { key: 'typeName', label: 'Type Name' },
    { key: 'allowRawMaterials', label: 'Allow Raw Materials', render: (v) => (
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${v ? 'bg-blue-100 text-blue-700' : 'bg-gray-100 text-gray-500'}`}>
            {v ? 'Yes' : 'No'}
        </span>
    )},
    { key: 'allowProducts', label: 'Allow Products', render: (v) => (
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${v ? 'bg-purple-100 text-purple-700' : 'bg-gray-100 text-gray-500'}`}>
            {v ? 'Yes' : 'No'}
        </span>
    )},
    { key: 'isActive', label: 'Active', render: (v) => (
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${v ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
            {v ? 'Active' : 'Inactive'}
        </span>
    )},
];

const formFields = [
    { key: 'typeCode', label: 'Type Code', required: true, placeholder: 'e.g., RM, FG, CHEM' },
    { key: 'typeName', label: 'Type Name', required: true, placeholder: 'e.g., Raw Materials, Finished Goods' },
    { key: 'allowRawMaterials', label: 'Allow Raw Materials', type: 'checkbox' },
    { key: 'allowProducts', label: 'Allow Finished Goods', type: 'checkbox' },
];

const StorageLocationTypes = () => (
    <MasterCRUDPage
        title="Storage Location Types"
        icon={Layers}
        columns={columns}
        formFields={formFields}
        fetchAll={getStorageLocationTypes}
        createFn={createStorageLocationType}
        updateFn={updateStorageLocationType}
        deleteFn={deleteStorageLocationType}
        idKey="id"
        dataPath="data"
    />
);

export default StorageLocationTypes;
