﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using BepInEx;
using System;
using System.Collections.Generic;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// Registry for message decoders and receive actions.
        /// </summary>
        static readonly Dictionary<string, MessageBase> messageRegistry = new();

        /// <summary>
        /// Holds live update messages that arrive during the initial full sync, to be
        /// replayed right after the client entered the game world.
        /// </summary>
        static readonly Queue<MessageBase> deferredMessages = new();

        void InitMessageDispatcher()
        {
            AddMessageRegistry<MessageLogin>(ReceiveMessageLogin);
            AddMessageRegistry<MessageLoginResponse>(ReceiveMessageLoginResponse);

            AddMessageRegistry<MessageSyncAllAltitude>(ReceiveMessageSyncAllAltitude);
            AddMessageRegistry<MessageSyncAllContentData>(ReceiveMessageSyncAllContentData);
            AddMessageRegistry<MessageSyncAllContentId>(ReceiveMessageSyncAllContentId);
            AddMessageRegistry<MessageSyncAllFlags>(ReceiveMessageSyncAllFlags);
            AddMessageRegistry<MessageSyncAllGroundData>(ReceiveMessageSyncAllGroundData);
            AddMessageRegistry<MessageSyncAllGroundId>(ReceiveMessageSyncAllGroundId);
            AddMessageRegistry<MessageSyncAllWater>(ReceiveMessageSyncAllWater);

            AddMessageRegistry<MessageSyncAllMain>(ReceiveMessageSyncAllMain);
            AddMessageRegistry<MessageSyncAllGame>(ReceiveMessageSyncAllGame);
            AddMessageRegistry<MessageSyncAllPlanet>(ReceiveMessageSyncAllPlanet);
            AddMessageRegistry<MessageSyncAllItems>(ReceiveMessageSyncAllItems);
            AddMessageRegistry<MessageSyncAllWaterInfo>(ReceiveMessageSyncAllWaterInfo);
            AddMessageRegistry<MessageSyncAllDrones>(ReceiveMessageSyncAllDrones);
            AddMessageRegistry<MessageSyncAllWays>(ReceiveMessageSyncAllWays);
            AddMessageRegistry<MessageSyncAllCamera>(ReceiveMessageSyncAllCamera);

            AddMessageRegistry<MessageActionBuild>(ReceiveMessageActionBuild);
            AddMessageRegistry<MessageActionDestroy>(ReceiveMessageActionDestroy);
            AddMessageRegistry<MessageActionRenameLandmark>(ReceiveMessageActionRenameLandmark);
            AddMessageRegistry<MessageActionReverseLine>(ReceiveMessageActionReverseLine);
            AddMessageRegistry<MessageActionCopy>(ReceiveMessageActionCopy);
            AddMessageRegistry<MessageUpdateDepotDrones>(ReceiveMessageUpdateDepotDrones);

            AddMessageRegistry<MessageUpdateStackAt>(ReceiveMessageUpdateStackAt);
            AddMessageRegistry<MessageUpdateStacksAt>(ReceiveMessageUpdateStacksAt);
            AddMessageRegistry<MessageUpdateRecipeAt>(ReceiveMessageUpdateRecipeAt);
            AddMessageRegistry<MessageUpdateTransportedAt>(ReceiveMessageUpdateTransportedAt);

            AddMessageRegistry<MessageUpdateContentData>(ReceiveMessageUpdateContentData);
            AddMessageRegistry<MessageUpdateDatasAt>(ReceiveMessageUpdateDatasAt);
            AddMessageRegistry<MessageUpdatePlanetGasses>(ReceiveMessageUpdatePlanetGasses);
            AddMessageRegistry<MessageUpdateForest>(ReceiveMessageUpdateForest);
            AddMessageRegistry<MessageUpdateCity>(ReceiveMessageUpdateCity);

            AddMessageRegistry<MessageUpdateTime>(ReceiveMessageUpdateTime);
            AddMessageRegistry<MessageUpdatePlanet>(ReceiveMessageUpdatePlanet);
            AddMessageRegistry<MessageUpdateLine>(ReceiveMessageUpdateLine);
            AddMessageRegistry<MessageUpdateLines>(ReceiveMessageUpdateLines);
            AddMessageRegistry<MessageUpdateDrones>(ReceiveMessageUpdateDrones);
            AddMessageRegistry<MessageUpdateTransportStacks>(ReceiveMessageUpdateTransportStacks);
            AddMessageRegistry<MessageUpdateItems>(ReceiveMessageUpdateItems);

            AddMessageRegistry<MessageActionBeginLine>(ReceiveMessageActionBeginLine);
            AddMessageRegistry<MessageUpdateStartLine>(ReceiveMessageUpdateStartLine);
            AddMessageRegistry<MessageActionFinishLine>(ReceiveMessageActionFinishLine);
            AddMessageRegistry<MessageUpdateFinishLine>(ReceiveMessageUpdateFinishLine);
            AddMessageRegistry<MessageActionRemoveLine>(ReceiveMessageActionRemoveLine);
            AddMessageRegistry<MessageActionChangeVehicleCount>(ReceiveMessageActionChangeVehicleCount);
        }

        static void AddMessageRegistry<T>(Action<T> handler) where T : MessageBase, new()
        {
            var m = new T();

            if (!m.GetType().Name.EndsWith(m.MessageCode()))
            {
                throw new InvalidOperationException("MessageCode (by convention) mismatch: " + m.GetType().Name + " vs *" + m.MessageCode());
            }

            m.onReceive = m => handler((T)m);
            messageRegistry.Add(m.MessageCode(), m);
        }

        static void DispatchMessageLoop()
        {
            // process messages that were deferred during the ClientJoin phase
            if (multiplayerMode == MultiplayerMode.Client)
            {
                while (deferredMessages.Count != 0)
                {
                    var m = deferredMessages.Dequeue();
                    m.onReceive(m);
                }
            }
            while (multiplayerMode == MultiplayerMode.Host 
                || multiplayerMode == MultiplayerMode.Client
                || multiplayerMode == MultiplayerMode.ClientJoin) 
            {
                try
                {
                    if (receiverQueue.TryDequeue(out var receiver))
                    {
                        receiver.onReceive(receiver);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
        }
    }
}
