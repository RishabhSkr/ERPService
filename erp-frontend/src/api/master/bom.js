import { productionApi } from '../axios';

// ====================================================================
// BOM API — matches BOMsController (Production Service!)
// Base: /api/production/boms
// ====================================================================

// GET /api/production/boms — returns ApiResponse<IEnumerable<BOMDto>>
export const getBOMsService = async () => {
    const response = await productionApi.get('/production/boms');
    return response.data;
};

// GET /api/production/boms/{id} — returns ApiResponse<BOMDto> (by BOM ID)
export const getBOMByIdService = async (bomId) => {
    const response = await productionApi.get(`/production/boms/${bomId}`);
    return response.data;
};

// GET /api/production/boms/product/{productId} — returns ApiResponse<BOMDto> (active BOM for product)
export const getBOMByProductIdService = async (productId) => {
    const response = await productionApi.get(`/production/boms/product/${productId}`);
    return response.data;
};

// POST /api/production/boms — CreateBOMDto { productId, bomCode, productName, description, lines[] }
export const createBOMService = async (data) => {
    const response = await productionApi.post('/production/boms', data);
    return response.data;
};

// PUT /api/production/boms/{bomId} — UpdateBOMDto { description, lines[] }
export const updateBOMService = async (bomId, data) => {
    const response = await productionApi.put(`/production/boms/${bomId}`, data);
    return response.data;
};

// DELETE /api/production/boms/{bomId} — deactivate
export const deleteBOMService = async (bomId) => {
    const response = await productionApi.delete(`/production/boms/${bomId}`);
    return response.data;
};
