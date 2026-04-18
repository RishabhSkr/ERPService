import React from 'react';
import { Building2 } from 'lucide-react';
import MasterCRUDPage from '../../components/common/MasterCRUDPage';
import { getWorkCenters, createWorkCenter, updateWorkCenter, deleteWorkCenter } from '../../api/productionService';

const columns = [
    { key: 'centerCode', label: 'Code' },
    { key: 'centerName', label: 'Name' },
    { key: 'location', label: 'Location' },
    { key: 'costPerHour', label: 'Cost/Hr', render: (v) => `₹${v?.toLocaleString('en-IN')}` },
    { key: 'capacityPerHour', label: 'Capacity/Hr' },
    { key: 'description', label: 'Description' },
];

const formFields = [
    { key: 'centerCode', label: 'Center Code', required: true },
    { key: 'centerName', label: 'Center Name', required: true },
    { key: 'location', label: 'Location', required: true },
    { key: 'costPerHour', label: 'Cost Per Hour (₹)', type: 'number', required: true },
    { key: 'capacityPerHour', label: 'Capacity Per Hour', type: 'number', required: true },
    { key: 'description', label: 'Description' },
];

const WorkCenters = () => (
    <MasterCRUDPage
        title="Work Centers"
        icon={Building2}
        columns={columns}
        formFields={formFields}
        fetchAll={getWorkCenters}
        createFn={createWorkCenter}
        updateFn={updateWorkCenter}
        deleteFn={deleteWorkCenter}
        idKey="workCenterId"
    />
);

export default WorkCenters;
