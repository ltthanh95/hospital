using System.Collections.Concurrent;

namespace backend.Services.Chat
{
    /// Tracks the presence and availability of doctors, managing their connections and session queues.
    public class DoctorPresenceTracker : IDoctorPresenceTracker
    {
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, byte>> _connectionsByDoctor = new();
        private readonly ConcurrentDictionary<int, bool> _availability = new();
        private readonly ConcurrentQueue<int> _waitingSessions = new();

        /// Marks a doctor as connected by adding their connection ID.
        public void MarkConnected(int doctorId, string connectionId)
        {
            var connections = _connectionsByDoctor.GetOrAdd(doctorId, _ => new ConcurrentDictionary<string, byte>());
            connections[connectionId] = 0;
        }

        /// Marks a doctor as disconnected by removing their connection ID.
        /// If no connections remain, the doctor is removed from availability.
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
        /// Marks a doctor as available if they are connected.
        public void SetAvailable(int doctorId)
        {
            if (IsConnected(doctorId))
            {
                _availability[doctorId] = true;
            }
        }

        /// Marks a doctor as unavailable.
        public void SetUnavailable(int doctorId)
        {
            _availability[doctorId] = false;
        }

        /// Attempts to find an available doctor.
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

        /// Adds a session ID to the waiting queue.
        public void Enqueue(int sessionId)
        {
            _waitingSessions.Enqueue(sessionId);
        }

        /// Attempts to dequeue a session ID for a doctor.
        public bool TryDequeueForDoctor(out int sessionId)
        {
            return _waitingSessions.TryDequeue(out sessionId);
        }

        /// Retrieves all connection IDs for a specific doctor.
        public IReadOnlyCollection<string> GetConnections(int doctorId)
        {
            if (_connectionsByDoctor.TryGetValue(doctorId, out var connections))
            {
                return connections.Keys.ToList();
            }

            return Array.Empty<string>();
        }

        /// Checks if a doctor is connected.
        private bool IsConnected(int doctorId) =>
            _connectionsByDoctor.TryGetValue(doctorId, out var connections) && !connections.IsEmpty;
    }
}
