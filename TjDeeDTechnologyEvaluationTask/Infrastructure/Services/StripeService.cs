using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Infrastructure.Data;
using Domain.Entities;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    /// <summary>
    /// Service for handling Stripe payments, including creating and confirming payment intents.
    /// </summary>
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly DbContextTask _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="StripeService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration containing the Stripe API key.</param>
        /// <param name="context">The database context for interacting with payment records.</param>
        public StripeService(IConfiguration configuration, DbContextTask context)
        {
            _configuration = configuration;
            _secretKey = _configuration["Stripe:ApiKey"];
            StripeConfiguration.ApiKey = _secretKey;
            _context = context;
        }

        /// <summary>
        /// Creates a payment intent with the specified amount and order ID.
        /// </summary>
        /// <param name="amount">The amount for the payment (in the currency's smallest unit, e.g., cents for USD).</param>
        /// <param name="orderId">The order ID associated with the payment.</param>
        /// <param name="currency">The currency code (default is "usd").</param>
        /// <returns>The created PaymentIntent object.</returns>
        public async Task<PaymentIntent> CreatePaymentIntent(decimal amount, int orderId, string currency = "usd")
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),  // Convert amount to smallest unit (e.g., cents)
                Currency = currency,
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
            {
                { "OrderId", orderId.ToString() }
            }
            };

            var service = new PaymentIntentService();
            PaymentIntent paymentIntent = await service.CreateAsync(options);

            // Create payment record in the database
            await CreatePaymentRecord(paymentIntent, orderId);

            return paymentIntent;
        }

        /// <summary>
        /// Confirms a payment intent with the given payment method.
        /// </summary>
        /// <param name="paymentIntentId">The ID of the PaymentIntent to confirm.</param>
        /// <param name="paymentMethodId">The ID of the payment method used to confirm the PaymentIntent.</param>
        /// <returns>The confirmed PaymentIntent object.</returns>
        public async Task<PaymentIntent> ConfirmPaymentIntent(string paymentIntentId, string paymentMethodId)
        {
            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethodId,
            };

            var service = new PaymentIntentService();
            PaymentIntent paymentIntent = await service.ConfirmAsync(paymentIntentId, options);

            // Update payment record with the payment status
            await UpdatePaymentRecord(paymentIntent);

            return paymentIntent;
        }

        /// <summary>
        /// Creates a payment record in the database when a payment intent is created.
        /// </summary>
        /// <param name="paymentIntent">The PaymentIntent object returned by Stripe.</param>
        /// <param name="orderId">The order ID associated with the payment.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task CreatePaymentRecord(PaymentIntent paymentIntent, int orderId)
        {
            var payment = new Payment
            {
                OrderId = orderId,
                Amount = paymentIntent.Amount / 100m,  
                PaymentIntentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the payment record in the database with the payment intent status.
        /// </summary>
        /// <param name="paymentIntent">The PaymentIntent object returned by Stripe.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task UpdatePaymentRecord(PaymentIntent paymentIntent)
        {
            var payment = await _context.Payments
                                         .FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntent.Id);

            if (payment != null)
            {
                payment.Status = paymentIntent.Status;
                payment.CreatedAt = DateTime.UtcNow;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
            }
        }
    }

}
