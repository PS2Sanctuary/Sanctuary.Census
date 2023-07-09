using Sanctuary.Census.RealtimeHub.Objects.Commands;

namespace Sanctuary.Census.RealtimeHub.Objects.Control;

/// <summary>
/// Used to send information about a subscription.
/// </summary>
/// <param name="Subscription">The current subscription.</param>
public record SubscriptionInformation(Subscribe Subscription);
