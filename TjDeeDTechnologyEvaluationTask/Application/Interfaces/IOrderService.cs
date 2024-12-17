using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.OrderDTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<Order> GetOrderByIdAsync(int orderId);
        //Task<bool> UpdateOrderAsync(int orderId, UpdateOrderDto updateOrderDto);
    }
}
