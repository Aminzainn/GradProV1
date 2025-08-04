using GP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;
using System.Text.Json;
using Stripe.Events;
namespace GP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly CustomerService _customerService;
        private readonly ChargeService _chargeService;
        private readonly ProductService _productService;
        private readonly PriceService _priceService;
        private readonly IConfiguration _configuration;
        private readonly EventManagerContext _context;

        public StripeController(
            TokenService tokenService,
            CustomerService customerService,
            ChargeService chargeService,
            ProductService productService,
            PriceService priceService,
            IConfiguration configuration,
            EventManagerContext context)
        {
            _tokenService = tokenService;
            _customerService = customerService;
            _chargeService = chargeService;
            _productService = productService;
            _priceService = priceService;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] StripeCustomerRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { error = "Invalid customer data." });

            try
            {
                var customerOptions = new CustomerCreateOptions
                {
                    Email = request.Email,
                    Name = request.Name,
                    Source = request.Token
                };

                var customer = await _customerService.CreateAsync(customerOptions);
                return Ok(new { CustomerId = customer.Id });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("events")]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { error = "Invalid event data." });

            try
            {
                var metadata = new Dictionary<string, string>
                {
                    { "eventDate", request.EventDate.ToString("yyyy-MM-dd") },
                    { "venue", request.Venue ?? string.Empty }
                };
                if (request.Metadata != null)
                {
                    foreach (var kv in request.Metadata)
                        metadata[kv.Key] = kv.Value;
                }

                var productOptions = new ProductCreateOptions
                {
                    Name = request.Name,
                    Description = request.Description,
                    Active = true,
                    Metadata = metadata
                };

                var product = await _productService.CreateAsync(productOptions);

                var priceOptions = new PriceCreateOptions
                {
                    Product = product.Id,
                    UnitAmount = (long)(request.Price * 100),
                    Currency = "usd",
                };

                var price = await _priceService.CreateAsync(priceOptions);

                return Ok(new { ProductId = product.Id, PriceId = price.Id });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("checkout/session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutSessionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PriceId) || string.IsNullOrWhiteSpace(request.DomainUrl))
                return BadRequest(new { error = "Invalid checkout session data." });

            try
            {
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    CustomerEmail = request.CustomerEmail,
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = request.PriceId,
                            Quantity = request.Quantity
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = $"{request.DomainUrl}/payment/success?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{request.DomainUrl}/payment/cancelled"
                };

                var service = new Stripe.Checkout.SessionService(); // Explicitly specify Checkout SessionService
                var session = await service.CreateAsync(options);

                return Ok(new { SessionId = session.Id, SessionUrl = session.Url });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("customer-portal")]
        public async Task<IActionResult> CreateCustomerPortalSession([FromBody] CustomerPortalRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CustomerId) || string.IsNullOrWhiteSpace(request.ReturnUrl))
                return BadRequest(new { error = "Invalid portal session data." });

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = request.CustomerId,
                ReturnUrl = request.ReturnUrl
            };
            var service = new Stripe.BillingPortal.SessionService(); // Explicitly specify BillingPortal SessionService
            var session = await service.CreateAsync(options);
            return Ok(new { url = session.Url });
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents()
        {
            try
            {
                var options = new ProductListOptions
                {
                    Active = true,
                    Limit = 100
                };

                var products = await _productService.ListAsync(options);
                return Ok(products.Data);
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("payment/success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] string sessionId)
        {
            try
            {
                var service = new Stripe.Checkout.SessionService(); // Explicitly specify Checkout SessionService
                var session = await service.GetAsync(sessionId);

                using (var context = _context)
                {
                    var payment = await context.Payments
                        .FirstOrDefaultAsync(p => p.TransactionRef == sessionId); // Use TransactionRef

                    if (payment == null)
                        return BadRequest(new { error = "Payment not found." });

                    return Ok(new
                    {
                        Status = session.PaymentStatus,
                        ReservationId = payment.ReservationId,
                        Amount = session.AmountTotal / 100m
                    });
                }
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class StripeCustomerRequest
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
    }

    public class EventCreateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime EventDate { get; set; }
        public string Venue { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class CheckoutSessionRequest
    {
        public string PriceId { get; set; }
        public int Quantity { get; set; }
        public string CustomerEmail { get; set; }
        public string DomainUrl { get; set; }
    }

    public class CustomerPortalRequest
    {
        public string CustomerId { get; set; }
        public string ReturnUrl { get; set; }
    }
}