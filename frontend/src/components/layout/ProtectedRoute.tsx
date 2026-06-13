import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import { UserRole } from "../../types";
import { LoadingState } from "../ui";

export function ProtectedRoute({ minRole = UserRole.Viewer }: { minRole?: UserRole }) {
  const { user, loading } = useAuth();
  const location = useLocation();

  if (loading) {
    return <LoadingState />;
  }

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  if (user.role < minRole) {
    return <Navigate to="/" replace state={{ denied: true }} />;
  }

  return <Outlet />;
}
