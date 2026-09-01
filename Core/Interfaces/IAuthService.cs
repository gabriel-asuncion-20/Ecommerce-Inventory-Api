using System.Threading.Tasks;
using EcommerceInventoryApi.Core.DTOs;

namespace EcommerceInventoryApi.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }
}
