import React, { useState } from 'react';
import { ChevronDown, X } from 'lucide-react';
import SearchSelectModal from './SearchSelectModal';

/**
 * SearchSelect — Inline trigger that opens SearchSelectModal
 * 
 * Drop-in replacement for <select>. Shows current selection as a 
 * clickable button/pill, opens searchable modal on click.
 * 
 * @param {string} value - Currently selected value (id)
 * @param {string} displayValue - Text to show for current selection
 * @param {string} placeholder - Placeholder when nothing selected
 * @param {function} onSelect - Callback with selected item object
 * @param {function} onClear - Optional callback to clear selection
 * @param {Array} items - Items array for the modal
 * @param {string} title - Modal title
 * @param {Array} displayFields - Column definitions for modal
 * @param {Array} searchKeys - Fields to search in
 * @param {string} valueKey - Primary key field name
 * @param {string} className - Additional CSS classes for the trigger
 * @param {boolean} required - Show required indicator
 * @param {boolean} disabled - Disable the component
 * @param {string} size - 'sm' | 'md' (default: 'md')
 */
const SearchSelect = ({
    value = '',
    displayValue = '',
    placeholder = 'Select...',
    onSelect,
    onClear,
    items = [],
    title = 'Select',
    displayFields = [],
    searchKeys = [],
    valueKey = 'id',
    className = '',
    required = false,
    disabled = false,
    size = 'md',
}) => {
    const [isOpen, setIsOpen] = useState(false);

    const sizeClasses = size === 'sm'
        ? 'px-2 py-1.5 text-xs min-h-[30px]'
        : 'px-3 py-2.5 text-sm min-h-[38px]';

    return (
        <>
            <div
                onClick={() => !disabled && setIsOpen(true)}
                className={`
                    relative flex items-center justify-between gap-1 
                    border border-slate-300 rounded-lg bg-white
                    transition cursor-pointer
                    ${disabled ? 'opacity-50 cursor-not-allowed bg-slate-50' : 'hover:border-blue-500 hover:bg-blue-50 hover:shadow-sm'}
                    ${sizeClasses}
                    ${className}
                `}
            >
                <span className={`truncate ${value ? 'text-slate-800' : 'text-slate-400'}`}>
                    {value ? displayValue : placeholder}
                </span>
                <div className="flex items-center gap-0.5 flex-shrink-0">
                    {value && onClear && !disabled && (
                        <button
                            type="button"
                            onClick={(e) => { e.stopPropagation(); onClear(); }}
                            className="p-0.5 rounded hover:bg-slate-100 text-slate-400 hover:text-slate-600"
                        >
                            <X size={12} />
                        </button>
                    )}
                    <ChevronDown size={14} className="text-slate-400" />
                </div>
            </div>

            <SearchSelectModal
                isOpen={isOpen}
                onClose={() => setIsOpen(false)}
                onSelect={onSelect}
                title={title}
                items={items}
                displayFields={displayFields}
                searchKeys={searchKeys}
                valueKey={valueKey}
                currentValue={value}
            />
        </>
    );
};

export default SearchSelect;
