import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { AppLayout } from "./components/layout/AppLayout";
import { ProtectedRoute } from "./components/layout/ProtectedRoute";
import { AuthProvider } from "./context/AuthContext";
import { AdminPage } from "./pages/AdminPage";
import { AlertsPage } from "./pages/AlertsPage";
import { DashboardsPage } from "./pages/DashboardsPage";
import { LiveDashboardPage } from "./pages/LiveDashboardPage";
import { LoginPage } from "./pages/LoginPage";
import { ObservationsPage } from "./pages/ObservationsPage";
import { UserRole } from "./types";

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<AppLayout />}>
            <Route index element={<LiveDashboardPage />} />
            <Route element={<ProtectedRoute minRole={UserRole.Viewer} />}>
              <Route path="dashboards" element={<DashboardsPage />} />
              <Route path="alerts" element={<AlertsPage />} />
              <Route path="observations" element={<ObservationsPage />} />
            </Route>
            <Route element={<ProtectedRoute minRole={UserRole.Admin} />}>
              <Route path="admin" element={<AdminPage />} />
            </Route>
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
