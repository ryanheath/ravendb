using System;
using System.Collections.Generic;
using System.Threading;

namespace Raven.Client.Http
{
    public class NodeSelector
    {
        private class NodeSelectorState
        {
            public readonly Topology Topology;
            public readonly int CurrentNodeIndex;
            public readonly List<ServerNode> Nodes;
            public readonly int[] Failures; 

            public NodeSelectorState(int currentNodeIndex, Topology topology)
            {
                Topology = topology;
                CurrentNodeIndex = currentNodeIndex;
                Nodes = topology.Nodes;
                Failures = new int[topology.Nodes.Count];
            }
        }        

        private NodeSelectorState _state;

        public Topology Topology => _state.Topology;

        public NodeSelector(Topology topology)
        {
            _state = new NodeSelectorState(0, topology);
        }

        public void OnFailedRequest(int nodeIndex)
        {
            var state = _state;
            if (nodeIndex < 0 || nodeIndex >= state.Failures.Length)
                return; // probably already changed

            Interlocked.Increment(ref state.Failures[nodeIndex]);
        }

        public bool OnUpdateTopology(Topology topology, bool forceUpdate = false)
        {
            if (topology == null)
                return false;

            if (_state.Topology.Etag >= topology.Etag && forceUpdate == false)
                return false;

            var state = new NodeSelectorState(0, topology);

            Interlocked.Exchange(ref _state, state);

            return true;
        }

        public (int Index, ServerNode Node) GetPreferredNode()
        {
            var state = _state;
            for (int i = 0; i < state.Failures.Length; i++)
            {
                if (state.Failures[i] == 0)
                    return (i, state.Nodes[i]);
            }

            return UnlikelyEveryoneFaultedChoice(state);
        }

        private static ValueTuple<int, ServerNode> UnlikelyEveryoneFaultedChoice(NodeSelectorState state)
        {
            // if there are all marked as failed, we'll chose the first
            // one so the user will get an error (or recover :-) );
            return (0, state.Nodes[0]);
        }

        public (int Index, ServerNode Node) GetNodeBySessionId(int sessionId)
        {
            var state = _state;
            var index = sessionId % state.Topology.Nodes.Count;

            for (int i = index; i < state.Failures.Length; i++)
            {
                if (state.Failures[i] == 0)
                    return (i, state.Nodes[i]);
            }

            for (int i = 0; i < index; i++)
            {
                if (state.Failures[i] == 0)
                    return (i, state.Nodes[i]);
            }
            
            return UnlikelyEveryoneFaultedChoice(state);
        }

        public void RestoreNodeIndex(int nodeIndex)
        {
            var state = _state;
            if (state.CurrentNodeIndex < nodeIndex)
                return; // nothing to do

            var stateFailure = state.Failures[nodeIndex];
            Interlocked.Add(ref state.Failures[nodeIndex], -stateFailure);// zero it
        }

        protected static void ThrowEmptyTopology()
        {
            throw new InvalidOperationException("Empty database topology, this shouldn't happen.");
        }
    }
}