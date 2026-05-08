import { useAuth } from '../../context/AuthContext';
import { registerUser, getPublicRoles } from '../../api/authService';
import { useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { LogIn, UserPlus, Mail, Lock, User, Eye, EyeOff, ArrowRight, AlertCircle, CheckCircle, Loader2 } from 'lucide-react';

/**
 * AuthPage — Combined Login & Signup with toggle
 * 
 * 📚 PATTERN: "Single Page Auth" with mode toggle
 * 
 * Instead of 2 separate pages (/login and /signup), we use ONE component
 * with a `mode` state that toggles between 'login' and 'signup'.
 * 
 * WHY this pattern?
 *   1. Single route (/login) — no extra routing needed
 *   2. Shared UI — header, branding, error display reused
 *   3. Smooth UX — user toggles without page reload
 *   4. Less code duplication — one file, one component
 * 
 * HOW it works:
 *   - `mode` state: 'login' | 'signup'
 *   - Toggle button switches mode + clears form/errors
 *   - Conditionally render extra fields (email for signup)
 *   - handleSubmit checks mode → calls login() or registerUser()
 */
function AuthPage() {
    const { login } = useAuth();
    const navigate = useNavigate();

    // ─── Mode Toggle ───────────────────────────────────────
    // This single state controls which "view" is shown
    const [mode, setMode] = useState('login'); // 'login' | 'signup'

    // ─── Form Fields ───────────────────────────────────────
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [roleId, setRoleId] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    
    // ─── Roles Data ────────────────────────────────────────
    const [roles, setRoles] = useState([]);

    // ─── UI State ──────────────────────────────────────────
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const [loading, setLoading] = useState(false);

    // Fetch public roles on component mount
    useEffect(() => {
        const fetchRoles = async () => {
            try {
                const data = await getPublicRoles();
                console.log(data);
                setRoles(data?.data || []);
            } catch (err) {
                console.error("Failed to fetch roles", err);
            }
        };
        fetchRoles();
    }, []);

    // ─── Toggle Mode Handler ───────────────────────────────
    // When user clicks "Switch to Signup" or "Switch to Login"
    const toggleMode = () => {
        setMode(prev => prev === 'login' ? 'signup' : 'login');
        // Clear all form state when switching
        setError('');
        setSuccess('');
        setUsername('');
        setEmail('');
        setPassword('');
        setConfirmPassword('');
        setRoleId('');
    };

    // ─── Form Submit Handler ───────────────────────────────
    // Single handler — checks `mode` to decide what to do
    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        setSuccess('');

        try {
            if (mode === 'signup') {
                // ── Signup Flow ──
                // 1. Validate confirm password
                if (password !== confirmPassword) {
                    setError('Passwords do not match');
                    setLoading(false);
                    return;
                }
                
                // If roles exist but user didn't select one
                if (roles.length > 0 && !roleId) {
                    setError('Please select a requested role');
                    setLoading(false);
                    return;
                }

                // The backend requires a Guid for RoleId in the DTO, but for public signup, 
                // the backend ignores it and forces 'Pending' role anyway.
                // We send a dummy Guid if no role was selected (i.e. when roles array is empty).
                const finalRoleId = roleId || "00000000-0000-0000-0000-000000000000";

                // 2. Call register API
                await registerUser(username, email, password, finalRoleId);
                // 3. Show success message + switch to login
                setSuccess('Account created successfully! Awaiting admin approval.');
                // 4. Auto-switch to login after 3 seconds
                setTimeout(() => {
                    setMode('login');
                    setSuccess('');
                    setEmail('');
                    setConfirmPassword('');
                    setRoleId('');
                }, 3000);
            } else {
                // ── Login Flow ──
                // 1. Call login from AuthContext (saves token + user)
                await login(username, password);
                // 2. Redirect to dashboard
                window.location.href = '/';
            }
        } catch (err) {
            console.error(err);
            // Handle error from either login or register
            const message = err?.response?.data?.message
                || err?.message
                || (mode === 'login' ? 'Login failed' : 'Registration failed');
            setError(message);
        } finally {
            setLoading(false);
        }
    };

    // ─── Computed values based on mode ─────────────────────
    const isLogin = mode === 'login';

    return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 py-12 px-4">
            {/* Decorative Background Elements */}
            <div className="absolute inset-0 overflow-hidden pointer-events-none">
                <div className="absolute -top-40 -right-40 w-80 h-80 bg-blue-500/10 rounded-full blur-3xl"></div>
                <div className="absolute -bottom-40 -left-40 w-80 h-80 bg-indigo-500/10 rounded-full blur-3xl"></div>
            </div>

            <div className="relative max-w-md w-full">
                {/* ─── Brand Header ─────────────────────────── */}
                <div className="text-center mb-8">
                    <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-gradient-to-br from-blue-500 to-indigo-600 shadow-lg shadow-blue-500/25 mb-4">
                        <span className="text-2xl font-black text-white">E</span>
                    </div>
                    <h1 className="text-3xl font-bold text-white tracking-tight">MyERP</h1>
                    <p className="text-slate-400 text-sm mt-1">Enterprise Resource Planning</p>
                </div>

                {/* ─── Auth Card ─────────────────────────────── */}
                <div className="bg-white/5 backdrop-blur-xl border border-white/10 rounded-2xl shadow-2xl p-8">
                    
                    {/* ─── Mode Toggle Tabs ──────────────────── */}
                    {/* 
                      📚 This is the TOGGLE MECHANISM:
                      Two buttons side by side — active one has bg highlight.
                      onClick calls toggleMode or sets mode directly.
                    */}
                    <div className="flex bg-white/5 rounded-xl p-1 mb-8">
                        <button
                            type="button"
                            onClick={() => mode !== 'login' && toggleMode()}
                            className={`flex-1 flex items-center justify-center gap-2 py-2.5 px-4 rounded-lg text-sm font-medium transition-all duration-300 ${
                                isLogin
                                    ? 'bg-gradient-to-r from-blue-500 to-indigo-600 text-white shadow-lg shadow-blue-500/25'
                                    : 'text-slate-400 hover:text-slate-300'
                            }`}
                        >
                            <LogIn size={16} />
                            Sign In
                        </button>
                        <button
                            type="button"
                            onClick={() => mode !== 'signup' && toggleMode()}
                            className={`flex-1 flex items-center justify-center gap-2 py-2.5 px-4 rounded-lg text-sm font-medium transition-all duration-300 ${
                                !isLogin
                                    ? 'bg-gradient-to-r from-blue-500 to-indigo-600 text-white shadow-lg shadow-blue-500/25'
                                    : 'text-slate-400 hover:text-slate-300'
                            }`}
                        >
                            <UserPlus size={16} />
                            Sign Up
                        </button>
                    </div>

                    {/* ─── Heading ────────────────────────────── */}
                    <h2 className="text-xl font-semibold text-white mb-1">
                        {isLogin ? 'Welcome back' : 'Create account'}
                    </h2>
                    <p className="text-slate-400 text-sm mb-6">
                        {isLogin
                            ? 'Enter your credentials to access your account'
                            : 'Fill in the details to create a new account'
                        }
                    </p>

                    {/* ─── Form ───────────────────────────────── */}
                    <form onSubmit={handleSubmit} className="space-y-4">
                        
                        {/* Username Field (Both modes) */}
                        <div>
                            <label htmlFor="auth-username" className="block text-xs font-medium text-slate-400 mb-1.5 uppercase tracking-wider">
                                Username
                            </label>
                            <div className="relative">
                                <User size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                                <input
                                    id="auth-username"
                                    type="text"
                                    required
                                    placeholder="Enter username"
                                    value={username}
                                    onChange={(e) => setUsername(e.target.value)}
                                    className="w-full pl-10 pr-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500/50 transition-all text-sm"
                                />
                            </div>
                        </div>

                        {/* Email Field (Signup only) */}
                        {/* 
                          📚 CONDITIONAL RENDERING:
                          This field only appears when mode === 'signup'
                          This is the key technique for toggle forms!
                        */}
                        {!isLogin && (
                            <div className="animate-in">
                                <label htmlFor="auth-email" className="block text-xs font-medium text-slate-400 mb-1.5 uppercase tracking-wider">
                                    Email
                                </label>
                                <div className="relative">
                                    <Mail size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                                    <input
                                        id="auth-email"
                                        type="email"
                                        required
                                        placeholder="you@example.com"
                                        value={email}
                                        onChange={(e) => setEmail(e.target.value)}
                                        className="w-full pl-10 pr-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500/50 transition-all text-sm"
                                    />
                                </div>
                            </div>
                        )}

                        {/* Role Selection (Signup only) */}
                        {/* Role Selection (Signup only) */}
                        {!isLogin && (
                            <div className="animate-in">
                                <label htmlFor="auth-role" className="block text-xs font-medium text-slate-400 mb-1.5 uppercase tracking-wider">
                                    Requested Role
                                </label>
                                <div className="relative">
                                    <User size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                                    {roles.length > 0 ? (
                                        <select
                                            id="auth-role"
                                            required
                                            value={roleId}
                                            onChange={(e) => setRoleId(e.target.value)}
                                            className="w-full pl-10 pr-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500/50 transition-all text-sm appearance-none"
                                        >
                                            <option value="" disabled className="bg-slate-800 text-slate-400">Select a role</option>
                                            {roles.map(r => (
                                                <option key={r.roleId} value={r.roleId} className="bg-slate-800 text-white">
                                                    {r.roleName}
                                                </option>
                                            ))}
                                        </select>
                                    ) : (
                                        <div className="w-full pl-10 pr-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-slate-400 text-sm flex items-center">
                                            No roles available (You will be set as Pending)
                                        </div>
                                    )}
                                </div>
                            </div>
                        )}

                        {/* Password Field (Both modes) */}
                        <div>
                            <label htmlFor="auth-password" className="block text-xs font-medium text-slate-400 mb-1.5 uppercase tracking-wider">
                                Password
                            </label>
                            <div className="relative">
                                <Lock size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                                <input
                                    id="auth-password"
                                    type={showPassword ? 'text' : 'password'}
                                    required
                                    placeholder="••••••••"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    className="w-full pl-10 pr-10 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500/50 transition-all text-sm"
                                />
                                <button
                                    type="button"
                                    onClick={() => setShowPassword(!showPassword)}
                                    className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-500 hover:text-slate-300 transition-colors"
                                >
                                    {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                                </button>
                            </div>
                        </div>

                        {/* Confirm Password Field (Signup only) */}
                        {!isLogin && (
                            <div className="animate-in">
                                <label htmlFor="auth-confirm-password" className="block text-xs font-medium text-slate-400 mb-1.5 uppercase tracking-wider">
                                    Confirm Password
                                </label>
                                <div className="relative">
                                    <Lock size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                                    <input
                                        id="auth-confirm-password"
                                        type="password"
                                        required
                                        placeholder="••••••••"
                                        value={confirmPassword}
                                        onChange={(e) => setConfirmPassword(e.target.value)}
                                        className="w-full pl-10 pr-4 py-2.5 bg-white/5 border border-white/10 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500/50 transition-all text-sm"
                                    />
                                </div>
                            </div>
                        )}

                        {/* ─── Error Message ────────────────────── */}
                        {error && (
                            <div className="flex items-start gap-2 bg-red-500/10 border border-red-500/20 text-red-400 px-4 py-3 rounded-xl text-sm">
                                <AlertCircle size={16} className="mt-0.5 shrink-0" />
                                <span>{error}</span>
                            </div>
                        )}

                        {/* ─── Success Message ───────────────────── */}
                        {success && (
                            <div className="flex items-start gap-2 bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 px-4 py-3 rounded-xl text-sm">
                                <CheckCircle size={16} className="mt-0.5 shrink-0" />
                                <span>{success}</span>
                            </div>
                        )}

                        {/* ─── Submit Button ─────────────────────── */}
                        <button
                            type="submit"
                            disabled={loading}
                            className="w-full flex items-center justify-center gap-2 py-3 px-4 bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white font-medium rounded-xl shadow-lg shadow-blue-500/25 hover:shadow-blue-500/40 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:shadow-blue-500/25 text-sm"
                        >
                            {loading ? (
                                <>
                                    <Loader2 size={18} className="animate-spin" />
                                    <span>{isLogin ? 'Signing in...' : 'Creating account...'}</span>
                                </>
                            ) : (
                                <>
                                    <span>{isLogin ? 'Sign In' : 'Create Account'}</span>
                                    <ArrowRight size={16} />
                                </>
                            )}
                        </button>
                    </form>

                    {/* ─── Bottom Toggle Link ────────────────── */}
                    <div className="mt-6 text-center">
                        <p className="text-slate-500 text-sm">
                            {isLogin ? "Don't have an account?" : "Already have an account?"}
                            <button
                                type="button"
                                onClick={toggleMode}
                                className="ml-1 text-blue-400 hover:text-blue-300 font-medium transition-colors"
                            >
                                {isLogin ? 'Sign up' : 'Sign in'}
                            </button>
                        </p>
                    </div>
                </div>

                {/* ─── Footer ────────────────────────────────── */}
                <p className="text-center text-slate-600 text-xs mt-6">
                    © 2026 MyERP. All rights reserved.
                </p>
            </div>
        </div>
    );
}

export default AuthPage;
