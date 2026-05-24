import { inventoryApi } from '../axios';

// ====================================================================
// Raw Materials API — matches RawMaterialsController
// Base: /api/inventory/raw-materials
//
// GET  /raw-materials?pageNumber=1&pageSize=100 → ApiResponse<PagedResponse<RawMaterialListDto>>
// GET  /raw-materials/{id} → ApiResponse<RawMaterialResponseDto>
// POST /raw-materials → CreateRawMaterialDto
// PUT  /raw-materials/{id} → UpdateRawMaterialDto { materialName?, description?, cost?, minStockLevel?, supplier?, isActive? }
// DEL  /raw-materials/{id}
// PATCH /raw-materials/{id}/restore
// POST /raw-materials/{id}/add-stock → { warehouseId, quantity, batchNumber }
// POST /raw-materials/reserve → ReserveRawMaterialsDto
// POST /raw-materials/release → ReleaseRawMaterialsDto
// ====================================================================

export const getRawMaterials = async (params = {}) => {
    const response = await inventoryApi.get('/inventory/raw-materials', {
        params: { pageNumber: 1, pageSize: 100, ...params }
    });
    return response.data;
};

export const getRawMaterialById = async (id) => {
    const response = await inventoryApi.get(`/inventory/raw-materials/${id}`);
    return response.data;
};

export const createRawMaterial = async (data) => {
    const response = await inventoryApi.post('/inventory/raw-materials', data);
    return response.data;
};

export const updateRawMaterial = async (id, data) => {
    const response = await inventoryApi.put(`/inventory/raw-materials/${id}`, data);
    return response.data;
};

export const deleteRawMaterial = async (id) => {
    const response = await inventoryApi.delete(`/inventory/raw-materials/${id}`);
    return response.data;
};

export const restoreRawMaterial = async (id) => {
    const response = await inventoryApi.patch(`/inventory/raw-materials/${id}/restore`);
    return response.data;
};

// POST /raw-materials/{id}/add-stock → { warehouseId, quantity, batchNumber }
export const addStock = async (id, data) => {
    const response = await inventoryApi.post(`/inventory/raw-materials/${id}/add-stock`, data);
    return response.data;
};

// POST /raw-materials/reserve
export const reserveRawMaterials = async (data) => {
    const response = await inventoryApi.post('/inventory/raw-materials/reserve', data);
    return response.data;
};

// POST /raw-materials/release
export const releaseRawMaterials = async (data) => {
    const response = await inventoryApi.post('/inventory/raw-materials/release', data);
    return response.data;
};
