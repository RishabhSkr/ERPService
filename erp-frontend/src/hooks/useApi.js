import { useState,useCallback} from 'react';
import { toast } from 'react-hot-toast';

const useApi = () => {
    const [loading, setLoading] = useState(false);
    
    const requestHandlerFunction = useCallback(async ( apiFunction, successMsg = null) => {
        setLoading(true);
        try {
            
            const result = await apiFunction(); // API Call execute karo
            const successMessage = result.data?.message || successMsg;
            if (successMessage) {
                toast.success(successMessage);
            }
            return { success: true, data: result }; 
            
        } catch (error) {
            console.log("API Error:", error);
            const errMsg = error.response?.data?.error || "Something went wrong";
            toast.error(errMsg);
            return { success: false, error: errMsg };
        }
        finally {
            setLoading(false);
        }
    }, []);
        return {
            loading,
            requestHandlerFunction
        }
};

export default useApi;