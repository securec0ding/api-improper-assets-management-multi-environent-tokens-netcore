using Backend.Model;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/api/v2")]
    public class AccountControllerV2 : ControllerBase
    {
        private IIdentityService _identityService;
        private IAccountService _accountService;

        public AccountControllerV2(IIdentityService identityService, IAccountService accountService)
        {
            _identityService = identityService;
            _accountService = accountService;
        }

        [HttpGet]
        [Route("account/{accountId}")]
        [Authorize(Policy = "OnlyForAccountHolders")]
        public async Task<IActionResult> Account([FromRoute] string accountId)
        {
            var user = await GetUser();
            var account = _accountService.GetAccountById(user.Id, Guid.Parse(accountId));

            if (account == null)
                return NotFound(new ErrorModel { Message = "Account not found" });

            var response = new BankAccountResponse(account);
            return Ok(response);
        }

        [HttpGet]
        [Route("auditors/accounts")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = "OnlyForAuditors")]
        public async Task<IActionResult> GetAllAccountsForAuditors()
        {
            var accounts = await _accountService.GetAccounts();
            var accountResponses = new List<BankAccountResponse>();
            accounts.ForEach(x => { accountResponses.Add(new BankAccountResponse(x)); });
            return Ok(accountResponses);
        }

        private async Task<UserModel> GetUser()
        {
            var userName = HttpContext.User.Identity.Name;
            var user = await _identityService.GetUserAsync(userName);

            return user;
        }
    }
}