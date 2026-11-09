using Invoices.Api.Auth;
using Invoices.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Invoices.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(JwtTokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    [Consumes("application/json")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, ProblemJson)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var username = request.Username!.Trim();
        if (!tokens.ValidateCredentials(username, request.Password!))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Credenciales inválidas",
                detail: "Usuario o contraseña incorrectos.");
        }

        var token = tokens.CreateToken(username);
        return Ok(new LoginResponse(token.AccessToken, "Bearer", token.ExpiresAt, username));
    }
}
