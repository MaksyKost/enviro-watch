import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { authApi } from "../api";
import type { User } from "../types";
import { UserRole } from "../types";

const TOKEN_KEY = "envirowatch_token";

interface AuthContextValue {
  user: User | null;
  token: string | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string) => Promise<void>;
  logout: () => void;
  isAnalyst: boolean;
  isAdmin: boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_KEY));
  const [loading, setLoading] = useState(true);
  const skipMeValidation = useRef(false);

  const persist = useCallback((nextToken: string, nextUser: User) => {
    skipMeValidation.current = true;
    localStorage.setItem(TOKEN_KEY, nextToken);
    setToken(nextToken);
    setUser(nextUser);
    setLoading(false);
  }, []);

  const logout = useCallback(() => {
    skipMeValidation.current = false;
    localStorage.removeItem(TOKEN_KEY);
    setToken(null);
    setUser(null);
    setLoading(false);
  }, []);

  useEffect(() => {
    if (!token) {
      setLoading(false);
      return;
    }

    if (skipMeValidation.current) {
      skipMeValidation.current = false;
      return;
    }

    setLoading(true);
    authApi
      .me(token)
      .then(setUser)
      .catch(() => logout())
      .finally(() => setLoading(false));
  }, [token, logout]);

  const login = useCallback(
    async (email: string, password: string) => {
      const response = await authApi.login(email, password);
      persist(response.token, response.user);
    },
    [persist],
  );

  const register = useCallback(
    async (email: string, password: string) => {
      const response = await authApi.register(email, password);
      persist(response.token, response.user);
    },
    [persist],
  );

  const value = useMemo(
    () => ({
      user,
      token,
      loading,
      login,
      register,
      logout,
      isAnalyst: (user?.role ?? UserRole.Viewer) >= UserRole.Analyst,
      isAdmin: user?.role === UserRole.Admin,
    }),
    [user, token, loading, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}
