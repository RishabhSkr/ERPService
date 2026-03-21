using MyERP.Services.Production.DTOs.MRP;

namespace MyERP.Services.Production.Services.MRP
{
    public interface IMRPService
    {
        Task<MRPResultDto> RunAsync(RunMRPRequestDto request);
    }
}
