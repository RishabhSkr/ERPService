import { Zap, AlertTriangle, BarChart3, Factory, Package } from 'lucide-react';

const SmartBatchCard = ({ planningInfo, onUseSuggestion }) => {
    if (!planningInfo) return null;

    const {
        productName,
        remainingQuantity,
        machineDailyCapacity,
        maxPossibleByMaterial,
        limitingMaterial,
        suggestedBatches,
        suggestedBatchSize,
        batchSizes,
        minEfficiency,
        fullCapacityBatches
    } = planningInfo;

    const isLimited = maxPossibleByMaterial < remainingQuantity;

    return (
        <div className="bg-gradient-to-br from-blue-50 to-purple-50 p-5 rounded-xl border border-blue-200 shadow-sm">
            
            {/* Header */}
            <h3 className="font-bold text-blue-900 flex items-center gap-2 mb-4 text-base">
                <Zap size={18} className="text-yellow-500" />
                Smart Batch Suggestion
            </h3>

            {/* Product + Remaining */}
            <div className="bg-white p-3 rounded-lg border border-blue-100 mb-3">
                <span className="block text-gray-500 text-xs uppercase">Product</span>
                <span className="font-semibold text-gray-800">{productName}</span>
                <div className="flex gap-4 mt-2 text-xs text-gray-600">
                    <span>Remaining: <strong className="text-blue-700">{remainingQuantity}</strong></span>
                    <span>Capacity/Day: <strong>{machineDailyCapacity}</strong></span>
                </div>
            </div>

            {/* Suggestion Grid */}
            <div className="grid grid-cols-2 gap-2 mb-3">
                <div className="bg-white p-3 rounded-lg border border-green-100 text-center">
                    <Factory size={16} className="mx-auto text-green-600 mb-1" />
                    <span className="block text-xs text-gray-500">Batches</span>
                    <span className="font-bold text-green-700 text-lg">{suggestedBatches}</span>
                </div>
                <div className="bg-white p-3 rounded-lg border border-purple-100 text-center">
                    <Package size={16} className="mx-auto text-purple-600 mb-1" />
                    <span className="block text-xs text-gray-500">Batch Size</span>
                    <span className="font-bold text-purple-700 text-lg">{suggestedBatchSize}</span>
                </div>
            </div>

            {/* Batch Sizes Breakdown */}
            {batchSizes && batchSizes.length > 0 && (
                <div className="bg-white p-3 rounded-lg border border-gray-100 mb-3">
                    <span className="block text-xs text-gray-500 mb-2 flex items-center gap-1">
                        <BarChart3 size={12} /> Batch Breakdown
                    </span>
                    <div className="flex flex-wrap gap-1.5">
                        {batchSizes.map((size, i) => (
                            <span key={i} className="bg-blue-100 text-blue-700 px-2 py-0.5 rounded text-xs font-mono font-semibold">
                                B{i + 1}: {size}
                            </span>
                        ))}
                    </div>
                </div>
            )}

            {/* Efficiency + Stats */}
            <div className="flex gap-2 mb-3 text-xs">
                <div className="bg-white flex-1 p-2 rounded border text-center">
                    <span className="block text-gray-400">Full Cap. Batches</span>
                    <span className="font-bold text-green-600">{fullCapacityBatches}/{suggestedBatches}</span>
                </div>
                <div className="bg-white flex-1 p-2 rounded border text-center">
                    <span className="block text-gray-400">Min Efficiency</span>
                    <span className={`font-bold ${minEfficiency >= 70 ? 'text-green-600' : 'text-orange-600'}`}>
                        {minEfficiency?.toFixed(1)}%
                    </span>
                </div>
            </div>

            {/* Material Warning */}
            {isLimited && limitingMaterial && (
                <div className="bg-amber-50 p-2.5 rounded border border-amber-200 flex items-start gap-2 mb-3">
                    <AlertTriangle size={14} className="text-amber-500 mt-0.5 flex-shrink-0" />
                    <div className="text-xs text-amber-800">
                        <strong>Material limit:</strong> Max <strong>{maxPossibleByMaterial}</strong> units possible.
                        Limited by <strong>{limitingMaterial}</strong>
                    </div>
                </div>
            )}

            {/* Use Suggestion Button */}
            {onUseSuggestion && (
                <button
                    onClick={() => onUseSuggestion(suggestedBatchSize)}
                    className="w-full bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg text-sm font-medium transition-colors flex items-center justify-center gap-2"
                >
                    Suggested Batch Size: {suggestedBatchSize}
                </button>
            )}
        </div>
    );
};

export default SmartBatchCard;
