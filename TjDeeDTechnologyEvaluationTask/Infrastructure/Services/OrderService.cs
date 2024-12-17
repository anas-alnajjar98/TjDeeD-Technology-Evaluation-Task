using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.OrderDTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class OrderService:IOrderService
    {
        private readonly DbContextTask _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        private readonly ILogger<CategoryService> _logger;
        public OrderService(DbContextTask context, IHttpContextAccessor httpContextAccessor, ILogger<CategoryService> logger)
        {
            
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            
        }
        /// <summary>
        /// Creates a new order for a user and stores it in the database.
        /// </summary>
        /// <param name="createOrderDto">The details of the order to create.</param>
        /// <returns>The ID of the created order.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided order data is null.</exception>
        /// <exception cref="Exception">Thrown if an unexpected error occurs while creating the order or adding order items.</exception>
        public async Task<int> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
           
            _logger.LogInformation("Starting to create an order for UserId: {UserId}", createOrderDto.UserId);

            if (createOrderDto == null)
            {
                _logger.LogError("Received null order data.");
                throw new ArgumentNullException(nameof(createOrderDto), "Order data cannot be null.");
            }

            try
            {
              
                var order = new Order
                {
                    UserId = createOrderDto.UserId.ToString(),
                    TotalAmount = createOrderDto.TotalAmount,
                   OrderDate=DateTime.Now,
                   Status="Pending"
                };

               
                _logger.LogInformation("Adding the order to the database.");
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync(); 
                _logger.LogInformation("Order created successfully with OrderId: {OrderId}", order.OrderId);

               
                foreach (var item in createOrderDto.OrderItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        ProductName = _context.Products.Find(item.ProductId).ProductName,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };

                    _logger.LogInformation("Adding order item for ProductId: {ProductId} to the database.", item.ProductId);
                    await _context.OrderItems.AddAsync(orderItem);
                }

                await _context.SaveChangesAsync(); 
                _logger.LogInformation("Order items added successfully.");

                return order.OrderId; 
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "An error occurred while creating the order.");
                throw; 
            }
        }
        /// <summary>
        /// Retrieves an order by its ID, including the order items.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve.</param>
        /// <returns>The order with its associated items, or null if not found.</returns>
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems) 
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            return order; 
        }


    }
}
