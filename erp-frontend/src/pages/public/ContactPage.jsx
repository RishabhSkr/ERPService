import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { Factory, Mail, MapPin, Phone, Send } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

const ContactPage = () => {
    const { isAuthenticated } = useAuth();
    const [form, setForm] = useState({ name: '', email: '', message: '' });
    const [submitted, setSubmitted] = useState(false);

    const handleSubmit = (e) => {
        e.preventDefault();
        // Simulate form submission
        setSubmitted(true);
        setTimeout(() => setSubmitted(false), 3000);
        setForm({ name: '', email: '', message: '' });
    };

    return (
        <div className="min-h-screen bg-slate-950 text-white">
            {/* Navbar */}
            <nav className="flex items-center justify-between px-8 py-4 border-b border-slate-800/50">
                <Link to="/" className="flex items-center gap-2 text-xl font-bold">
                    <Factory className="text-blue-500" size={28} /> MyERP
                </Link>
                <div className="flex items-center gap-6">
                    <Link to="/" className="text-sm text-slate-300 hover:text-white transition-colors">Home</Link>
                    <Link to="/about" className="text-sm text-slate-300 hover:text-white transition-colors">About</Link>
                    <Link to="/contact" className="text-sm text-white font-medium">Contact</Link>
                    {isAuthenticated ? (
                        <Link to="/app" className="bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors">Dashboard →</Link>
                    ) : (
                        <Link to="/login" className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors">Login →</Link>
                    )}
                </div>
            </nav>

            <section className="max-w-4xl mx-auto px-8 py-20">
                <h1 className="text-4xl font-extrabold mb-4">Contact Us</h1>
                <p className="text-lg text-slate-400 mb-12">Have questions? We'd love to hear from you.</p>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
                    {/* Contact Form */}
                    <div className="bg-slate-900 border border-slate-800 rounded-xl p-6">
                        <h2 className="text-lg font-semibold mb-4">Send a Message</h2>
                        {submitted && (
                            <div className="mb-4 p-3 bg-green-500/10 border border-green-500/20 rounded-lg text-green-400 text-sm">
                                ✅ Message sent successfully! We'll get back to you soon.
                            </div>
                        )}
                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div>
                                <label className="block text-xs text-slate-400 mb-1">Your Name</label>
                                <input type="text" required
                                    className="w-full bg-slate-800 text-white rounded-lg px-3 py-2.5 border border-slate-700 outline-none focus:border-blue-500 transition-colors"
                                    placeholder="John Doe"
                                    value={form.name} onChange={(e) => setForm({...form, name: e.target.value})} />
                            </div>
                            <div>
                                <label className="block text-xs text-slate-400 mb-1">Email Address</label>
                                <input type="email" required
                                    className="w-full bg-slate-800 text-white rounded-lg px-3 py-2.5 border border-slate-700 outline-none focus:border-blue-500 transition-colors"
                                    placeholder="john@example.com"
                                    value={form.email} onChange={(e) => setForm({...form, email: e.target.value})} />
                            </div>
                            <div>
                                <label className="block text-xs text-slate-400 mb-1">Message</label>
                                <textarea rows="4" required
                                    className="w-full bg-slate-800 text-white rounded-lg px-3 py-2.5 border border-slate-700 outline-none focus:border-blue-500 transition-colors resize-none"
                                    placeholder="How can we help you?"
                                    value={form.message} onChange={(e) => setForm({...form, message: e.target.value})} />
                            </div>
                            <button type="submit" className="bg-blue-600 hover:bg-blue-700 text-white px-5 py-2.5 rounded-lg font-medium flex items-center gap-2 transition-colors w-full justify-center">
                                <Send size={16} /> Send Message
                            </button>
                        </form>
                    </div>

                    {/* Contact Info */}
                    <div className="space-y-6">
                        <h2 className="text-lg font-semibold">Get in Touch</h2>
                        <div className="space-y-4">
                            {[
                                { icon: Mail, label: 'Email', value: 'support@myerp.com' },
                                { icon: Phone, label: 'Phone', value: '+91 98765 43210' },
                                { icon: MapPin, label: 'Address', value: 'Tech Hub, Bangalore, India' },
                            ].map((info, i) => (
                                <div key={i} className="flex items-start gap-4 bg-slate-900 border border-slate-800 rounded-xl p-4">
                                    <div className="w-10 h-10 rounded-lg bg-blue-500/10 flex items-center justify-center flex-shrink-0">
                                        <info.icon size={18} className="text-blue-400" />
                                    </div>
                                    <div>
                                        <div className="text-xs text-slate-500 mb-0.5">{info.label}</div>
                                        <div className="text-sm text-white">{info.value}</div>
                                    </div>
                                </div>
                            ))}
                        </div>

                        <div className="bg-slate-900 border border-slate-800 rounded-xl p-6 mt-6">
                            <h3 className="font-medium mb-2">Office Hours</h3>
                            <div className="text-sm text-slate-400 space-y-1">
                                <p>Monday - Friday: 9:00 AM - 6:00 PM</p>
                                <p>Saturday: 10:00 AM - 2:00 PM</p>
                                <p>Sunday: Closed</p>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            {/* Footer */}
            <footer className="border-t border-slate-800 px-8 py-6 mt-12">
                <div className="max-w-4xl mx-auto flex items-center justify-between text-sm text-slate-500">
                    <div className="flex items-center gap-2"><Factory size={16} className="text-blue-500" /> MyERP System</div>
                    <div className="flex gap-6">
                        <Link to="/" className="hover:text-white transition-colors">Home</Link>
                        <Link to="/about" className="hover:text-white transition-colors">About</Link>
                    </div>
                </div>
            </footer>
        </div>
    );
};

export default ContactPage;
