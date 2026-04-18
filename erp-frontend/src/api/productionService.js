import { productionApi } from './axios';

// ====================================================================
// Pending Requests API — matches PendingRequestsController
// Base: /api/production/pending-requests
// ====================================================================

export const getPendingRequests = () => productionApi.get('/production/pending-requests');
export const getPendingOnly = () => productionApi.get('/production/pending-requests/pending');
export const getPendingRequestById = (id) => productionApi.get(`/production/pending-requests/${id}`);
export const approveRequest = (id, data) => productionApi.post(`/production/pending-requests/${id}/approve`, data);
export const cancelRequest = (id, data) => productionApi.post(`/production/pending-requests/${id}/cancel`, data);


// ====================================================================
// Production Orders API — matches ProductionOrdersController
// Base: /api/production/orders
// postman request 'http://localhost:5006/api/production/orders'
// ====================================================================

export const getAllOrders = () => productionApi.get('/production/orders');
export const getAllPendingOrders = () => productionApi.get('/production/pending-requests/');
export const getOrdersByStatus = (status) => productionApi.get(`/production/orders/status/${status}`);
export const getOrderById = (id) => productionApi.get(`/production/orders/${id}`);
export const createOrder = (data) => productionApi.post('/production/orders', data);
export const startOrder = (id) => productionApi.patch(`/production/orders/${id}/start`);
export const completeOrder = (id, data) => productionApi.post(`/production/orders/${id}/complete`, data);
export const cancelOrder = (id, reason) => productionApi.delete(`/production/orders/${id}`, { params: { reason } });
export const releaseOrder = (id) => productionApi.patch(`/production/orders/${id}/release`);
export const retryReservation = (id) => productionApi.post(`/production/orders/${id}/retry-reservation`);
export const updateProgress = (id, data) => productionApi.put(`/production/orders/${id}/progress`, data);


// ====================================================================
// Work Orders API — matches WorkOrdersController
// Base: /api/production/work-orders
// ====================================================================

// Dashboard + Planning
export const getWODashboard = () => productionApi.get('/production/work-orders/dashboard');
export const getWOPlanningInfo = (poId) => productionApi.get(`/production/work-orders/planning-info/${poId}`);

// CRUD
export const createWorkOrder = (data) => productionApi.post('/production/work-orders/create', data);
export const getWorkOrdersByPO = (poId) => productionApi.get(`/production/orders/${poId}/work-orders`);
export const generateWorkOrders = (poId) => productionApi.post(`/production/orders/${poId}/generate-work-orders`);

// WO Lifecycle
export const releaseWO = (woId) => productionApi.patch(`/production/work-orders/${woId}/release`);
export const cancelWO = (woId, reason) => productionApi.delete(`/production/work-orders/${woId}`, { params: { reason } });
export const retryWOReservation = (woId) => productionApi.post(`/production/work-orders/${woId}/retry-reservation`);

// Execution (Equipment)
export const activateWO = (woId, data) => productionApi.post(`/production/work-orders/${woId}/activate`, data);
export const completeExecution = (woId, execId, data) => 
    productionApi.patch(`/production/work-orders/${woId}/executions/${execId}/complete`, data);
export const pauseExecution = (woId, execId) => 
    productionApi.patch(`/production/work-orders/${woId}/executions/${execId}/pause`);
export const getExecutions = (woId) => productionApi.get(`/production/work-orders/${woId}/executions`);
export const getEquipmentExecutions = (equipmentId) => 
    productionApi.get(`/production/equipment/${equipmentId}/executions`);


// ====================================================================
// Processes API — matches ProcessesController
// Base: /api/production/processes
// ====================================================================

export const getProcesses = () => productionApi.get('/production/processes');
export const getProcessById = (id) => productionApi.get(`/production/processes/${id}`);
export const createProcess = (data) => productionApi.post('/production/processes', data);
export const updateProcess = (id, data) => productionApi.put(`/production/processes/${id}`, data);
export const deleteProcess = (id) => productionApi.delete(`/production/processes/${id}`);


// ====================================================================
// Work Centers API — matches WorkCentersController
// Base: /api/production/work-centers
// ====================================================================

export const getWorkCenters = () => productionApi.get('/production/work-centers');
export const getWorkCenterById = (id) => productionApi.get(`/production/work-centers/${id}`);
export const createWorkCenter = (data) => productionApi.post('/production/work-centers', data);
export const updateWorkCenter = (id, data) => productionApi.put(`/production/work-centers/${id}`, data);
export const deleteWorkCenter = (id) => productionApi.delete(`/production/work-centers/${id}`);


// ====================================================================
// Equipment API — matches EquipmentController
// Base: /api/production/equipment
// ====================================================================

export const getEquipment = () => productionApi.get('/production/equipment');
export const getEquipmentById = (id) => productionApi.get(`/production/equipment/${id}`);
export const getEquipmentByWorkCenter = (wcId) => productionApi.get(`/production/equipment/work-center/${wcId}`);
export const createEquipment = (data) => productionApi.post('/production/equipment', data);
export const updateEquipment = (id, data) => productionApi.put(`/production/equipment/${id}`, data);
export const deleteEquipment = (id) => productionApi.delete(`/production/equipment/${id}`);
export const linkProcesses = (id, data) => productionApi.post(`/production/equipment/${id}/processes`, data);
export const getLinkedProcesses = (id) => productionApi.get(`/production/equipment/${id}/processes`);


// ====================================================================
// Process Routes API — matches ProcessRoutesController
// Base: /api/production/process-routes
// ====================================================================

export const getProcessRoutes = () => productionApi.get('/production/process-routes');
export const getProcessRouteById = (id) => productionApi.get(`/production/process-routes/${id}`);
export const getProcessRouteByProduct = (productId) => productionApi.get(`/production/process-routes/product/${productId}`);
export const createProcessRoute = (data) => productionApi.post('/production/process-routes', data);
export const updateProcessRoute = (id, data) => productionApi.put(`/production/process-routes/${id}`, data);


// ====================================================================
// MRP API — matches MRPController
// Base: /api/production/mrp
// ====================================================================

export const runMRP = (data) => productionApi.post('/production/mrp/run', data);
export const getMRPResult = (id) => productionApi.get(`/production/mrp/${id}`);
