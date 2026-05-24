import React from 'react';
import { Ruler } from 'lucide-react';
import MasterCRUDPage from '../../components/common/MasterCRUDPage';
import { getUnits, createUnit, updateUnit, deleteUnit } from '../../api/inventoryService';

const columns = [
    { key: 'unitCode', label: 'Code' },
    { key: 'unitName', label: 'Name' },
    { key: 'description', label: 'Description' },
    { key: 'isActive', label: 'Active', render: (v) => (
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${v ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
            {v ? 'Active' : 'Inactive'}
        </span>
    )},
];

const formFields = [
    { key: 'unitCode', label: 'Unit Code', required: true },
    { key: 'unitName', label: 'Unit Name', required: true },
    { key: 'description', label: 'Description' },
];

const Units = () => (
    <MasterCRUDPage
        title="Units of Measure"
        icon={Ruler}
        columns={columns}
        formFields={formFields}
        fetchAll={getUnits}
        createFn={createUnit}
        updateFn={updateUnit}
        deleteFn={deleteUnit}
        idKey="id"
    />
);

export default Units;
