using System.Security.Claims;
using HireMe.Contracts.JobConnection.Requests;
using HireMe.Enums;
using HireMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireMe.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class JobConnectionsController : ControllerBase
    {
        private readonly IJobConnectionService _jobConnectionService;

        public JobConnectionsController(IJobConnectionService jobConnectionService)
        {
            _jobConnectionService = jobConnectionService;
        }

        /// <summary>
        /// Cancels an active job connection
        /// </summary>
        /// <remarks>
        /// This endpoint allows either the worker or employer to cancel an active job connection.
        /// The user type is determined from the authentication token.
        /// Only the worker or employer associated with the connection can cancel it.
        /// 
        /// Sample request:
        /// 
        ///     PUT /JobConnections/5/cancel
        ///     
        /// </remarks>
        /// <param name="jobConnectionId">The unique identifier of the job connection to cancel</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <response code="204">Job connection cancelled successfully</response>
        /// <response code="400">Bad request - Possible errors:
        /// - JobConnectionNotFound: The specified job connection does not exist
        /// - JobConnectionNotActive: The connection is not in Active status
        /// - UnauthorizedCancellation: User is not the worker or employer of this connection
        /// </response>
        /// <response code="401">Unauthorized - User must be authenticated</response>
        [HttpPut("{jobConnectionId}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelJobConnection(
            [FromRoute] int jobConnectionId, 
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            
            var result = await _jobConnectionService.CancelJobConnectionAsync(userId!, jobConnectionId, userRoles, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }
    }
}
