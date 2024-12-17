using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stripe;

namespace Application.Interfaces
{
    public interface IStripeService
    {
        
        Task<PaymentIntent> CreatePaymentIntent(decimal amount, int orderId, string currency = "usd");

       
        Task<PaymentIntent> ConfirmPaymentIntent(string paymentIntentId, string paymentMethodId);
    }
}
