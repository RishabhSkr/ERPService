import React from 'react';
import { Link } from 'react-router-dom';
import { Factory, Code2, Server, Globe, Shield, Database } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

const AboutPage = () => {
    const { isAuthenticated } = useAuth();
    return (
        <div className="min-h-screen bg-slate-950 text-white">
            {/* Navbar */}
            <nav className="flex items-center justify-between px-8 py-4 border-b border-slate-800/50">
                <Link to="/" className="flex items-center gap-2 text-xl font-bold">
                    <Factory className="text-blue-500" size={28} /> MyERP
                </Link>
                <div className="flex items-center gap-6">
                    <Link to="/" className="text-sm text-slate-300 hover:text-white transition-colors">Home</Link>
                    <Link to="/about" className="text-sm text-white font-medium">About</Link>
                    <Link to="/contact" className="text-sm text-slate-300 hover:text-white transition-colors">Contact</Link>
                    {isAuthenticated ? (
                        <Link to="/app" className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors">Dashboard →</Link>
                    ) : (
                        <Link to="/login" className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors">Login →</Link>
                    )}
                </div>
            </nav>

            {/* Hero */}
            <section className="max-w-4xl mx-auto px-8 py-20">
                <h1 className="text-4xl font-extrabold mb-4">About MyERP System</h1>
                <p className="text-lg text-slate-400 mb-12">
                    A full-featured, microservices-based Enterprise Resource Planning system 
                    designed for modern manufacturing businesses.
                </p>

                {/* Tech Stack */}
                <div className="mb-16">
                    <h2 className="text-2xl font-bold mb-6">Technology Stack</h2>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        {[
                            { icon: Server, title: 'Backend', items: ['.NET 10', 'Entity Framework Core', 'SQL Server', 'YARP Gateway'] },
                            { icon: Code2, title: 'Frontend', items: ['React 18', 'Tailwind CSS', 'Lucide Icons', 'React Router'] },
                            { icon: Shield, title: 'Security', items: ['JWT Authentication', 'API-Level RBAC', 'Permission Middleware', 'Role Management'] },
                        ].map((stack, i) => (
                            <div key={i} className="bg-slate-900 border border-slate-800 rounded-xl p-6">
                                <stack.icon size={24} className="text-blue-400 mb-3" />
                                <h3 className="font-semibold mb-3">{stack.title}</h3>
                                <ul className="space-y-1.5">
                                    {stack.items.map((item, j) => (
                                        <li key={j} className="text-sm text-slate-400 flex items-center gap-2">
                                            <span className="w-1.5 h-1.5 rounded-full bg-blue-500" /> {item}
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Architecture */}
                <div className="mb-16">
                    <h2 className="text-2xl font-bold mb-6">Architecture</h2>
                    <div className="bg-slate-900 border border-slate-800 rounded-xl p-8">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            {[
                                { icon: Globe, title: 'API Gateway', desc: 'Central entry point using YARP reverse proxy with permission middleware for all microservices.' },
                                { icon: Shield, title: 'Identity Service', desc: 'Handles authentication, authorization, role management, and API-level permission control.' },
                                { icon: Database, title: 'Sales Service', desc: 'Manages sales orders, customer tracking, and order fulfillment workflows.' },
                                { icon: Factory, title: 'Production Service', desc: 'Production orders, work orders, BOM management, process routing, and scheduling.' },
                                { icon: Factory, title: 'Inventory Service', desc: 'Inventory management, product catalog, stock tracking, and movement.' },
                            ].map((svc, i) => (
                                <div key={i} className="flex gap-4">
                                    <svc.icon size={20} className="text-blue-400 mt-1 flex-shrink-0" />
                                    <div>
                                        <h3 className="font-medium mb-1">{svc.title}</h3>
                                        <p className="text-sm text-slate-400">{svc.desc}</p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            </section>

            {/* Footer */}
            <footer className="border-t border-slate-800 px-8 py-6">
                <div className="max-w-4xl mx-auto flex items-center justify-between text-sm text-slate-500">
                    <div className="flex items-center gap-2"><Factory size={16} className="text-blue-500" /> MyERP System</div>
                    <div className="flex gap-6">
                        <Link to="/" className="hover:text-white transition-colors">Home</Link>
                        <Link to="/contact" className="hover:text-white transition-colors">Contact</Link>
                    </div>
                </div>
            </footer>
        </div>
    );
};

export default AboutPage;
