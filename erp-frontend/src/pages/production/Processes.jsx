import React from 'react';
import { Cog } from 'lucide-react';
import MasterCRUDPage from '../../components/common/MasterCRUDPage';
import { getProcesses, createProcess, updateProcess, deleteProcess } from '../../api/productionService';

const columns = [
    { key: 'processCode', label: 'Code' },
    { key: 'processName', label: 'Name' },
    { key: 'category', label: 'Category', render: (v) => (
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
            v === 'Raw' ? 'bg-orange-100 text-orange-700' :
            v === 'Assembly' ? 'bg-blue-100 text-blue-700' :
            'bg-green-100 text-green-700'
        }`}>{v}</span>
    )},
    { key: 'standardTimeMinutes', label: 'Std Time (min)' },
    { key: 'description', label: 'Description' },
];

const formFields = [
    { key: 'processCode', label: 'Process Code', required: true },
    { key: 'processName', label: 'Process Name', required: true },
    { key: 'category', label: 'Category', type: 'select', required: true, options: [
        { value: 'Raw', label: 'Raw' },
        { value: 'Assembly', label: 'Assembly' },
        { value: 'Finishing', label: 'Finishing' },
    ]},
    { key: 'standardTimeMinutes', label: 'Standard Time (minutes)', type: 'number', required: true },
    { key: 'description', label: 'Description' },
];

const Processes = () => (
    <MasterCRUDPage
        title="Processes"
        icon={Cog}
        columns={columns}
        formFields={formFields}
        fetchAll={getProcesses}
        createFn={createProcess}
        updateFn={updateProcess}
        deleteFn={deleteProcess}
        idKey="processId"
    />
);

export default Processes;
