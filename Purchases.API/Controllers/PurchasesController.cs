﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Purchases.API.Helpers;
using Purchases.Domain.Services;
using ServicesDtoModels.Models.Identity;
using ServicesDtoModels.Models.Purchases;

namespace Purchases.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    //TODO: добавить миддлвеер для авторизации с кроликом
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchasesService _purchasesService;
        public PurchasesController(IPurchasesService purchasesService)
        {
            _purchasesService = purchasesService;
        }
        [HttpGet("{userid}/{id}")]
        public async Task<IActionResult> GetHistory(string id)
        {
            var user = HttpContext.Items["User"] as User;
            //TODO: проверить работает ли, когда Id пустой
            if (id != null && !int.TryParse(id, out _))
                return BadRequest($"Invalid id: {id}");
            return id is null ? Ok(await _purchasesService.GetTransactions(user)) 
                : Ok(await _purchasesService.GetTransactionById(user, int.Parse(id)));
        }
        [HttpPost("{userid}")]
        public async Task<IActionResult> AddTransaction([FromBody] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                var user     = HttpContext.Items["User"] as User;
                await _purchasesService.AddTransaction(user, transaction);
                return Ok();
            }
            return BadRequest("Invalid request");
        }
        /// <summary>
        /// Обновить транзакцию, если можно
        /// </summary>
        /// <returns></returns>
        [HttpPut("{userid}")]
        public async Task<IActionResult> UpdateTransaction( [FromBody] UpdateTransaction updateTransaction)
        {
            if (ModelState.IsValid)
            {
                var user = HttpContext.Items["User"] as User;
                var (content, isSuccess) = await _purchasesService.UpdateTransaction(user, updateTransaction);
                if (isSuccess)
                    return Ok();
                return Forbid(content);
            }
            return BadRequest("Invalid request");
        }
    }
}
