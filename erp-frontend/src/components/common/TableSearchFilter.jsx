import React from 'react';
import { Search, X } from 'lucide-react';

/**
 * TableSearchFilter — Reusable inline search bar for tables
 * 
 * @param {string} value - Current search text
 * @param {function} onChange - Callback when search text changes
 * @param {string} placeholder - Placeholder text
 * @param {number} resultCount - Number of filtered results
 * @param {number} totalCount - Total number of items
 * @param {string} className - Additional classes for container
 */
const TableSearchFilter = ({
    value = '',
    onChange,
    placeholder = 'Search...',
    resultCount,
    totalCount,
    className = ''
}) => {
    return (
        <div className={`flex items-center gap-3 ${className}`}>
            <div className="relative flex-1 max-w-xs">
                <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                <input
                    type="text"
                    className="w-full bg-slate-800 text-white text-sm rounded-lg pl-9 pr-8 py-1.5 border border-slate-700 outline-none focus:border-blue-500 transition-colors"
                    placeholder={placeholder}
                    value={value}
                    onChange={(e) => onChange(e.target.value)}
                />
                {value && (
                    <button
                        onClick={() => onChange('')}
                        className="absolute right-2 top-1/2 -translate-y-1/2 text-slate-500 hover:text-slate-300 transition-colors"
                    >
                        <X size={14} />
                    </button>
                )}
            </div>
            {totalCount !== undefined && (
                <span className="text-xs text-slate-500 whitespace-nowrap">
                    {value ? `${resultCount} of ${totalCount}` : `${totalCount} total`}
                </span>
            )}
        </div>
    );
};

export default TableSearchFilter;
