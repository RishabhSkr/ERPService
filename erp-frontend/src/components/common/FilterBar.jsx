import React from 'react';
import { Search, X } from 'lucide-react';

const FilterBar = ({ 
    value,          // Input ka current value (State from parent)
    onChange,       // Value change hone par function (setState)
    onSearch,       // Search button click hone par function (API Call)
    onClear,        // Clear button click hone par function (Reset)
    placeholder = "Custom Search...", 
    type = "text"  
}) => {

    const handleSubmit = (e) => {
        e.preventDefault();
        onSearch(); // Parent ka search logic trigger karo
    };

    return (
        <div className="relative w-full sm:w-72">
            {/* Search Icon (left) */}
            <Search className="absolute left-2.5 top-2.5 text-gray-400 pointer-events-none" size={16} />
            
            {/* Input Field */}
            <input 
                type={type} 
                placeholder={placeholder} 
                className="w-full border border-gray-300 rounded-md py-2 pl-9 pr-8 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 transition-shadow"
                value={value}
                onChange={(e) => onChange(e.target.value)}
            />

            {/* Clear X Button (inside input, right side) */}
            {value && (
                <button 
                    type="button"
                    onClick={onClear}
                    className="absolute right-2 top-2 text-gray-400 hover:text-red-500 transition-colors"
                    title="Clear"
                >
                    <X size={16} />
                </button>
            )}
        </div>
    );
};

export default FilterBar;