using Application.DTOs.OrderDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        private readonly IStripeService _stripeService;
        public OrderController(ILogger<OrderController> logger, IOrderService orderService, IStripeService stripeService)
        {
            _logger = logger;
            
        _orderService = orderService;
            _stripeService = stripeService;
        }
        /// <summary>
        /// Creates a new order for a user.
        /// </summary>
        /// <param name="createOrderDto">The details of the order to create.</param>
        /// <returns>The created order, or an error message if the order creation fails.</returns>
        /// <response code="201">Returns the created order details.</response>
        /// <response code="400">Returns bad request if the order data is null or invalid.</response>
        /// <response code="500">Returns internal server error if an unexpected error occurs during order creation.</response>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            _logger.LogInformation("Received request to create order for UserId: {UserId}", createOrderDto.UserId);

            if (createOrderDto == null)
            {
                _logger.LogWarning("Order data is null.");
                return BadRequest("Order data is required.");
            }

            try
            {
                _logger.LogInformation("Starting order creation for UserId: {UserId}", createOrderDto.UserId);

                var orderId = await _orderService.CreateOrderAsync(createOrderDto);

                _logger.LogInformation("Order created successfully with OrderId: {OrderId}", orderId);

                
                return CreatedAtAction(nameof(GetOrderById), new { orderId = orderId }, createOrderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the order for UserId: {UserId}", createOrderDto.UserId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        /// <summary>
        /// Retrieves an order by its ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve.</param>
        /// <returns>The order details if found, or a not found message if the order doesn't exist.</returns>
        /// <response code="200">Returns the order details if the order is found.</response>
        /// <response code="404">Returns not found if no order with the specified ID exists.</response>

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            _logger.LogInformation("Fetching order with OrderId: {OrderId}", orderId);

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                _logger.LogWarning("Order with OrderId: {OrderId} not found.", orderId);
                return NotFound($"Order with ID {orderId} not found.");
            }

            _logger.LogInformation("Order with OrderId: {OrderId} retrieved successfully.", orderId);
            return Ok(order);
        }
        /// <summary>
        /// Initiates the payment process by creating an order and generating a payment intent.
        /// </summary>
        /// <param name="createOrderDto">The order details to be created, including customer information, items, and total amount.</param>
        /// <returns>An HTTP response containing the payment intent ID and client secret if successful, or an error message if failed.</returns>
        /// <response code="200">Returns the payment intent ID and client secret.</response>
        /// <response code="400">Returns an error if order creation fails.</response>
        /// <response code="404">Returns an error if the created order is not found.</response>
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreateOrderDto createOrderDto)
        {
            var orderId = await _orderService.CreateOrderAsync(createOrderDto);

            if (orderId <= 0)
            {
                return BadRequest("Order creation failed.");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            var paymentIntent = await _stripeService.CreatePaymentIntent(order.TotalAmount, orderId);

            return Ok(new { PaymentIntentId = paymentIntent.Id, ClientSecret = paymentIntent.ClientSecret });
        }

    }
}
