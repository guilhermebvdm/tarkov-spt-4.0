"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const config_json_1 = __importDefault(require("../../config/config.json"));
const constants_1 = require("./constants");
const utils_1 = require("./utils");
const ILocationBase_1 = require("@spt/models/eft/common/ILocationBase");
const spawnZoneUtils_1 = __importDefault(require("./spawnZoneUtils"));
const GlobalValues_1 = require("../GlobalValues");
function buildScavMarksmanWaves(config, locationList, botConfig) {
    let { maxBotCap, scavWaveQuantity, scavWaveDistribution, sniperMaxGroupSize, maxBotPerZone, scavMaxGroupSize, scavDifficulty, sniperGroupChance, scavGroupChance, } = config;
    for (let index = 0; index < locationList.length; index++) {
        const mapSettingsList = Object.keys(config_json_1.default.mapConfigs);
        const map = mapSettingsList[index];
        locationList[index].base.waves = [];
        locationList[index].base = {
            ...locationList[index].base,
            ...{
                NewSpawn: false,
                OcculsionCullingEnabled: true,
                OfflineNewSpawn: false,
                OfflineOldSpawn: true,
                OldSpawn: true,
                BotSpawnCountStep: 0,
            },
        };
        locationList[index].base.NonWaveGroupScenario.Enabled = false;
        locationList[index].base["BotStartPlayer"] = 0;
        if (locationList[index].base.BotStop <
            locationList[index].base.EscapeTimeLimit * 60) {
            locationList[index].base.BotStop =
                locationList[index].base.EscapeTimeLimit * 60;
        }
        const { maxBotPerZoneOverride, maxBotCapOverride, EscapeTimeLimit, scavHotZones = [], sniperQuantity = 1, scavWaveCount, initialSpawnDelay, } = config_json_1.default.mapConfigs?.[map] || {};
        // Set per map EscapeTimeLimit
        if (EscapeTimeLimit) {
            locationList[index].base.EscapeTimeLimit = EscapeTimeLimit;
            locationList[index].base.exit_access_time = EscapeTimeLimit + 1;
        }
        // Set default or per map maxBotCap
        if (maxBotCapOverride || maxBotCap) {
            const capToSet = maxBotCapOverride || maxBotCap;
            // console.log(map, capToSet, maxBotCapOverride, maxBotCap);
            locationList[index].base.BotMax = capToSet;
            locationList[index].base.BotMaxPvE = capToSet;
            locationList[index].base.BotMaxPlayer = capToSet;
            botConfig.maxBotCap[constants_1.originalMapList[index]] = capToSet;
        }
        // Adjust botZone quantity
        if (maxBotPerZoneOverride || maxBotPerZone) {
            const BotPerZone = maxBotPerZoneOverride || maxBotPerZone;
            // console.log(map, BotPerZone, maxBotPerZoneOverride, maxBotPerZone);
            locationList[index].base.MaxBotPerZone = BotPerZone;
        }
        // const sniperLocations = new Set(
        //   [...locationList[index].base.SpawnPointParams]
        //     .filter(
        //       ({ Categories, DelayToCanSpawnSec, BotZoneName, Sides }) =>
        //         !Categories.includes("Boss") &&
        //         Sides[0] === "Savage" &&
        //         (BotZoneName?.toLowerCase().includes("snipe") ||
        //           DelayToCanSpawnSec > 40)
        //     )
        //     .map(({ BotZoneName }) => BotZoneName || "")
        // );
        const { Position: { x, y, z }, } = GlobalValues_1.globalValues.playerSpawn;
        const sniperSpawns = (0, spawnZoneUtils_1.default)(locationList[index].base.SpawnPointParams.filter((point) => point["type"] === "sniper"), x, y, z);
        let sniperLocations = sniperSpawns.map(({ BotZoneName }) => BotZoneName);
        // console.log(sniperLocations);
        const sniperDelay = 25;
        // Make sure that the sniper spawns permit snipers to actually spawn early.
        const sniperIds = new Set(sniperSpawns.map(({ Id }) => Id));
        locationList[index].base.SpawnPointParams.forEach((point, snipeIndex) => {
            if (sniperIds.has(point.Id)) {
                locationList[index].base.SpawnPointParams[snipeIndex].DelayToCanSpawnSec = 20;
            }
        });
        if (sniperLocations.length) {
            locationList[index].base.MinMaxBots = [
                {
                    WildSpawnType: "marksman",
                    max: sniperLocations.length * 5,
                    min: sniperLocations.length,
                },
            ];
        }
        let scavZones = (0, spawnZoneUtils_1.default)(locationList[index].base.SpawnPointParams.filter((point) => point["type"] === "scav"), x, y, z, 0.05).map(({ BotZoneName }) => BotZoneName);
        (0, utils_1.looselyShuffle)(scavZones, 3);
        const escapeTimeLimitRatio = Math.round(locationList[index].base.EscapeTimeLimit / constants_1.defaultEscapeTimes[map]);
        // Scavs
        let scavTotalWaveCount = Math.round(scavWaveCount * scavWaveQuantity * escapeTimeLimitRatio);
        if (scavHotZones.length && scavTotalWaveCount > 0) {
            scavTotalWaveCount = scavTotalWaveCount + scavHotZones.length;
        }
        while (scavTotalWaveCount - scavZones.length > 0) {
            console.log(`${map} ran out of appropriate zones for scavs, duplicating zones`);
            // const addEmpty = new Array(numberOfZoneless).fill("");
            scavZones = [...scavZones, ...scavZones];
            if (scavZones.length === 0) {
                scavZones = [""];
            }
        }
        config.debug &&
            escapeTimeLimitRatio !== 1 &&
            console.log(`${map} Scav wave count changed from ${scavWaveCount} to ${scavTotalWaveCount} due to escapeTimeLimit adjustment`);
        const timeLimit = locationList[index].base.EscapeTimeLimit * 60;
        // if (config.randomSpawns)
        //   sniperLocations = shuffle<string[]>(sniperLocations);
        // console.log(map);
        const snipers = (0, utils_1.buildBotWaves)(Math.min(sniperQuantity, sniperLocations.length), timeLimit, ///30,
        sniperMaxGroupSize, sniperGroupChance, sniperLocations, 0.8, ILocationBase_1.WildSpawnType.MARKSMAN, true, 0.3, sniperDelay);
        if (config.randomSpawns)
            scavZones = (0, utils_1.shuffle)(scavZones);
        const scavWaves = (0, utils_1.buildBotWaves)(scavTotalWaveCount, timeLimit, scavMaxGroupSize, scavGroupChance, scavZones, scavDifficulty, ILocationBase_1.WildSpawnType.ASSAULT, false, scavWaveDistribution, initialSpawnDelay + Math.round(10 * Math.random()));
        // Add hotzones if exist
        if (scavWaves.length) {
            scavHotZones.forEach((hotzone) => {
                const index = Math.floor(scavWaves.length * Math.random());
                scavWaves[index].BossZone = hotzone;
                // console.log(scavWaves[index].BossZone);
            });
        }
        // if (map === "shoreline") console.log(scavWaves.map(({ Time }) => Time));
        // console.log(snipers, scavWaves)
        locationList[index].base.BossLocationSpawn = [
            ...snipers,
            ...scavWaves,
            ...locationList[index].base.BossLocationSpawn,
        ];
    }
}
exports.default = buildScavMarksmanWaves;
