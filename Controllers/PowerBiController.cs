using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using API.Modelo;
using IdentityModel.Client;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PowerBIController : ControllerBase
    {
        private readonly string clientId = "YOUR_CLIENT_ID";
        private readonly string tenantId = "YOUR_TENANT_ID";
        private readonly string clientSecret = "YOUR_CLIENT_SECRET";
        private readonly string authority = "https://login.microsoftonline.com/YOUR_TENANT_ID";
        private readonly string[] scopes = new string[] { "https://analysis.windows.net/powerbi/api/.default" };
        private readonly string groupId = "YOUR_GROUP_ID"; // Reemplaza con tu Group ID

        [HttpGet("embed-token")]
        public async Task<IActionResult> GetEmbedToken(string reportId)
        {
            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new System.Uri(authority))
                .Build();

            var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            var tokenCredentials = new TokenCredentials(authResult.AccessToken, "Bearer");
            using (var client = new PowerBIClient(new System.Uri("https://api.powerbi.com/"), tokenCredentials))
            {
                var generateTokenRequestParameters = new GenerateTokenRequest(TokenAccessLevel.View);

                // Usa el método GenerateTokenInGroupAsync
                var tokenResponse = await client.Reports.GenerateTokenInGroupAsync(groupId, reportId, generateTokenRequestParameters);

                var embedTokenResponse = new EmbedTokenResponse
                {
                    EmbedUrl = $"https://app.powerbi.com/reportEmbed?reportId={reportId}",
                    Token = tokenResponse.Token
                };

                return Ok(embedTokenResponse);
            }
        }
    }
}