// const getEfficiencyWarning = () => {
//         if (!planningInfo || !formData.quantityToProduce) return null;
//         if(formData.quantityToProduce <= 0) return null;
//         const capacity = planningInfo.machineDailyCapacity;
//         const availableQtyToProduce = planningInfo.remainingQuantity;

//         if (capacity <= 0) return null;
        
//         const qty = parseFloat(formData.quantityToProduce);
//         const lastBatch = qty % capacity;
        
//         if(qty>availableQtyToProduce){
//             return {
//                 availableQtyToProduce,
//                 lastBatch,
//                 capacity
//             }
//         }
        
//         // Agar perfectly divisible hai toh koi issue nahi
//         if (lastBatch === 0) return null;
        
//         const efficiency = (lastBatch / capacity) * 100;
        
//         if (efficiency < 50) {
//             return {
//                 efficiency: efficiency.toFixed(1),
//                 lastBatch,
//                 capacity
//             };
//         }
//         return null;
//     };

// src/utils/batchUtils.js

export const getEfficiencyWarning = (planningInfo, quantityToProduce) => {
    if (!planningInfo || !quantityToProduce) return null;
    if (quantityToProduce <= 0) return null;

    const capacity = planningInfo.machineDailyCapacity;
    const availableQtyToProduce = planningInfo.remainingQuantity;

    if (capacity <= 0) return null;

    const qty = parseFloat(quantityToProduce);

    if (qty > availableQtyToProduce) {
        return {
            type: 'overLimit',
            availableQtyToProduce,
            lastBatch: qty % capacity,
            capacity
        };
    }

    const lastBatch = qty % capacity;
    if (lastBatch === 0) return null;

    const efficiency = (lastBatch / capacity) * 100;

    if (efficiency < 50) {
        return {
            type: 'lowEfficiency',
            efficiency: efficiency.toFixed(1),
            lastBatch,
            capacity
        };
    }
    return null;
};