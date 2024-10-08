﻿using System.Threading.RateLimiting;

namespace BattleTrace.Common;

public sealed class RateLimitingHandler : DelegatingHandler
{
    private readonly RateLimiter _limiter;

    public RateLimitingHandler(RateLimiter limiter)
    {
        _limiter = limiter;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var lease = await _limiter.AcquireAsync(1, cancellationToken);

        if (!lease.IsAcquired)
            throw new InvalidOperationException();

        return await base.SendAsync(request, cancellationToken);
    }
}
