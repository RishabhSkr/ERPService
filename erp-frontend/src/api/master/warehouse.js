import { inventoryApi } from '../axios';

export const getWarehouses = async () => {
    const response = await inventoryApi.get('/inventory/warehouses');
    return response.data;
};

export const getWarehouseById = async (id) => {
    const response = await inventoryApi.get(`/inventory/warehouses/${id}`);
    return response.data;
};

export const createWarehouse = async (data) => {
    const response = await inventoryApi.post('/inventory/warehouses', data);
    return response.data;
};

export const updateWarehouse = async (id, data) => {
    const response = await inventoryApi.put(`/inventory/warehouses/${id}`, data);
    return response.data;
};

export const deleteWarehouse = async (id) => {
    const response = await inventoryApi.delete(`/inventory/warehouses/${id}`);
    return response.data;
};
