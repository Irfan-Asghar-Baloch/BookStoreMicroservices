using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTO;
using OrderService.Interface;
using OrderService.Interface.IOrderService;
using OrderService.ServiceImplementation;
using System.Security.Claims;
namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IAiAgentService _aiAgentService;
        private readonly IPaymentService _paymentService;
        public OrderController(IOrderService orderService, IAiAgentService aiAgentService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _aiAgentService = aiAgentService;
            _paymentService = paymentService;
        }

        [HttpPost("ask-agent")]
        public async Task<IActionResult> Ask(AskAiRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { success = false, message = "Message is required" });

            var answer = await _aiAgentService.AskAsync(request.Message, request.History);

            if (answer == null)
                return StatusCode(502, new { success = false, message = "AI service is unavailable" });

            return Ok(new { success = true, answer });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var response = await _orderService.GetAllAsync();

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var response = await _orderService.GetMyOrdersAsync(userId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
           // var response = await _orderService.GetByIdAsync(id);
           var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);    
            var canAccess = User.IsInRole("Admin") || User.IsInRole("Service");
            var response = await _orderService.GetByIdAsync(id, userId, canAccess);
            if (!response.Success && response.Message == "Access denied.")
                return StatusCode(403, response);

            return Ok(response);
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var canAccess = User.IsInRole("Admin") || User.IsInRole("Service") || User.IsInRole("User"); ;
            var response = await _orderService.CancelAsync(id, userId, canAccess);

            if (!response.Success && response.Message == "You do not have permission to cancel this order.")
                return StatusCode(403, response);

            return Ok(response);
        }


        [HttpPatch("{id}/refund")]
        public async Task<IActionResult> Refund(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var canAccessAny = User.IsInRole("Admin") || User.IsInRole("Service")|| User.IsInRole("User");

            var result = await _paymentService.RefundAsync(id, userId, canAccessAny);

            if (!result.Success && result.Message == "Access denied.")
                return StatusCode(403, result);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create(CreateOrderRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var response = await _orderService.CreateAsync(request, userId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
