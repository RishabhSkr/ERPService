import React from 'react';
import { FolderTree } from 'lucide-react';
import MasterCRUDPage from '../../components/common/MasterCRUDPage';
import { getCategories, createCategory, updateCategory, deleteCategory } from '../../api/inventoryService';

const columns = [
    { key: 'categoryCode', label: 'Code' },
    { key: 'categoryName', label: 'Name' },
    { key: 'description', label: 'Description' },
    { key: 'isActive', label: 'Active', render: (v) => (
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${v ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
            {v ? 'Active' : 'Inactive'}
        </span>
    )},
];

const formFields = [
    { key: 'categoryCode', label: 'Category Code', required: true },
    { key: 'categoryName', label: 'Category Name', required: true },
    { key: 'description', label: 'Description' },
];

const Categories = () => (
    <MasterCRUDPage
        title="Categories"
        icon={FolderTree}
        columns={columns}
        formFields={formFields}
        fetchAll={getCategories}
        createFn={createCategory}
        updateFn={updateCategory}
        deleteFn={deleteCategory}
        idKey="id"
    />
);

export default Categories;
