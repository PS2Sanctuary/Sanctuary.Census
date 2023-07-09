using Sanctuary.Census.RealtimeHub.Objects.Commands;
using Sanctuary.Census.RealtimeHub.Objects.Control;
using System.Collections.Generic;
using System.Linq;

namespace Sanctuary.Census.RealtimeHub.Objects;

/// <summary>
/// Contains information about the events that an ESS connection is subscribed to.
/// </summary>
public class EventSubscription
{
    private readonly HashSet<string> _events;
    private readonly HashSet<uint> _worlds;

    private bool _allWorlds;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventSubscription"/> class.
    /// </summary>
    public EventSubscription()
    {
        _events = new HashSet<string>();
        _worlds = new HashSet<uint>();
    }

    /// <summary>
    /// Checks whether the given <paramref name="worldId"/> has been subscribed to.
    /// </summary>
    /// <param name="worldId">The ID of the world.</param>
    /// <returns>Whether the world is subscribed to.</returns>
    public bool IsSubscribedToWorld(uint worldId)
        => _allWorlds || _worlds.Contains(worldId);

    /// <summary>
    /// Checks whether the given <paramref name="eventName"/> has been subscribed to.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>Whether the event is subscribed to.</returns>
    public bool IsSubscribedToEvent(string eventName)
        => _events.Contains("all") || _events.Contains(eventName);

    /// <summary>
    /// Adds to the subscription.
    /// </summary>
    /// <param name="subscription">The addition details.</param>
    public void AddSubscription(Subscribe subscription)
    {
        AddRange(_events, subscription.EventNames);

        if (subscription.Worlds is not { } worlds)
            return;

        if (worlds.IsT0)
            _allWorlds = true;

        if (worlds.IsT1)
            AddRange(_worlds, worlds.AsT1);
    }

    /// <summary>
    /// Subtracts from the subscription.
    /// </summary>
    /// <param name="clearSubscription">The clear details.</param>
    public void SubtractSubscription(ClearSubscribe clearSubscription)
    {
        if (clearSubscription.All)
        {
            _allWorlds = false;
            _worlds.Clear();
            _events.Clear();
            return;
        }

        if (clearSubscription.EventNames?.Contains("all") is true)
            _events.Clear();
        else
            RemoveRange(_events, clearSubscription.EventNames);

        if (clearSubscription.Worlds is { IsT0: true })
        {
            _allWorlds = false;
            _worlds.Clear();
        }
        else
        {
            RemoveRange(_worlds, clearSubscription.Worlds?.AsT1);
        }
    }

    /// <summary>
    /// Converts this subscription to a subscribe command.
    /// </summary>
    /// <returns></returns>
    public SubscriptionInformation ToSubscriptionInformation()
        => new(new Subscribe
        (
            _events,
            _allWorlds ? new All() : _worlds
        ));

    private static void AddRange<T>(ISet<T> set, IEnumerable<T>? collection)
    {
        if (collection is null)
            return;

        foreach (T item in collection)
                set.Add(item);
    }

    private static void RemoveRange<T>(ICollection<T> set, IEnumerable<T>? collection)
    {
        if (collection is null)
            return;

        foreach (T item in collection)
            set.Remove(item);
    }
}
