import { useState, useEffect, useCallback } from 'react';
import { 
    getBOMsService, 
    createBOMService, 
    updateBOMService, 
    deleteBOMService,
    getBOMByProductIdService
} from '../api/master/bom';

import useApi from './useApi';

export const useBom = () => {
    const [boms, setBoms] = useState([]); 
    const {loading, requestHandlerFunction} = useApi();

    // Get All BOMs — unwrap ApiResponse
    const fetchBoms = useCallback(async () => {
        const result = await requestHandlerFunction(getBOMsService); 
        if (result.success) {
            // API returns { success, data: { data: [...] } } or { data: [...] }
            const list = result.data?.data || result.data || [];
            setBoms(Array.isArray(list) ? list : []);
        }
        return result.success;
    }, [requestHandlerFunction]);

    // Get BOM By Product Id — unwrap ApiResponse
    const getBOMByProductId = useCallback(async (productId) => {
        try {
            const response = await getBOMByProductIdService(productId);
            // response is already unwrapped by axios interceptor: { success, data: <BOMDto> }
            const bom = response?.data || response;
            return bom;
        } catch (error) {
            console.error("Error fetching BOM:", error);
            return null;
        }
    }, []);

    // Create BOM
    const createBOM = async (data) => {
        const result = await requestHandlerFunction(
            () => createBOMService(data), 
            "BOM Created Successfully!"
        );
        if (result.success) fetchBoms();
        return result.success;
    };

    // Delete BOM (by BOM ID, not product ID)
    const deleteBOM = async (bomId) => {
        const result = await requestHandlerFunction(
            () => deleteBOMService(bomId), 
            "BOM Deleted"
        );
        if (result.success) fetchBoms();
        return result.success;
    };

    // Update BOM (by BOM ID)
    const updateBOM = async (bomId, data) => {
        const result = await requestHandlerFunction(
            () => updateBOMService(bomId, data), 
            "BOM Updated Successfully!"
        );
        if (result.success) fetchBoms();
        return result.success;
    }

    // Initial Load
    useEffect(() => {
        const loadingBom = async () => await fetchBoms();
        loadingBom();
    }, [fetchBoms]);

    return { 
        boms, 
        loading, 
        createBOM, 
        deleteBOM,
        updateBOM,
        fetchBoms,
        getBOMByProductId 
    };
};