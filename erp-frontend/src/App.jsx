import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';

import Layout from './components/layout/Layout';
import Dashboard from './pages/production/Dashboard';
import WODashboard from './pages/production/WODashboard';
import WOManagement from './pages/production/WOManagement';
import RawMaterial from './pages/masters/rawMaterial';
import Products from './pages/masters/products';
import BOM from './pages/masters/BOM';
import Production from './pages/production/AllOrders';
import OrderManagement from './pages/production/OrderManagement';
import CreateOrder from './pages/production/CreateOrder';
import AddRawMaterialStock from './pages/inventory/AddRawMaterialStock';
import FinishedGoodStock from './pages/inventory/FinishedGoodStock';

// Auth
import LoginPage from './pages/auth/AuthPage';
import { AuthProvider, useAuth } from './context/AuthContext';
// Sales Pages
import SalesOrders from './pages/sales/SalesOrders';
import FulfillmentDashboard from './pages/sales/FulfillmentDashboard';
// Production Master Data
import Processes from './pages/production/Processes';
import WorkCenters from './pages/production/WorkCenters';
import EquipmentPage from './pages/production/EquipmentPage';
import ProcessRoutesPage from './pages/production/ProcessRoutesPage';

// Inventory Pages
import Categories from './pages/masters/Categories';
import Units from './pages/masters/Units';
import Warehouses from './pages/masters/Warehouses';
import StorageLocations from './pages/masters/StorageLocations';
import StorageLocationTypes from './pages/masters/StorageLocationTypes';
import StockMovements from './pages/inventory/StockMovements';

// Settings Pages (Admin Only)
import UserManagement from './pages/settings/UserManagement';
import RoleManagement from './pages/settings/RoleManagement';
import PermissionManager from './pages/settings/PermissionManager';
import ModuleManagement from './pages/settings/ModuleManagement';
import UserProfile from './pages/settings/UserProfile';

// Public Pages
import LandingPage from './pages/public/LandingPage';
import AboutPage from './pages/public/AboutPage';
import ContactPage from './pages/public/ContactPage';


const ProtectedRoute = ({ children }) => {
    const { isAuthenticated, loading } = useAuth();
    if (loading) return <div>Loading...</div>;
    if (!isAuthenticated) return <Navigate to="/login" />;
    return children;
};

// Redirect logged-in users from public pages to /app
const RedirectIfLoggedIn = ({ children }) => {
    const { isAuthenticated, loading } = useAuth();
    if (loading) return <div>Loading...</div>;
    if (isAuthenticated) return <Navigate to="/app" />;
    return children;
};

function App() {
  return (
    <AuthProvider>
    <BrowserRouter>
    <Routes>
      {/* Public Routes — accessible to everyone */}
      <Route path="/" element={<LandingPage />} />
      <Route path="/about" element={<AboutPage />} />
      <Route path="/contact" element={<ContactPage />} />
      <Route path="/login" element={<LoginPage />} />

      {/* Protected Routes (login required) */}
      <Route path="/app/*" element={
        <ProtectedRoute>  
          <Layout>
            <Routes>
              {/* Dashboard */}
              <Route path="/" element={<Dashboard />} />

              {/* Sales Routes */}
              <Route path="/sales/orders" element={<SalesOrders />} />
              <Route path="/sales/fulfillment" element={<FulfillmentDashboard />} />

              {/* Production Management */}
              <Route path="/production-plan" element={<OrderManagement />} />
              <Route path="/production-create-order" element={<CreateOrder />} />
              <Route path="/production-orders" element={<Production />} />
              <Route path="/production/wo-dashboard" element={<WODashboard />} />
              <Route path="/production/work-orders" element={<WOManagement />} />
              <Route path="/production/bom" element={<BOM />} />

              {/* Production Master Data */}
              <Route path="/production/processes" element={<Processes />} />
              <Route path="/production/work-centers" element={<WorkCenters />} />
              <Route path="/production/equipment" element={<EquipmentPage />} />
              <Route path="/production/process-routes" element={<ProcessRoutesPage />} />

              {/* Masters Routes */}
              <Route path="/masters/raw-materials" element={<RawMaterial />} />
              <Route path="/masters/products" element={<Products />} />
              <Route path="/masters/categories" element={<Categories />} />
              <Route path="/masters/units" element={<Units />} />
              <Route path="/masters/warehouses" element={<Warehouses />} />
              <Route path="/masters/storage-locations" element={<StorageLocations />} />
              <Route path="/masters/storage-location-types" element={<StorageLocationTypes />} />

              {/* Inventory Routes */}
              <Route path="/inventory/raw-material" element={<AddRawMaterialStock />} />
              <Route path="/inventory/finished-goods" element={<FinishedGoodStock />} />
              <Route path="/inventory/stock-movements" element={<StockMovements />} />

              {/* Settings Routes */}
              <Route path="/settings/users" element={<UserManagement />} />
              <Route path="/settings/roles" element={<RoleManagement />} />
              <Route path="/settings/permissions" element={<PermissionManager />} />
              <Route path="/settings/modules" element={<ModuleManagement />} />
              <Route path="/profile" element={<UserProfile />} />
              
            </Routes>
          </Layout>
        </ProtectedRoute>
      }/>
    </Routes>
    </BrowserRouter>
    </AuthProvider>
  );
}

export default App;