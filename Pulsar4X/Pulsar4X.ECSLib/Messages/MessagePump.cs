﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Pulsar4X.ECSLib.DataSubscription;

namespace Pulsar4X.ECSLib
{
    public class MessagePumpServer
    {
        private readonly Game _game;
        private readonly ConcurrentQueue<string> _incommingMessages = new ConcurrentQueue<string>();
        private readonly ConcurrentDictionary<Guid, ConcurrentQueue<string>> _outGoingQueus = new ConcurrentDictionary<Guid, ConcurrentQueue<string>>();
        internal readonly Dictionary<Guid, DataSubsciber> DataSubscibers = new Dictionary<Guid, DataSubsciber>();
        public MessagePumpServer(Game game)
        {
            _game = game;
            //local UIconnection is an empty guid.
            AddNewConnection(Guid.Empty);
        }

        public void AddNewConnection(Guid connectionID)
        {
            _outGoingQueus.TryAdd(connectionID, new ConcurrentQueue<string>());
            DataSubscibers.Add(connectionID, new DataSubsciber(_game, Guid.Empty));
        }

        public void EnqueueMessage(string message)
        {
            _incommingMessages.Enqueue(message);
        }

        public void EnqueueOutgoingMessage(Guid toConnection, BaseToClientMessage message)
        {
            if(_outGoingQueus.ContainsKey(toConnection))
                _outGoingQueus[toConnection].Enqueue(ObjectSerializer.SerializeObject(message));
            else
            {
                //TODO: log NoConnection message.
            }
        }

        public bool TryPeekOutgoingMessage(Guid connction, out string message)
        {
            return _outGoingQueus[connction].TryPeek(out message);
        }

        public bool TryDequeueOutgoingMessage(Guid connection, out string message)
        {
            return _outGoingQueus[connection].TryDequeue(out message);
        }

        
        internal void NotifyConnectionsOfDataChanges<T>(Guid entityGuid, UIData data ) where T : UIData
        {
            foreach (var item in DataSubscibers.Values)
            {
                item.TriggerIfSubscribed<T>(entityGuid, data);
            }
        }

        internal bool AreAnySubscribers<T>(Guid entityGuid) where T : UIData
        {
            foreach (var item in DataSubscibers.Values)
            {
                if (item.IsSubscribedTo<T>(entityGuid))
                    return true;
            }
            return false;
        }

        public void ReadIncommingMessages(Game game)
        {
            string messageStr;
            while(_incommingMessages.TryDequeue(out messageStr))
            {
                BaseToServerMessage messageObj = ObjectSerializer.DeserializeObject<BaseToServerMessage>(messageStr);
                messageObj.HandleMessage(game);
            }
        }
    }


    public abstract class BaseToServerMessage
    {
        [JsonProperty]
        public Guid FactionGuid { get; set; }
        [JsonProperty]
        public Guid ConnectionID { get; set; }
        
        internal abstract void HandleMessage(Game game);
    }

    public abstract class BaseToClientMessage
    {
    }

    public class UIInfoMessage : BaseToClientMessage
    {   
        [JsonProperty]
        public string Message;     
        public UIInfoMessage(string message) { Message = message; }   
    }



    public class GameOrder : BaseToServerMessage
    {
        [JsonProperty]
        public IncomingMessageType MessageType { get; set; }
        [JsonProperty]
        private string Message { get; }
        
        public GameOrder(IncomingMessageType messageType) { MessageType = messageType; }
        
        internal override void HandleMessage(Game game)
        {
            switch (MessageType)
            {
                case IncomingMessageType.Exit:
                    game.ExitRequested = true;
                    break;                    
                case IncomingMessageType.ExecutePulse:
                    // TODO: Pulse length parsing
                    game.GameLoop.TimeStep();
                    break;
                case IncomingMessageType.StartRealTime:
                    float timeMultiplier;
                    if (float.TryParse(Message, out timeMultiplier))
                    {
                        game.GameLoop.TimeMultiplier = timeMultiplier;
                    }
                    game.GameLoop.StartTime();
                    break;
                case IncomingMessageType.StopRealTime:
                    game.GameLoop.PauseTime();
                    break;
                case IncomingMessageType.Echo:
                    UIInfoMessage newMessage = new UIInfoMessage("Echo from " + ConnectionID);
                    game.MessagePump.EnqueueOutgoingMessage(ConnectionID, newMessage);
                    break;

                // This message may be getting too complex for this handler.
                /*
                case IncomingMessageType.GalaxyQuery:
                    var systemGuids = new StringBuilder();
                    foreach (StarSystem starSystem in game.GetSystems(authToken))
                    {
                        systemGuids.Append($"{starSystem.Guid:N},");
                    }

                    game.MessagePump.EnqueueOutgoingMessage(OutgoingMessageType.GalaxyResponse, systemGuids.ToString(0, systemGuids.Length - 1));
                    break;
                    */
            }
        }
    }



}