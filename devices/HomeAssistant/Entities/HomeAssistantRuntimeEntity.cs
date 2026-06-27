// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Delegate for state change events on a runtime entity.
    /// </summary>
    /// <param name="sender">The entity that changed.</param>
    /// <param name="oldState">Previous state value.</param>
    /// <param name="newState">New state value.</param>
    public delegate void HomeAssistantStateChangeDelegate(object sender, string oldState, string newState);

    /// <summary>
    /// Base class for runtime Home Assistant entities that manage state and publishing.
    /// </summary>
    public abstract class HomeAssistantRuntimeEntity
    {
        /// <summary>
        /// Fired when the entity state changes.
        /// </summary>
        public event HomeAssistantStateChangeDelegate OnStateChange;

        /// <summary>
        /// Gets the current state.
        /// </summary>
        public string State
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets the underlying discovery entity configuration.
        /// </summary>
        public HomeAssistantDiscoveryEntity Discovery
        {
            get { return _discovery; }
        }

        /// <summary>
        /// Gets the command topic this entity listens on.
        /// </summary>
        public string CommandTopic
        {
            get { return _discovery?.CommandTopic; }
        }

        /// <summary>
        /// Gets the state topic this entity publishes to.
        /// </summary>
        public string StateTopic
        {
            get { return _discovery?.StateTopic; }
        }

        /// <summary>
        /// Gets the unique identifier for this entity.
        /// </summary>
        public string UniqueId
        {
            get { return _discovery?.UniqueId; }
        }

        /// <summary>
        /// Sets the entity state from an external command.
        /// Triggers OnStateChange event if state actually changed.
        /// </summary>
        /// <param name="newState">New state value.</param>
        public virtual void SetState(string newState)
        {
            ApplyState(newState, true, false);
        }

        /// <summary>
        /// Publishes the current local state to MQTT without raising OnStateChange.
        /// Use this for application-originated state updates to avoid command feedback loops.
        /// </summary>
        /// <param name="newState">State value to publish.</param>
        public void PublishState(string newState)
        {
            ApplyState(newState, false, true);
        }

        private void ApplyState(string newState, bool notifyListeners, bool publishToBroker)
        {
            if (newState == null)
            {
                newState = string.Empty;
            }

            bool changed = newState != _state;
            string oldState = _state;

            if (changed)
            {
                _state = newState;
            }

            if (publishToBroker && _publisher != null && _discovery != null && !string.IsNullOrEmpty(_discovery.StateTopic))
            {
                _publisher(_discovery.StateTopic, changed ? _state : newState, true);
            }

            if (notifyListeners && changed)
            {
                OnStateChange?.Invoke(this, oldState, _state);
            }
        }

        /// <summary>
        /// Initializes the runtime entity with discovery metadata and publishing callback.
        /// </summary>
        /// <param name="discovery">Discovery entity definition.</param>
        /// <param name="initialState">Initial state value.</param>
        /// <param name="publisher">Callback to publish MQTT messages.</param>
        protected void Initialize(
            HomeAssistantDiscoveryEntity discovery,
            string initialState,
            HomeAssistantPublishDelegate publisher)
        {
            _discovery = discovery;
            _state = initialState ?? string.Empty;
            _publisher = publisher;
        }

        private HomeAssistantDiscoveryEntity _discovery;
        private string _state;
        private HomeAssistantPublishDelegate _publisher;
    }
}
