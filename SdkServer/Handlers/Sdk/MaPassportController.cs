// TODO: Since these stuff requires client patch, we will disable it for now

// using Microsoft.AspNetCore.Mvc;
// using NahidaImpact.Database.Repositories;
// using NahidaImpact.SdkServer.Models;
// using NahidaImpact.SdkServer.Models.Sdk;
//
// namespace NahidaImpact.SdkServer.Handlers.Sdk;
//
// [ApiController]
// public class MaPassportController : ControllerBase
// {
//     [HttpPost("/{productName}/account/ma-passport/api/appLoginByPassword")]
//     public async Task<IActionResult> AppLoginByPassword(string productName,
//         [FromBody] AppLoginByPasswordRequest request)
//     {
//         var account = AccountRepository.FindAccountByUsername(request.Account);
//
//         // Make new account
//         if (account == null)
//         {
//             var (success, accountUid) = await AccountRepository.CreateAccount(request.Account, request.Password);
//             if (!success)
//             {
//                 return Ok(new ResponseBase
//                 {
//                     Retcode = -101,
//                     Message = "Failed to create account"
//                 });
//             }
//
//             account = AccountRepository.FindAccountByAccountUid(accountUid);
//         }
//
//
//         return Ok(new AppLoginByPasswordResponse
//         {
//             Data = new AppLoginByPasswordResponse.AppLoginByPasswordResponseData
//             {
//             }
//         });
//     }
//
//     [HttpPost("/{productName}/account/ma-passport/api/logout")]
//     public IActionResult Logout(string productName, [FromBody] LogoutRequest request)
//     {
//         return Ok(new ResponseBase());
//     }
// }