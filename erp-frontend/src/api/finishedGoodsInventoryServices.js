import { inventoryApi } from './axios';

// ====================================================================
// Finished Goods = Products with stock info from Inventory
// GET /products returns PagedResponse<ProductListDto>
// ====================================================================

export const getFinishedGoodsStock = async () => {
    const response = await inventoryApi.get('/inventory/products', {
        params: { pageNumber: 1, pageSize: 100 }
    });
    return response.data;
};
