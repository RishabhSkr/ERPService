import { inventoryApi } from '../axios';

export const getStorageLocations = async () => {
    const response = await inventoryApi.get('/inventory/storagelocations');
    return response.data;
};

export const getStorageLocationById = async (id) => {
    const response = await inventoryApi.get(`/inventory/storagelocations/${id}`);
    return response.data;
};

export const getStorageLocationsByWarehouse = async (warehouseId) => {
    const response = await inventoryApi.get(`/inventory/storagelocations/warehouse/${warehouseId}`);
    return response.data;
};

export const createStorageLocation = async (data) => {
    const response = await inventoryApi.post('/inventory/storagelocations', data);
    return response.data;
};

export const updateStorageLocation = async (id, data) => {
    const response = await inventoryApi.put(`/inventory/storagelocations/${id}`, data);
    return response.data;
};

export const deleteStorageLocation = async (id) => {
    const response = await inventoryApi.delete(`/inventory/storagelocations/${id}`);
    return response.data;
};
