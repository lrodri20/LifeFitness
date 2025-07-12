// Controllers/ChatsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFitnessApi.Data.Dtos;
using SmartFitnessApi.Services;
using System.Security.Claims;

namespace SmartFitnessApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Returns a list of chats for the current user.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatDto>>> Get()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var chats = await _chatService.GetChatsAsync(userId);
            return Ok(chats);
        }
        [HttpGet("{chatId}/messages")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(
            [FromRoute] int chatId)
        {
            // assume NameIdentifier claim holds the numeric user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var messages = await _chatService.GetMessagesAsync(chatId, userId);
            return Ok(messages);
        }
    }
}
