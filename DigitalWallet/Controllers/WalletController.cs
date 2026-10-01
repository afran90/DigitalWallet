using DigitalWallet.Common.DTO;
using DigitalWallet.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalWallet.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> SubmitTransaction(SumittedTransaction userDto)
        {
            var employee = await _walletService.SubmitTransaction(userDto);
            return Ok(employee);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> WalletSummary([FromQuery] string walletId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var summary = await _walletService.GetSummary(walletId, from, to);


            return Ok(summary);
        }


    }
}
