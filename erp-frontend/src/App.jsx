import { BrowserRouter, Routes, Route } from 'react-router-dom';
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
import StockMovements from './pages/inventory/StockMovements';


function App() {
  return (
    <BrowserRouter>
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

          {/* Inventory Routes */}
          <Route path="/inventory/raw-material" element={<AddRawMaterialStock />} />
          <Route path="/inventory/finished-goods" element={<FinishedGoodStock />} />
          <Route path="/inventory/stock-movements" element={<StockMovements />} />
        </Routes>
      </Layout>
    </BrowserRouter>
  );
}

export default App;