using MassTransit;

namespace Fiap.CloudGames.Application.Payments.Services;

/// <summary>
/// MassTransit implementation of IEventPublisher.
/// Publishes events via MassTransit/RabbitMQ.
/// Will be replaced by EventBridgeEventPublisher in Phase B (AWS integration).
/// </summary>
public class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task PublishAsync<T>(T @event, CancellationToken ct, string? tenantId = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            await _publishEndpoint.Publish(@event, ct);
            return;
        }

        await _publishEndpoint.Publish(@event, ctx => { ctx.Headers.Set("X-Tenant-Id", tenantId); ctx.Headers.Set("TenantId", tenantId); }, ct);
    }
}
