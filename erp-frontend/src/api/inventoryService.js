import { inventoryApi } from './axios';

// ====================================================================
// Inventory Master APIs — Categories, Units, Stock Movements
// ====================================================================

// --- Categories ---
export const getCategories = () => inventoryApi.get('/inventory/categories');
export const getCategoryById = (id) => inventoryApi.get(`/inventory/categories/${id}`);
export const createCategory = (data) => inventoryApi.post('/inventory/categories', data);
export const updateCategory = (id, data) => inventoryApi.put(`/inventory/categories/${id}`, data);
export const deleteCategory = (id) => inventoryApi.delete(`/inventory/categories/${id}`);
export const restoreCategory = (id) => inventoryApi.patch(`/inventory/categories/${id}/restore`);

// --- Units ---
export const getUnits = () => inventoryApi.get('/inventory/units');
export const getUnitById = (id) => inventoryApi.get(`/inventory/units/${id}`);
export const createUnit = (data) => inventoryApi.post('/inventory/units', data);
export const updateUnit = (id, data) => inventoryApi.put(`/inventory/units/${id}`, data);
export const deleteUnit = (id) => inventoryApi.delete(`/inventory/units/${id}`);
export const restoreUnit = (id) => inventoryApi.patch(`/inventory/units/${id}/restore`);

// --- Stock Movements ---
export const getStockMovements = (params = {}) => inventoryApi.get('/inventory/stock-movements', { params });
export const getStockMovementById = (id) => inventoryApi.get(`/inventory/stock-movements/${id}`);
export const getMovementsByItem = (itemType, itemId) => inventoryApi.get(`/inventory/stock-movements/item/${itemType}/${itemId}`);
export const recordStockMovement = (data) => inventoryApi.post('/inventory/stock-movements/record', data);
