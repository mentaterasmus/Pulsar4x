﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pulsar4X.ECSLib
{
    internal class EconProcessor
    {
        [JsonProperty]
        private DateTime _lastRun = DateTime.MinValue;
        private IndustrySubprocessor _industrySubprocessor;

        internal EconProcessor(Game game)
        {
            _industrySubprocessor = new IndustrySubprocessor(game);
        }


        internal void Process(Game game, List<StarSystem> systems, int deltaSeconds)
        {
            if (game.CurrentDateTime - _lastRun < game.Settings.EconomyCycleTime)
            {
                return;
            }

            _lastRun = game.CurrentDateTime;

            if (game.Settings.EnableMultiThreading ?? false)
            {
                Parallel.ForEach(systems, system => ProcessSystem(game, system));
            }
            else
            {
                foreach (var system in systems)
                {
                    ProcessSystem(game, system);
                }
            }
        }

        private void ProcessSystem(Game game, StarSystem system)
        {
            IndustrySubprocessor.Process(game, system);


            List<Entity> allPlanets = system.SystemManager.GetAllEntitiesWithDataBlob<SystemBodyDB>().Where(body =>
            {
                var bodyDBType = body.GetDataBlob<SystemBodyDB>().Type;
                return bodyDBType != BodyType.Moon && bodyDBType != BodyType.Asteroid && bodyDBType != BodyType.Comet;
            }).ToList();
            foreach (Entity colonyEntity in system.SystemManager.GetAllEntitiesWithDataBlob<RuinsDB>())
            {
                // Process Ruins
            }
            
            // Process Trade Goods
            //
            TechProcessor.ProcessSystem(system, game);
        }
    }
}