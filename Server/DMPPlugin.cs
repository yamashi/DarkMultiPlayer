using System;
using DarkMultiPlayerCommon;


namespace DarkMultiPlayerServer
{
    public interface IDMPPlugin
    {
        /// <summary>
        /// Fires every main thread tick (10ms).
        /// </summary>
        void OnUpdate();
        /// <summary>
        /// Fires just after the server is started or restarted.
        /// </summary>
        void OnServerStart();
        /// <summary>
        /// Fires just before the server stops or restarts.
        /// </summary>
        void OnServerStop();
        /// <summary>
        /// Fires when the client's connection is accepted.
        /// </summary>
        void OnClientConnect(ClientObject client);
        /// <summary>
        /// Fires just after the client has authenticated
        /// </summary>
        void OnClientAuthenticated(ClientObject client);
        /// <summary>
        /// Fires when a client disconnects
        /// </summary>
        void OnClientDisconnect(ClientObject client);
        /// <summary>
        /// Fires every time a message is received from a client
        /// </summary>
        /// <param name="aMessage">The message</param>
        bool OnMessageReceived(IMessage aMessage);
    }

    public abstract class DMPPlugin : IDMPPlugin
    {
        public virtual void OnUpdate() { }
        public virtual void OnServerStart() { }
        public virtual void OnServerStop() { }
        public virtual void OnClientConnect(ClientObject client) { }
        public virtual void OnClientAuthenticated(ClientObject client) { }
        public virtual void OnClientDisconnect(ClientObject client) { }
        public virtual bool OnMessageReceived(IMessage aMessage) { return false; }
    }
}
