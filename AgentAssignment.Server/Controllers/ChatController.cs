using AgentAssignment.Server.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AgentAssignment.Server.Controllers;

[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatCompletionService _chatCompletionService;

    public ChatController(IChatCompletionService chatCompletionService)
    {
        _chatCompletionService = chatCompletionService;
    }

    [HttpPost("/v1/chat/completions")]
    public async Task<IActionResult> CompleteAsync(
        [FromBody] ChatCompletionRequest request, CancellationToken cancellationToken)
    {
        if (request.Messages is null || request.Messages.Count == 0)
        {
            return BadRequest(new ApiErrorResponse(
                new ApiErrorBody("Messages must not be null or empty.",
                    "invalid_request_error")));
        }

        if (!_chatCompletionService.IsReady)
        {
            return StatusCode(503, new ApiErrorResponse(
                new ApiErrorBody("The model is not ready yet.",
                    "service_unavailable")));
        }

        try
        {
            var response = await _chatCompletionService.CompleteAsync(
                request,
                cancellationToken);

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiErrorResponse(
                new ApiErrorBody(
                    "An error occurred while processing the chat completion.",
                    "internal_server_error")));
        }
    }

    [HttpGet("/health")]
    public IActionResult Health()
    {
        if (_chatCompletionService.IsReady)
        {
            return Ok(new { status = "ready" });
        }

        return StatusCode(503, new { status = "starting" });
    }

    

}