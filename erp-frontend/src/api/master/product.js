import { inventoryApi } from '../axios';

// ====================================================================
// Products API — matches ProductsController
// Base: /api/inventory/products
//
// GET  /products?pageNumber=1&pageSize=100 → ApiResponse<PagedResponse<ProductListDto>>
// GET  /products/{id} → ApiResponse<ProductResponseDto>
// POST /products → CreateProductDto
// PUT  /products/{id} → UpdateProductDto { productName?, description?, price?, minStockLevel?, isActive? }
// DEL  /products/{id}
// PATCH /products/{id}/restore
// GET  /products/{id}/check-availability?quantity=100
// POST /products/{id}/add-stock → { warehouseId, quantity, batchNumber }
// ====================================================================

export const getProducts = async (params = {}) => {
    const response = await inventoryApi.get('/inventory/products', {
        params: { pageNumber: 1, pageSize: 100, ...params },
    });
    return response.data;
};

export const getProductById = async id => {
    const response = await inventoryApi.get(`/inventory/products/${id}`);
    return response.data;
};

export const createProduct = async data => {
    const response = await inventoryApi.post('/inventory/products', data);
    return response.data;
};

export const updateProduct = async (id, data) => {
    const response = await inventoryApi.put(`/inventory/products/${id}`, data);
    return response.data;
};

export const deleteProduct = async id => {
    const response = await inventoryApi.delete(`/inventory/products/${id}`);
    return response.data;
};

export const restoreProduct = async id => {
    const response = await inventoryApi.patch(
        `/inventory/products/${id}/restore`
    );
    return response.data;
};

export const checkAvailability = async (id, quantity) => {
    const response = await inventoryApi.get(
        `/inventory/products/${id}/check-availability`,
        {
            params: { quantity },
        }
    );
    return response.data;
};
