import { useState, type FormEvent } from "react";
import { Link, Navigate, useLocation, useNavigate } from "react-router-dom";
import { HttpError } from "../api/client";
import { ErrorBanner, Icon } from "../components/ui";
import { useAuth } from "../context/AuthContext";

export function LoginPage() {
  const { user, login, register, loading } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const from = (location.state as { from?: string } | null)?.from ?? "/dashboards";
  const [mode, setMode] = useState<"login" | "register">("login");
  const [email, setEmail] = useState("admin@envirowatch.local");
  const [password, setPassword] = useState("Admin123!");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (!loading && user) {
    return <Navigate to={from} replace />;
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);

    if (!email.includes("@")) {
      setError("Enter a valid email address.");
      return;
    }
    if (password.length < 8) {
      setError("Password must be at least 8 characters.");
      return;
    }
    if (mode === "register" && password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setSubmitting(true);
    try {
      if (mode === "login") {
        await login(email, password);
      } else {
        await register(email, password);
      }
      navigate(from, { replace: true });
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Authentication failed.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center p-md bg-background">
      <div className="surface-panel w-full max-w-md p-lg">
        <div className="flex items-center gap-sm mb-lg">
          <Icon name="eco" className="text-primary text-[32px]" />
          <div>
            <h1 className="font-headline-md text-headline-md text-primary">EcoMonitor</h1>
            <p className="font-label-sm text-label-sm text-on-surface-variant uppercase">
              EnviroWatch Platform
            </p>
          </div>
        </div>

        <p className="font-body-md text-body-md text-on-surface-variant mb-lg">
          {mode === "login" ? "Sign in to your account" : "Create a new account"}
        </p>

        {error && <ErrorBanner message={error} />}

        <form onSubmit={handleSubmit} className="space-y-md">
          <div>
            <label className="block font-label-sm text-label-sm text-on-surface-variant uppercase mb-1">
              Email
            </label>
            <input
              type="email"
              className="input-dark"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <div>
            <label className="block font-label-sm text-label-sm text-on-surface-variant uppercase mb-1">
              Password
            </label>
            <input
              type="password"
              className="input-dark"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              minLength={8}
            />
          </div>
          {mode === "register" && (
            <div>
              <label className="block font-label-sm text-label-sm text-on-surface-variant uppercase mb-1">
                Confirm password
              </label>
              <input
                type="password"
                className="input-dark"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
                minLength={8}
              />
            </div>
          )}
          <button type="submit" className="btn-solid w-full" disabled={submitting}>
            {submitting ? "Please wait…" : mode === "login" ? "Sign in" : "Register"}
          </button>
        </form>

        <p className="mt-md text-sm text-on-surface-variant text-center">
          {mode === "login" ? (
            <>
              No account?{" "}
              <button type="button" className="text-primary hover:underline" onClick={() => setMode("register")}>
                Register
              </button>
            </>
          ) : (
            <>
              Already registered?{" "}
              <button type="button" className="text-primary hover:underline" onClick={() => setMode("login")}>
                Sign in
              </button>
            </>
          )}
        </p>

        <p className="mt-md text-xs text-on-surface-variant border-t border-slate-border pt-md font-data-label">
          Dev admin: <code className="text-primary">admin@envirowatch.local</code> /{" "}
          <code className="text-primary">Admin123!</code>
        </p>

        <Link to="/" className="block text-center text-sm text-primary mt-md hover:underline">
          Continue without login
        </Link>
      </div>
    </div>
  );
}
