import { useState } from "react";
import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import {
  NAV_ITEMS,
  PAGE_LAYOUTS,
  PAGE_TITLES,
  canAccessNav,
  getInitials,
} from "../../config/navigation";
import { useAuth } from "../../context/AuthContext";
import { ROLE_LABELS, UserRole } from "../../types";
import { Icon } from "../ui";

function SidebarNav({ onNavigate }: { onNavigate?: () => void }) {
  const { user, isAdmin } = useAuth();
  const role = user?.role ?? null;
  const displayName = user?.email.split("@")[0] ?? "Guest";

  return (
    <>
      <div className="px-md pb-md mb-sm border-b border-outline-variant">
        <div className="font-headline-sm text-headline-sm font-bold text-on-surface mb-sm hidden md:block">
          EcoMonitor
        </div>
        <div className="flex items-center gap-sm">
          <div className="w-8 h-8 rounded-full bg-surface-variant flex items-center justify-center border border-outline-variant shrink-0">
            {user ? (
              <span className="font-label-sm text-primary text-[10px]">{getInitials(displayName)}</span>
            ) : (
              <Icon name="person" className="text-[18px] text-on-surface-variant" />
            )}
          </div>
          <div className="min-w-0">
            <div className="font-label-sm text-label-sm text-on-surface truncate capitalize">
              {displayName}
            </div>
            <div className="font-label-sm text-label-sm text-on-surface-variant opacity-80">
              {user ? `${ROLE_LABELS[user.role]} Role` : "Not signed in"}
            </div>
          </div>
        </div>
      </div>

      <nav className="flex-1 overflow-y-auto py-sm px-sm space-y-xs">
        {NAV_ITEMS.map((item) => {
          const locked = !canAccessNav(item, role, Boolean(user));
          return (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === "/"}
              onClick={onNavigate}
              title={locked ? "Sign in required" : item.description}
              className={({ isActive }) =>
                `nav-link ${isActive ? "nav-link-active" : locked ? "nav-link-idle opacity-60" : "nav-link-idle"}`
              }
            >
              <Icon name={item.icon} className="text-[20px] shrink-0" />
              <span className="flex-1">{item.label}</span>
              {locked && <Icon name="lock" className="text-[14px] opacity-70" />}
              {item.minRole === UserRole.Admin && user && !isAdmin && (
                <Icon name="block" className="text-[14px] opacity-50" title="Admin only" />
              )}
            </NavLink>
          );
        })}
      </nav>
    </>
  );
}

export function AppLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);
  const pageTitle = PAGE_TITLES[location.pathname];
  const isCanvas = PAGE_LAYOUTS[location.pathname] === "canvas";
  const isHome = location.pathname === "/";

  return (
    <div className="flex h-screen overflow-hidden bg-background">
      <aside className="hidden md:flex flex-col w-64 h-full shrink-0 border-r border-outline-variant bg-surface-container py-md fixed left-0 top-0 z-40">
        <SidebarNav />
        <div className="px-sm pt-sm mt-sm border-t border-outline-variant">
          {user ? (
            <button
              type="button"
              onClick={() => {
                logout();
                navigate("/login");
              }}
              className="nav-link nav-link-idle w-full"
            >
              <Icon name="logout" className="text-[20px]" />
              Sign out
            </button>
          ) : (
            <NavLink
              to="/login"
              state={{ from: location.pathname }}
              className="nav-link text-primary border-transparent hover:bg-surface-container-highest"
            >
              <Icon name="login" className="text-[20px]" />
              Sign in
            </NavLink>
          )}
        </div>
      </aside>

      {mobileOpen && (
        <button
          type="button"
          aria-label="Close menu"
          className="fixed inset-0 bg-black/60 z-40 md:hidden"
          onClick={() => setMobileOpen(false)}
        />
      )}

      <aside
        className={`fixed top-0 left-0 h-full w-64 bg-surface-container border-r border-outline-variant z-50 flex flex-col py-md transition-transform md:hidden ${
          mobileOpen ? "translate-x-0" : "-translate-x-full"
        }`}
      >
        <div className="px-md pb-sm mb-sm flex justify-between items-center">
          <span className="font-bold text-primary font-headline-sm">EcoMonitor</span>
          <button type="button" onClick={() => setMobileOpen(false)} aria-label="Close">
            <Icon name="close" />
          </button>
        </div>
        <SidebarNav onNavigate={() => setMobileOpen(false)} />
      </aside>

      <div className="flex-1 flex flex-col min-w-0 md:ml-64">
        <header className="sticky top-0 z-30 bg-surface border-b border-outline-variant h-16 flex justify-between items-center px-md shrink-0">
          <div className="flex items-center gap-md min-w-0">
            <button
              type="button"
              className="md:hidden p-1 rounded hover:bg-surface-container-high"
              onClick={() => setMobileOpen(true)}
              aria-label="Open menu"
            >
              <Icon name="menu" className="text-primary" />
            </button>
            {pageTitle ? (
              <div className="flex items-center gap-2 min-w-0">
                <span className="font-headline-md text-headline-md font-bold text-primary shrink-0">
                  EcoMonitor
                </span>
                <span className="text-on-surface-variant">/</span>
                <span className="font-headline-sm text-headline-sm text-on-surface font-medium truncate">
                  {pageTitle}
                </span>
              </div>
            ) : (
              <span className="font-headline-md text-headline-md font-bold text-primary">EcoMonitor</span>
            )}
          </div>

          <div className="flex items-center gap-sm shrink-0">
            {isHome && (
              <div className="hidden sm:flex items-center gap-xs mr-sm px-sm py-xs rounded-full border border-error/30 bg-error/10">
                <div className="w-2 h-2 rounded-full bg-error animate-pulse" />
                <span className="font-label-sm text-label-sm text-error uppercase tracking-wider">
                  Live
                </span>
              </div>
            )}
            {user ? (
              <button
                type="button"
                onClick={() => logout()}
                className="w-8 h-8 flex items-center justify-center rounded text-primary hover:bg-surface-container-high transition-colors"
                title={user.email}
              >
                <Icon name="account_circle" className="text-[20px]" />
              </button>
            ) : (
              <NavLink to="/login" state={{ from: location.pathname }} className="btn-solid text-[10px] py-1 px-3">
                Sign in
              </NavLink>
            )}
          </div>
        </header>

        {!user && isHome && (
          <div className="mx-margin-mobile md:mx-margin-desktop mt-md p-sm border border-primary/30 bg-primary/5 rounded text-sm text-on-surface-variant">
            <strong className="text-primary">Guest mode.</strong> Live dashboard is public.{" "}
            <NavLink to="/login" state={{ from: "/dashboards" }} className="text-primary underline">
              Sign in
            </NavLink>{" "}
            for Panels, Alerts, Observations and Admin.
          </div>
        )}

        <main
          className={`flex-1 min-h-0 ${
            isCanvas ? "overflow-hidden flex flex-col" : "overflow-y-auto p-margin-mobile md:p-margin-desktop"
          }`}
        >
          {isCanvas ? (
            <Outlet />
          ) : (
            <div className="max-w-7xl mx-auto">
              <Outlet />
            </div>
          )}
        </main>
      </div>
    </div>
  );
}
