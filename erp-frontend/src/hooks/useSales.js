import { useState, useCallback } from 'react';
import { 
    getOrders,
    getOrderById, 
    updateOrderStatus, 
    cancelOrder,
    dispatchItems,
    markDelivered, 
    getFulfillmentDashboard, 
} from '../api/salesOrderService';
import useApi from './useApi';

/**
 * useSales Hook — Central hook for Sales module
 * 
 * Returns:
 *  - orders[]        → list of orders
 *  - orderDetail     → single order with items (getOrderById)
 *  - loading, error
 *  - fetchOrders(page, pageSize, status)
 *  - fetchOrderById(id)
 *  - confirmOrder(id), handleCancel(id, reason)
 *  - handleDispatch(id, data), handleDeliver(id)
 *  - fetchFulfillment(), fulfillment
 */
export const useSales = () => {
    const [orders, setOrders] = useState([]);
    const [orderDetail, setOrderDetail] = useState(null);
    const [fulfillment, setFulfillment] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const { requestHandlerFunction } = useApi();

    // ─── Fetch All Orders ───
    const fetchOrders = useCallback(async (page = 1, pageSize = 10, status = '') => {
        setLoading(true);
        setError(null);
        try {
            const response = await requestHandlerFunction(
                () => getOrders(page, pageSize, status)
            );
            if (response.success) {
                // useApi wraps: { success, data: axiosResponse }
                // axiosResponse.data = { data: { data: [...], totalPages }, success, message }
                const backendWrapper = response.data?.data; // { data: {...}, success, message }
                setOrders(backendWrapper?.data?.data || backendWrapper?.data || []);
            }
        } catch (err) {
            setError(err);
        } finally {
            setLoading(false);
        }
    }, [requestHandlerFunction]);

    // ─── Fetch Single Order (with items) ───
    const fetchOrderById = useCallback(async (id) => {
        if (!id) return null;
        setLoading(true);
        setError(null);
        try {
            const response = await requestHandlerFunction(
                () => getOrderById(id)
            );
            if (response.success) {
                // axiosResponse.data = { data: { id, orderNumber, items: [...] }, success }
                const detail = response.data?.data?.data;
                setOrderDetail(detail);
                return detail;
            }
        } catch (err) {
            setError(err);
        } finally {
            setLoading(false);
        }
        return null;
    }, [requestHandlerFunction]);

    // ─── Confirm Order ───
    const confirmOrder = useCallback(async (id) => {
        const response = await requestHandlerFunction(
            () => updateOrderStatus(id, { status: 'Confirmed', notes: 'Confirmed' }),
            'Order Confirmed!'
        );
        return response.success;
    }, [requestHandlerFunction]);

    // ─── Cancel Order ───
    const handleCancel = useCallback(async (id, reason = 'Cancelled by user') => {
        const response = await requestHandlerFunction(
            () => cancelOrder(id, reason),
            'Order Cancelled'
        );
        return response.success;
    }, [requestHandlerFunction]);

    // ─── Dispatch Items ───
    const handleDispatch = useCallback(async (id, data) => {
        const response = await requestHandlerFunction(
            () => dispatchItems(id, data),
            'Items Dispatched!'
        );
        return response.success;
    }, [requestHandlerFunction]);

    // ─── Mark Delivered ───
    const handleDeliver = useCallback(async (id) => {
        const response = await requestHandlerFunction(
            () => markDelivered(id),
            'Marked as Delivered!'
        );
        return response.success;
    }, [requestHandlerFunction]);

    // ─── Fulfillment Dashboard ───
    const fetchFulfillment = useCallback(async () => {
        setLoading(true);
        try {
            const response = await requestHandlerFunction(
                () => getFulfillmentDashboard()
            );
            if (response.success) {
                setFulfillment(response.data?.data?.data);
            }
        } catch (err) {
            setError(err);
        } finally {
            setLoading(false);
        }
    }, [requestHandlerFunction]);

    return {
        // State
        orders,
        orderDetail,
        fulfillment,
        loading,
        error,
        // Actions
        fetchOrders,
        fetchOrderById,
        confirmOrder,
        handleCancel,
        handleDispatch,
        handleDeliver,
        fetchFulfillment,
        // Setters
        setOrderDetail,
    };
};