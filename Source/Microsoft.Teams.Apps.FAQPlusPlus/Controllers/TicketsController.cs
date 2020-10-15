
namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Client;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    /// Creating <see cref="TicketsController"/> class with ControllerBase as base class. Controller for tickets APIs.
    /// </summary>
    [Authorize]
    [Route("api/tickets")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly TelemetryClient telemetryClient;
        private readonly ITicketsProvider ticketsProvider;
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IBotFrameworkHttpAdapter adapter;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketsController"/> class.
        /// </summary>
        /// <param name="telemetryClient">Singleton TelemetryClient instance used to send telemetry to Azure application insights.</param>
        /// <param name="ticketsProvider">Tickets provider</param>
        /// <param name="configurationProvider">Configuration Data provider</param>
        /// <param name="httpContextAccessor">http context</param>
        /// <param name="adapter">Bot adapter</param>
        /// <param name="configuration">Configuration</param>
        public TicketsController(
            TelemetryClient telemetryClient,
            ITicketsProvider ticketsProvider,
            IConfigurationDataProvider configurationProvider,
            IHttpContextAccessor httpContextAccessor,
            IBotFrameworkHttpAdapter adapter,
            IConfiguration configuration)
        {
            this.telemetryClient = telemetryClient;
            this.ticketsProvider = ticketsProvider;
            this.httpContextAccessor = httpContextAccessor;
            this.adapter = adapter;
            this.configuration = configuration;
            this.configurationProvider = configurationProvider;
        }

        /// <summary>
        /// Gets lists of tickets.
        /// </summary>
        /// <returns>A <see cref="Task"/>List of tickets.</returns>
        [HttpGet]
        public async Task<List<TicketEntity>> GetListofTickets()
        {
            IList<TicketEntity> ticketList = new List<TicketEntity>();
            try
            {
                string userName = this.HttpContext.User.FindFirst(ClaimTypes.Upn)?.Value;
                ticketList = await this.ticketsProvider.GetAllTicketAsync(userName);
            }
            catch (MsalException ex)
            {
                this.telemetryClient.TrackTrace($"An error occurred in GetListofTickets: {ex.Message}.", SeverityLevel.Error);
                this.telemetryClient.TrackException(ex);
                this.HttpContext.Response.ContentType = "text/plain";
                this.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await this.HttpContext.Response.WriteAsync("An authentication error occurred while acquiring a token for downstream API\n" + ex.ErrorCode + "\n" + ex.Message);
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackTrace($"An error occurred in GetListofTickets: {ex.Message}.", SeverityLevel.Error);
                this.telemetryClient.TrackException(ex);
                this.HttpContext.Response.ContentType = "text/plain";
                this.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await this.HttpContext.Response.WriteAsync("An error occurred while calling the downstream API\n" + ex.Message);
            }

            return ticketList.ToList();
        }

        /// <summary>
        /// Delete Ticket.
        /// </summary>
        /// <param name="ticketDetails">Ticket</param>
        /// <returns>Result</returns>
        [HttpPost]
        [Route("deleteTicketDetails")]
        public async Task<IActionResult> DeleteTicketDetails([FromBody]TicketEntity[] ticketDetails)
        {
            foreach (TicketEntity ticket in ticketDetails)
            {
                // Delete the ticket in  table
                await this.ticketsProvider.DeleteTicketAsync(ticket);
                this.telemetryClient.TrackTrace($"Ticket {ticket.TicketId} was deleted in store");
            }

            return this.Ok();
        }
    }
}
