import { salesApi } from './axios';

// ====================================================================
// Sales Orders API — matches SalesOrdersController
// Base: http://localhost:5002/api/sales/orders
// ====================================================================

// GET /sales/orders?pageNumber=1&pageSize=10&status=&customerId=
export const getOrders = (pageNumber = 1, pageSize = 10, status = '', customerId = '') => {
    const params = { pageNumber, pageSize };
    if (status) params.status = status;
    if (customerId) params.customerId = customerId;
    return salesApi.get('/sales/orders', { params });
};
// GET /sales/orders/{id}
export const getOrderById = (id) => salesApi.get(`/sales/orders/${id}`);

// POST /sales/orders
export const createOrder = (data) => salesApi.post('/sales/orders', data);

// PATCH /sales/orders/{id}/status
export const updateOrderStatus = (id, data) => salesApi.patch(`/sales/orders/${id}/status`, data);

// DELETE /sales/orders/{id}?reason=
export const cancelOrder = (id, reason = '') => 
    salesApi.delete(`/sales/orders/${id}`, { params: { reason } });

// POST /sales/orders/{id}/dispatch
export const dispatchItems = (id, data) => salesApi.post(`/sales/orders/${id}/dispatch`, data);

// PATCH /sales/orders/{id}/deliver
export const markDelivered = (id) => salesApi.patch(`/sales/orders/${id}/deliver`);

// GET /sales/orders/fulfillment-dashboard
export const getFulfillmentDashboard = () => salesApi.get('/sales/orders/fulfillment-dashboard');


// ====================================================================
// Customers API — matches CustomersController
// Base: http://localhost:5002/api/sales/customers
// ====================================================================

// GET /sales/customers
export const getCustomers = (pageNumber = 1, pageSize = 50) => 
    salesApi.get('/sales/customers', { params: { pageNumber, pageSize } });

// GET /sales/customers/{id}
export const getCustomerById = (id) => salesApi.get(`/sales/customers/${id}`);
