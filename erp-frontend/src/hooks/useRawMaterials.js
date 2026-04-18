import { useState, useEffect, useCallback } from 'react';
import { getRawMaterials, 
        createRawMaterial,
        updateRawMaterial,
        deleteRawMaterial, 
        restoreRawMaterial,
        addStock } from '../api/master/rawMaterial';

import useApi from './useApi';

export const useRawMaterials = () => {
    const [materials, setMaterials] = useState([]);
    const {loading, requestHandlerFunction} = useApi();

    // Fetch — unwrap ApiResponse<PagedResponse>
    // Response: { success, data: { data: [...], pageNumber, pageSize, totalRecords } }
    const fetchMaterials = useCallback(async () => {
        const result = await requestHandlerFunction(getRawMaterials); 
        if (result.success) {
            // result.data is PagedResponse: { data: [...items], totalRecords, ... }
            const pagedData = result.data?.data || result.data || {};
            const items = pagedData?.data || (Array.isArray(pagedData) ? pagedData : []);
            setMaterials(Array.isArray(items) ? items : []);
        }
    }, [requestHandlerFunction]);

    // Create
    const addMaterial = async (data) => {
        const result = await requestHandlerFunction(() => createRawMaterial(data), "Material Added Successfully!");
        if (result.success) fetchMaterials(); 
        return result.success;
    };

    // Add Stock — POST /{id}/add-stock { warehouseId, quantity, batchNumber }
    const addRawMaterialStock = async (id, data) => {
        const result = await requestHandlerFunction(() => addStock(id, data), "Stock Updated Successfully!");
        if (result.success) fetchMaterials(); 
        return result.success;
    };

    // Update
    const updateRM = async (id, data) => {
        const result = await requestHandlerFunction(() => updateRawMaterial(id, data), "Material Updated Successfully!");
        if (result.success) fetchMaterials(); 
        return result.success;
    };

    // Delete (soft)
    const deleteRM = async (id) => {
        const result = await requestHandlerFunction(() => deleteRawMaterial(id), "Material Deleted!");
        if (result.success) fetchMaterials(); 
        return result.success;
    };

    // Restore
    const restoreRM = async (id) => {
        const result = await requestHandlerFunction(() => restoreRawMaterial(id), "Material Restored!");
        if (result.success) fetchMaterials(); 
        return result.success;
    };

    // Initial Load
    useEffect(() => {
        fetchMaterials();
    }, [fetchMaterials]);

    return { materials, loading, addMaterial, addRawMaterialStock, updateRM, deleteRM, restoreRM, fetchMaterials };
};