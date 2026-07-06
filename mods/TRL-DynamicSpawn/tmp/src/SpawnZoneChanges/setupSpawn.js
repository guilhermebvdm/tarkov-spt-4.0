"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.setupSpawns = void 0;
const constants_1 = require("../Spawning/constants");
const GlobalValues_1 = require("../GlobalValues");
const spawnZoneUtils_1 = require("../Spawning/spawnZoneUtils");
const utils_1 = require("../Spawning/utils");
const _1 = require(".");
const updateUtils_1 = require("./updateUtils");
const setupSpawns = (container) => {
    const databaseServer = container.resolve("DatabaseServer");
    const { locations } = databaseServer.getTables();
    const indexedMapSpawns = {};
    const mapsToExcludeFromPlayerCulling = new Set([
        "factory4_day",
        "factory4_night",
        "laboratory",
    ]);
    constants_1.originalMapList.forEach((map, mapIndex) => {
        const configMap = constants_1.configLocations[mapIndex];
        const allZones = [
            ...new Set(locations[map].base.SpawnPointParams.filter(({ BotZoneName }) => !!BotZoneName).map(({ BotZoneName }) => BotZoneName)),
        ];
        locations[map].base.OpenZones = allZones.join(",");
        let bossSpawns = [];
        let scavSpawns = [];
        let sniperSpawns = [];
        let pmcSpawns = [];
        const bossZoneList = new Set([
            "Zone_Blockpost",
            "Zone_RoofRocks",
            "Zone_RoofContainers",
            "Zone_RoofBeach",
            "Zone_TreatmentRocks",
            "Zone_TreatmentBeach",
            "Zone_Hellicopter",
            "Zone_Island",
            "BotZoneGate1",
            "BotZoneGate2",
            "BotZoneBasement",
        ]);
        const isGZ = map.includes("sandbox");
        (0, utils_1.shuffle)(locations[map].base.SpawnPointParams).forEach((point) => {
            switch (true) {
                case point.Categories.includes("Boss") ||
                    bossZoneList.has(point.BotZoneName):
                    bossSpawns.push(point);
                    break;
                case point.BotZoneName?.toLowerCase().includes("snipe") ||
                    (map !== "lighthouse" && point.DelayToCanSpawnSec > 40):
                    sniperSpawns.push(point);
                    break;
                case !!point.Infiltration || point.Categories.includes("Coop"):
                    pmcSpawns.push(point);
                    break;
                default:
                    scavSpawns.push(point);
                    break;
            }
        });
        // fix GZ
        if (isGZ) {
            sniperSpawns.map((point, index) => {
                if (index < 2) {
                    point.BotZoneName = Math.random()
                        ? "ZoneSandSnipeCenter"
                        : "ZoneSandSnipeCenter2";
                }
                else {
                    point.BotZoneName = ["ZoneSandSnipeCenter", "ZoneSandSnipeCenter2"][index];
                }
                return point;
            });
        }
        if (_config.ActivateSpawnCullingOnServerStart) {
            _1.ScavSpawns[map] =
                (0, spawnZoneUtils_1.removeClosestSpawnsFromCustomBots)(_1.ScavSpawns, scavSpawns, map, constants_1.configLocations[mapIndex]) || [];
            _1.PmcSpawns[map] =
                (0, spawnZoneUtils_1.removeClosestSpawnsFromCustomBots)(_1.PmcSpawns, pmcSpawns, map, constants_1.configLocations[mapIndex]) || [];
            _1.PlayerSpawns[map] =
                (0, spawnZoneUtils_1.removeClosestSpawnsFromCustomBots)(_1.PlayerSpawns, pmcSpawns, map, constants_1.configLocations[mapIndex]) || [];
            _1.SniperSpawns[map] =
                (0, spawnZoneUtils_1.removeClosestSpawnsFromCustomBots)(_1.SniperSpawns, sniperSpawns, map, constants_1.configLocations[mapIndex]) || [];
        }
        const { spawnMinDistance: limit } = _config.mapConfigs[constants_1.configLocations[mapIndex]];
        let playerSpawns = (0, spawnZoneUtils_1.BuildCustomPlayerSpawnPoints)(map, locations[map].base.SpawnPointParams);
        playerSpawns = (0, spawnZoneUtils_1.cleanClosest)(playerSpawns, mapIndex, _config.mapConfigs[configMap].mapCullingNearPointValuePlayer);
        scavSpawns = (0, spawnZoneUtils_1.cleanClosest)((0, spawnZoneUtils_1.AddCustomBotSpawnPoints)(scavSpawns, map), mapIndex, _config.mapConfigs[configMap].mapCullingNearPointValueScav).map((point, botIndex) => {
            if (point.ColliderParams?._props?.Radius < limit) {
                point.ColliderParams._props.Radius = limit;
            }
            return !!point.Categories.length
                ? {
                    ...point,
                    BotZoneName: isGZ ? "ZoneSandbox" : point?.BotZoneName,
                    Categories: ["Bot"],
                    Sides: ["Savage"],
                    CorePointId: 1,
                }
                : point;
        });
        pmcSpawns = (0, spawnZoneUtils_1.cleanClosest)((0, spawnZoneUtils_1.AddCustomPmcSpawnPoints)(pmcSpawns, map), mapIndex, _config.mapConfigs[configMap].mapCullingNearPointValuePmc).map((point, pmcIndex) => {
            if (point.ColliderParams?._props?.Radius < limit) {
                point.ColliderParams._props.Radius = limit;
            }
            return !!point.Categories.length
                ? {
                    ...point,
                    BotZoneName: isGZ
                        ? "ZoneSandbox"
                        : (0, spawnZoneUtils_1.getClosestZone)(scavSpawns, point.Position.x, point.Position.y, point.Position.z),
                    Categories: ["Coop", Math.random() ? "Group" : "Opposite"],
                    Sides: ["Pmc"],
                    CorePointId: 0,
                }
                : point;
        });
        sniperSpawns = (0, spawnZoneUtils_1.AddCustomSniperSpawnPoints)(sniperSpawns, map);
        indexedMapSpawns[mapIndex] = [
            ...sniperSpawns.map((point) => ({ ...point, type: "sniper" })),
            ...bossSpawns.map((point) => ({ ...point, type: "boss" })),
            ...scavSpawns.map((point) => ({ ...point, type: "scav" })),
            ...pmcSpawns.map((point) => ({ ...point, type: "pmc" })),
            ...playerSpawns.map((point) => ({ ...point, type: "player" })),
        ];
        showMapCullingDebug &&
            console.log("sniperSpawns", sniperSpawns.length, "bossSpawns", bossSpawns.length, "scavSpawns", scavSpawns.length, "pmcSpawns", pmcSpawns.length, "playerSpawns", playerSpawns.length, map);
        locations[map].base.SpawnPointParams = indexedMapSpawns[mapIndex];
        const listToAddToOpenZones = [
            ...new Set(locations[map].base.SpawnPointParams.map(({ BotZoneName }) => BotZoneName).filter((zone) => !!zone)),
        ];
        locations[map].base.OpenZones = listToAddToOpenZones.join(",");
    });
    //  PlayerSpawns, PmcSpawns, ScavSpawns, SniperSpawns
    if (_config.ActivateSpawnCullingOnServerStart) {
        (0, updateUtils_1.updateAllBotSpawns)(_1.PlayerSpawns, "playerSpawns");
        (0, updateUtils_1.updateAllBotSpawns)(_1.PmcSpawns, "pmcSpawns");
        (0, updateUtils_1.updateAllBotSpawns)(_1.ScavSpawns, "scavSpawns");
        (0, updateUtils_1.updateAllBotSpawns)(_1.SniperSpawns, "sniperSpawns");
    }
    GlobalValues_1.globalValues.indexedMapSpawns = indexedMapSpawns;
};
exports.setupSpawns = setupSpawns;
