import React, { useState, useEffect, useRef, useMemo } from 'react';
import { X, Search, ChevronRight } from 'lucide-react';

/**
 * SearchSelectModal — Reusable searchable selection modal
 * 
 * Replaces <select> dropdowns with a searchable, filterable modal
 * that scales well with large datasets (100s/1000s of items).
 * 
 * @param {boolean} isOpen - Show/hide the modal
 * @param {function} onClose - Close without selection
 * @param {function} onSelect - Callback with selected item object
 * @param {string} title - Modal title e.g. "Select Product"
 * @param {Array} items - Array of items to search through
 * @param {Array} displayFields - Fields to show as columns: [{ key, label, width? }]
 * @param {Array} searchKeys - Fields to search in: ['code', 'name']
 * @param {string} valueKey - Primary key field name ('id', 'productId', etc.)
 * @param {string} currentValue - Currently selected value (for highlighting)
 */
const SearchSelectModal = ({
    isOpen,
    onClose,
    onSelect,
    title = 'Select Item',
    items = [],
    displayFields = [],
    searchKeys = [],
    valueKey = 'id',
    currentValue = '',
}) => {
    const [search, setSearch] = useState('');
    const [highlightIdx, setHighlightIdx] = useState(0);
    const inputRef = useRef(null);
    const listRef = useRef(null);

    // Reset search when modal opens
    useEffect(() => {
        if (isOpen) {
            setSearch('');
            setHighlightIdx(0);
            // Focus search input after render
            setTimeout(() => inputRef.current?.focus(), 50);
        }
    }, [isOpen]);

    // Filter items by search query across all searchKeys
    const filtered = useMemo(() => {
        if (!search.trim()) return items;
        const q = search.toLowerCase().trim();
        return items.filter(item =>
            searchKeys.some(key => {
                const val = item[key];
                return val && String(val).toLowerCase().includes(q);
            })
        );
    }, [items, search, searchKeys]);

    // Keyboard navigation
    const handleKeyDown = (e) => {
        if (e.key === 'Escape') {
            onClose();
        } else if (e.key === 'ArrowDown') {
            e.preventDefault();
            setHighlightIdx(prev => Math.min(prev + 1, filtered.length - 1));
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            setHighlightIdx(prev => Math.max(prev - 1, 0));
        } else if (e.key === 'Enter' && filtered.length > 0) {
            e.preventDefault();
            onSelect(filtered[highlightIdx]);
            onClose();
        }
    };

    // Scroll highlighted item into view
    useEffect(() => {
        if (listRef.current) {
            const rows = listRef.current.querySelectorAll('[data-row]');
            if (rows[highlightIdx]) {
                rows[highlightIdx].scrollIntoView({ block: 'nearest' });
            }
        }
    }, [highlightIdx]);

    // Reset highlight when search changes
    useEffect(() => {
        setHighlightIdx(0);
    }, [search]);

    if (!isOpen) return null;

    return (
        <div
            className="fixed inset-0 bg-black/40 flex items-center justify-center z-[60] p-4"
            onClick={(e) => { if (e.target === e.currentTarget) onClose(); }}
            onKeyDown={handleKeyDown}
        >
            <div className="bg-white rounded-2xl shadow-2xl w-full max-w-xl flex flex-col max-h-[70vh] overflow-hidden">
                {/* Header */}
                <div className="flex items-center justify-between px-5 py-4 border-b border-slate-200 flex-shrink-0">
                    <h3 className="text-base font-bold text-slate-800">{title}</h3>
                    <button
                        onClick={onClose}
                        className="p-1.5 rounded-lg hover:bg-slate-100 text-slate-400 hover:text-slate-600 transition"
                    >
                        <X size={18} />
                    </button>
                </div>

                {/* Search */}
                <div className="px-5 py-3 border-b border-slate-100 flex-shrink-0">
                    <div className="relative">
                        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                        <input
                            ref={inputRef}
                            type="text"
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                            placeholder={`Search by ${searchKeys.join(', ')}...`}
                            className="w-full pl-9 pr-3 py-2.5 border border-slate-200 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition"
                        />
                    </div>
                </div>

                {/* Results */}
                <div ref={listRef} className="flex-1 overflow-y-auto min-h-0">
                    {/* Column headers */}
                    <div className="sticky top-0 bg-slate-50 border-b border-slate-200 flex px-5 py-2">
                        {displayFields.map(field => (
                            <span
                                key={field.key}
                                className="text-[10px] font-bold text-slate-400 uppercase tracking-wider"
                                style={{ width: field.width || `${100 / displayFields.length}%` }}
                            >
                                {field.label}
                            </span>
                        ))}
                        <span className="w-6" />
                    </div>

                    {filtered.length === 0 ? (
                        <div className="px-5 py-10 text-center text-slate-400 text-sm">
                            <Search size={32} className="mx-auto mb-2 opacity-20" />
                            <p>No results found for "{search}"</p>
                        </div>
                    ) : (
                        filtered.map((item, idx) => {
                            const isSelected = currentValue && String(item[valueKey]) === String(currentValue);
                            const isHighlighted = idx === highlightIdx;
                            return (
                                <div
                                    key={item[valueKey] || idx}
                                    data-row
                                    onClick={() => { onSelect(item); onClose(); }}
                                    className={`flex items-center px-5 py-3 cursor-pointer border-b border-slate-50 transition-all duration-150
                                        ${isHighlighted ? 'bg-blue-100 text-slate-900' : 'hover:bg-slate-100'}
                                        ${isSelected ? 'bg-blue-50 border-l-4 border-l-blue-500' : 'border-l-4 border-l-transparent'}
                                    `}
                                    onMouseEnter={() => setHighlightIdx(idx)}
                                >
                                    {displayFields.map(field => (
                                        <span
                                            key={field.key}
                                            className={`text-sm truncate ${
                                                field.bold ? 'font-semibold text-slate-800' : 'text-slate-600'
                                            } ${isHighlighted ? '!text-blue-900' : ''}`}
                                            style={{ width: field.width || `${100 / displayFields.length}%` }}
                                            title={String(item[field.key] ?? '')}
                                        >
                                            {item[field.key] ?? '-'}
                                        </span>
                                    ))}
                                    <ChevronRight size={14} className={`flex-shrink-0 ${isHighlighted ? 'text-blue-500' : 'text-slate-300'}`} />
                                </div>
                            );
                        })
                    )}
                </div>

                {/* Footer */}
                <div className="px-5 py-2.5 border-t border-slate-200 bg-slate-50 text-xs text-slate-400 flex-shrink-0">
                    Showing {filtered.length} of {items.length} · ↑↓ Navigate · Enter Select · Esc Close
                </div>
            </div>
        </div>
    );
};

export default SearchSelectModal;
