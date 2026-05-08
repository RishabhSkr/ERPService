import React from 'react';
import { Link } from 'react-router-dom';
import { Factory, ShieldCheck, BarChart3, Package, ArrowRight, Layers, Zap, Globe } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

const LandingPage = () => {
    const { isAuthenticated } = useAuth();
    return (
        <div className="min-h-screen bg-slate-950 text-white">
            {/* ═══ Navbar ═══ */}
            <nav className="flex items-center justify-between px-8 py-4 border-b border-slate-800/50">
                <div className="flex items-center gap-2 text-xl font-bold">
                    <Factory className="text-blue-500" size={28} />
                    <span>MyERP</span>
                </div>
                <div className="flex items-center gap-6">
                    <Link to="/" className="text-sm text-slate-300 hover:text-white transition-colors">Home</Link>
                    <Link to="/about" className="text-sm text-slate-300 hover:text-white transition-colors">About</Link>
                    <Link to="/contact" className="text-sm text-slate-300 hover:text-white transition-colors">Contact</Link>
                    {isAuthenticated ? (
                        <Link to="/app" className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors">
                            Dashboard →
                        </Link>
                    ) : (
                        <Link to="/login" className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors">
                            Login →
                        </Link>
                    )}
                </div>
            </nav>

            {/* ═══ Hero Section ═══ */}
            <section className="relative overflow-hidden">
                {/* Gradient bg */}
                <div className="absolute inset-0 bg-gradient-to-br from-blue-900/20 via-slate-950 to-purple-900/20" />
                <div className="absolute top-20 left-1/4 w-96 h-96 bg-blue-500/5 rounded-full blur-3xl" />
                <div className="absolute bottom-10 right-1/4 w-96 h-96 bg-purple-500/5 rounded-full blur-3xl" />
                
                <div className="relative max-w-5xl mx-auto px-8 py-24 text-center">
                    <div className="inline-flex items-center gap-2 px-3 py-1 bg-blue-500/10 border border-blue-500/20 rounded-full text-blue-400 text-xs font-medium mb-6">
                        <Zap size={12} /> Enterprise Resource Planning
                    </div>
                    <h1 className="text-5xl md:text-6xl font-extrabold leading-tight mb-6">
                        Manage Your Business
                        <br />
                        <span className="bg-gradient-to-r from-blue-400 to-purple-400 bg-clip-text text-transparent">
                            All in One Place
                        </span>
                    </h1>
                    <p className="text-lg text-slate-400 max-w-2xl mx-auto mb-10">
                        A complete microservices-based ERP system for Sales, Production, Inventory, 
                        and Master Data management. Built with .NET, React, and modern cloud architecture.
                    </p>
                    <div className="flex gap-4 justify-center">
                        {isAuthenticated ? (
                            <Link to="/app" className="bg-emerald-600 hover:bg-emerald-700 text-white px-6 py-3 rounded-xl font-medium flex items-center gap-2 transition-all hover:gap-3">
                                Go to Dashboard <ArrowRight size={18} />
                            </Link>
                        ) : (
                            <Link to="/login" className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-xl font-medium flex items-center gap-2 transition-all hover:gap-3">
                                Get Started <ArrowRight size={18} />
                            </Link>
                        )}
                        <Link to="/about" className="bg-slate-800 hover:bg-slate-700 text-white px-6 py-3 rounded-xl font-medium border border-slate-700 transition-colors">
                            Learn More
                        </Link>
                    </div>
                </div>
            </section>

            {/* ═══ Features Section ═══ */}
            <section className="max-w-6xl mx-auto px-8 py-20">
                <h2 className="text-3xl font-bold text-center mb-4">Powerful Modules</h2>
                <p className="text-slate-400 text-center mb-12 max-w-xl mx-auto">
                    Every module you need to run your manufacturing business efficiently.
                </p>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                    {[
                        { icon: BarChart3, title: 'Sales & CRM', desc: 'Orders, fulfillment, customer management', color: 'blue' },
                        { icon: Factory, title: 'Production', desc: 'Production orders, work orders, BOM, scheduling', color: 'purple' },
                        { icon: Package, title: 'Inventory', desc: 'Raw materials, finished goods, stock movements', color: 'green' },
                        { icon: ShieldCheck, title: 'Access Control', desc: 'Role-based API-level permission management', color: 'orange' },
                    ].map((feature, i) => (
                        <div key={i} className="bg-slate-900 border border-slate-800 rounded-xl p-6 hover:border-slate-600 transition-all hover:-translate-y-1 duration-300">
                            <div className={`w-12 h-12 rounded-lg flex items-center justify-center mb-4 bg-${feature.color}-500/10`}>
                                <feature.icon size={24} className={`text-${feature.color}-400`} />
                            </div>
                            <h3 className="text-lg font-semibold mb-2">{feature.title}</h3>
                            <p className="text-sm text-slate-400">{feature.desc}</p>
                        </div>
                    ))}
                </div>
            </section>

            {/* ═══ Architecture Section ═══ */}
            <section className="border-t border-slate-800">
                <div className="max-w-5xl mx-auto px-8 py-20 text-center">
                    <h2 className="text-3xl font-bold mb-4">Built for Scale</h2>
                    <p className="text-slate-400 mb-12">Microservices architecture with independent deployability.</p>
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                        {[
                            { label: 'API Gateway', sub: 'YARP Reverse Proxy' },
                            { label: 'Identity Service', sub: 'JWT + RBAC' },
                            { label: 'Sales Service', sub: 'Orders & Fulfillment' },
                            { label: 'Production Service', sub: 'PO & WO Management' },
                            { label: 'Inventory Service', sub: 'Inventory Management' },
                        ].map((svc, i) => (
                            <div key={i} className="bg-slate-900/50 border border-slate-800 rounded-lg p-4">
                                <div className="text-white font-medium text-sm">{svc.label}</div>
                                <div className="text-xs text-slate-500 mt-1">{svc.sub}</div>
                            </div>
                        ))}
                    </div>
                </div>
            </section>

            {/* ═══ Footer ═══ */}
            <footer className="border-t border-slate-800 px-8 py-6">
                <div className="max-w-5xl mx-auto flex items-center justify-between text-sm text-slate-500">
                    <div className="flex items-center gap-2">
                        <Factory size={16} className="text-blue-500" /> MyERP System
                    </div>
                    <div className="flex gap-6">
                        <Link to="/about" className="hover:text-white transition-colors">About</Link>
                        <Link to="/contact" className="hover:text-white transition-colors">Contact</Link>
                    </div>
                </div>
            </footer>
        </div>
    );
};

export default LandingPage;
