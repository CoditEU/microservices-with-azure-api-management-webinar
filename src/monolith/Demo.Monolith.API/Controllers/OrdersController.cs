﻿using Demo.Monolith.API.Contracts.v1;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading.Tasks;
using Demo.Monolith.API.Managers;
using Demo.Monolith.API.OpenAPI;
using Demo.Monolith.API.Repositories.Interfaces;

namespace Demo.Monolith.API.Controllers
{
    [Route("api/v1/orders")]
    [ApiExplorerSettings(GroupName = OpenApiCategories.Orders)]
    public class OrdersController : Controller
    {
        private readonly OrderManager _orderManager;
        private readonly IOrderRepository _orderRepository;

        public OrdersController(OrderManager orderManager, IOrderRepository orderRepository)
        {
            _orderManager = orderManager;
            _orderRepository = orderRepository;
        }

        /// <summary>
        ///     Create New Order
        /// </summary>
        /// <remarks>Provide capability create a new order for products from our catalog</remarks>
        /// <response code="201">Order was successfully created</response>
        /// <response code="503">Something went wrong, please contact support</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderConfirmation), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "Order_Create")]
        public async Task<IActionResult> Post([FromBody]Order orderRequest)
        {
            var orderConfirmation = await _orderManager.CreateAsync(orderRequest);
            return CreatedAtAction(nameof(Get), new { ConfirmationId = orderConfirmation.ConfirmationId }, orderConfirmation);
        }

        /// <summary>
        ///     Get Order
        /// </summary>
        /// <remarks>Provide information about a previously created order</remarks>
        /// <response code="200">Information about a specific order</response>
        /// <response code="400">Request was invalid</response>
        /// <response code="404">Requested product was not found</response>
        /// <response code="503">Something went wrong, please contact support</response>
        [HttpGet("{confirmationId}")]
        [ProducesResponseType(typeof(Order), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "Order_Get")]
        public async Task<IActionResult> Get(string confirmationId)
        {
            var foundOrder = await _orderRepository.GetAsync(confirmationId);
            if (foundOrder == null)
            {
                return NotFound();
            }

            return Ok(foundOrder);
        }
    }
}
