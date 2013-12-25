﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;
using Newsfeed.Services;

namespace Newsfeed.Managers
{
    /// <summary>
    /// Manages all clients connected to the service
    /// </summary>
    public class ClientsManager
    {
        #region Construction
        private ClientsManager()
        {
            this.clients = new Dictionary<string, INewsfeedServiceCallback>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the singleton instance of the manager.
        /// </summary>
        /// <value>The instance.</value>
        public static ClientsManager Instance
        { 
            get            
            {
                lock (ClientsManager.managerLock)
                {
                    if (ClientsManager.instance == null)
                    {
                        ClientsManager.instance = new ClientsManager();
                    }
                    return ClientsManager.instance;
                }
            }
        }

        /// <summary>
        /// Gets all clients connected to the service.
        /// </summary>
        /// <value>The clients.</value>
        public Dictionary<string, INewsfeedServiceCallback> Clients
        {
            get { return clients; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets the current callback channel and adds it to the collection of all channels.
        /// </summary>
        /// <returns></returns>
        public INewsfeedServiceCallback RegisterClient()
        {
            var sessionId = OperationContext.Current.SessionId;
            INewsfeedServiceCallback client;

            if (!this.clients.TryGetValue(sessionId, out client))
            {
                client = OperationContext.Current.GetCallbackChannel<INewsfeedServiceCallback>();
            }            

            ((IChannel)client).Closed += Connection_Closed;

            this.clients.Add(sessionId, client);

            return client;
        }
        #endregion

        #region Private methods
        private void Connection_Closed(object sender, EventArgs e)
        {
            this.clients.Remove(OperationContext.Current.SessionId);
        }
        #endregion

        #region Private fields and constants
        private static volatile ClientsManager instance;
        private readonly Dictionary<string, INewsfeedServiceCallback> clients;
        private static readonly object managerLock = new object();
        #endregion
    }
}