using System.Collections.Concurrent;

namespace backend.Services.Chat
{
    /// <summary>
    /// In-memory doctor presence + waiting-session queue for live chat handoff.
    /// Single-server only — a multi-instance deployment would need a shared
    /// backplane (e.g. Redis) for this to work across instances.
    /// </summary>
    public class DoctorPresenceTracker : IDoctorPresenceTracker
    {
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, byte>> _connectionsByDoctor = new();
        private readonly ConcurrentDictionary<int, bool> _availability = new();
        private readonly ConcurrentQueue<int> _waitingSessions = new();

        public void MarkConnected(int doctorId, string connectionId)
        {
            var connections = _connectionsByDoctor.GetOrAdd(doctorId, _ => new ConcurrentDictionary<string, byte>());
            connections[connectionId] = 0;
        }

        public void MarkDisconnected(int doctorId, string connectionId)
        {
            if (_connectionsByDoctor.TryGetValue(doctorId, out var connections))
            {
                connections.TryRemove(connectionId, out _);
                if (connections.IsEmpty)
                {
                    _availability.TryRemove(doctorId, out _);
                }
            }
        }

        public void SetAvailable(int doctorId)
        {
            if (IsConnected(doctorId))
            {
                _availability[doctorId] = true;
            }
        }

        public void SetUnavailable(int doctorId)
        {
            _availability[doctorId] = false;
        }

        public bool TryGetAvailableDoctor(out int doctorId)
        {
            foreach (var entry in _availability)
            {
                if (entry.Value && IsConnected(entry.Key))
                {
                    doctorId = entry.Key;
                    return true;
                }
            }

            doctorId = 0;
            return false;
        }

        public void Enqueue(int sessionId)
        {
            _waitingSessions.Enqueue(sessionId);
        }

        public bool TryDequeueForDoctor(out int sessionId)
        {
            return _waitingSessions.TryDequeue(out sessionId);
        }

        public IReadOnlyCollection<string> GetConnections(int doctorId)
        {
            if (_connectionsByDoctor.TryGetValue(doctorId, out var connections))
            {
                return connections.Keys.ToList();
            }

            return Array.Empty<string>();
        }

        private bool IsConnected(int doctorId) =>
            _connectionsByDoctor.TryGetValue(doctorId, out var connections) && !connections.IsEmpty;
    }
}
