"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.removeClosestSpawnsFromCustomBots = exports.getClosestZone = exports.BuildCustomPlayerSpawnPoints = exports.random360 = exports.AddCustomSniperSpawnPoints = exports.AddCustomBotSpawnPoints = exports.AddCustomPmcSpawnPoints = exports.uuidv4 = exports.cleanClosest = exports.getDistance = void 0;
const config_json_1 = __importDefault(require("../../config/config.json"));
const constants_1 = require("./constants");
const SpawnZoneChanges_1 = require("../SpawnZoneChanges");
function sq(n) {
    return n * n;
}
function pt(a, b) {
    return Math.sqrt(sq(a) + sq(b));
}
const getDistance = (x, y, z, mX, mY, mZ) => {
    (x = Math.abs(x - mX)), (y = Math.abs(y - mY)), (z = Math.abs(z - mZ));
    return pt(pt(x, z), y);
};
exports.getDistance = getDistance;
function getSortedSpawnPointList(SpawnPointParams, mX, my, mZ, cull) {
    let culledAmount = 0;
    const sorted = SpawnPointParams.sort((a, b) => {
        const a1 = (0, exports.getDistance)(a.Position.x, a.Position.y, a.Position.z, mX, my, mZ);
        const b1 = (0, exports.getDistance)(b.Position.x, b.Position.y, b.Position.z, mX, my, mZ);
        return a1 - b1;
    }).filter((_, index) => {
        if (!cull)
            return true;
        const result = index > SpawnPointParams.length * cull;
        if (!result)
            culledAmount++;
        return result;
    });
    if (config_json_1.default.debug && culledAmount > 0) {
        console.log("Reduced to " +
            Math.round((sorted.length / SpawnPointParams.length) * 100) +
            "% of original spawns", SpawnPointParams.length, ">", sorted.length, "\n");
    }
    return sorted;
}
exports.default = getSortedSpawnPointList;
function cleanClosest(SpawnPointParams, mapIndex, mapCullingNearPointValue) {
    const map = constants_1.configLocations[mapIndex];
    const okayList = new Set();
    const filteredParams = SpawnPointParams.map((point) => {
        const { Position: { x: X, y: Y, z: Z }, } = point;
        const result = !SpawnPointParams.some(({ Position: { z, x, y }, Id }) => {
            const dist = (0, exports.getDistance)(X, Y, Z, x, y, z);
            return mapCullingNearPointValue > dist && dist !== 0 && !okayList.has(Id);
        });
        if (!result) {
            okayList.add(point.Id);
        }
        return result
            ? point
            : {
                ...point,
                DelayToCanSpawnSec: 9999999,
                CorePointId: 99999,
                Categories: [],
                Sides: [],
            };
    });
    if (config_json_1.default.debug) {
        const actualCulled = filteredParams.filter(({ Categories }) => !!Categories.length);
        console.log(map, filteredParams.length, ">", actualCulled.length, "Reduced to " +
            Math.round((actualCulled.length / filteredParams.length) * 100) +
            "% of original spawns"); // high, low}
    }
    return filteredParams.filter((point) => !!point.Categories.length);
    // if (!_config.debug) {
    //   const actualCulled = culled.filter(({ Categories }) => !!Categories.length);
    //   console.log(
    //     map,
    //     "Reduced to " +
    //     Math.round((actualCulled.length / culled.length) * 100) +
    //     "% of original spawns",
    //     culled.length,
    //     ">",
    //     actualCulled.length
    //     // "\n"
    //   ); // high, low}
    // }
}
exports.cleanClosest = cleanClosest;
function uuidv4() {
    return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, (c) => (+c ^
        (crypto.getRandomValues(new Uint8Array(1))[0] & (15 >> (+c / 4)))).toString(16));
}
exports.uuidv4 = uuidv4;
const AddCustomPmcSpawnPoints = (SpawnPointParams, map) => {
    if (!SpawnZoneChanges_1.PmcSpawns[map] || !SpawnZoneChanges_1.PmcSpawns[map].length) {
        config_json_1.default.debug && console.log("no custom Bot spawns for " + map);
        return SpawnPointParams;
    }
    const playerSpawns = SpawnZoneChanges_1.PmcSpawns[map].map((coords, index) => ({
        BotZoneName: (0, exports.getClosestZone)(SpawnPointParams, coords.x, coords.y, coords.z),
        Categories: ["Coop", Math.random() ? "Group" : "Opposite"],
        Sides: ["Pmc"],
        CorePointId: 0,
        ColliderParams: {
            _parent: "SpawnSphereParams",
            _props: {
                Center: {
                    x: 0,
                    y: 0,
                    z: 0,
                },
                Radius: 20,
            },
        },
        DelayToCanSpawnSec: 4,
        Id: uuidv4(),
        Infiltration: "",
        Position: coords,
        Rotation: (0, exports.random360)(),
    }));
    return [...SpawnPointParams, ...playerSpawns];
};
exports.AddCustomPmcSpawnPoints = AddCustomPmcSpawnPoints;
const AddCustomBotSpawnPoints = (SpawnPointParams, map) => {
    if (!SpawnZoneChanges_1.ScavSpawns[map] || !SpawnZoneChanges_1.ScavSpawns[map].length) {
        config_json_1.default.debug && console.log("no custom Bot spawns for " + map);
        return SpawnPointParams;
    }
    const scavSpawns = SpawnZoneChanges_1.ScavSpawns[map].map((coords) => ({
        BotZoneName: (0, exports.getClosestZone)(SpawnPointParams, coords.x, coords.y, coords.z),
        Categories: ["Bot"],
        ColliderParams: {
            _parent: "SpawnSphereParams",
            _props: {
                Center: {
                    x: 0,
                    y: 0,
                    z: 0,
                },
                Radius: 20,
            },
        },
        CorePointId: 1,
        DelayToCanSpawnSec: 4,
        Id: uuidv4(),
        Infiltration: "",
        Position: coords,
        Rotation: (0, exports.random360)(),
        Sides: ["Savage"],
    }));
    return [...SpawnPointParams, ...scavSpawns];
};
exports.AddCustomBotSpawnPoints = AddCustomBotSpawnPoints;
const AddCustomSniperSpawnPoints = (SpawnPointParams, map) => {
    if (!SpawnZoneChanges_1.SniperSpawns[map] || !SpawnZoneChanges_1.SniperSpawns[map].length) {
        config_json_1.default.debug && console.log("no custom Player spawns for " + map);
        return SpawnPointParams;
    }
    const sniperSpawns = SpawnZoneChanges_1.SniperSpawns[map].map((coords, index) => ({
        BotZoneName: (0, exports.getClosestZone)(SpawnPointParams, coords.x, coords.y, coords.z) ||
            "custom_snipe_" + index,
        Categories: ["Bot"],
        ColliderParams: {
            _parent: "SpawnSphereParams",
            _props: {
                Center: {
                    x: 0,
                    y: 0,
                    z: 0,
                },
                Radius: 20,
            },
        },
        CorePointId: 1,
        DelayToCanSpawnSec: 4,
        Id: uuidv4(),
        Infiltration: "",
        Position: coords,
        Rotation: (0, exports.random360)(),
        Sides: ["Savage"],
    }));
    return [...SpawnPointParams, ...sniperSpawns];
};
exports.AddCustomSniperSpawnPoints = AddCustomSniperSpawnPoints;
const random360 = () => Math.random() * 360;
exports.random360 = random360;
const BuildCustomPlayerSpawnPoints = (map, refSpawns) => {
    const playerOnlySpawns = refSpawns
        .filter((item) => !!item.Infiltration && item.Categories[0] === "Player")
        .map((point) => {
        point.ColliderParams._props.Radius = 1;
        point.Position.y = point.Position.y + 0.5;
        return {
            ...point,
            BotZoneName: "",
            isCustom: true,
            Id: uuidv4(),
            Sides: ["Pmc"],
        };
    });
    // console.log(map, playerOnlySpawns.length);
    if (!SpawnZoneChanges_1.PlayerSpawns[map] || !SpawnZoneChanges_1.PlayerSpawns[map].length) {
        config_json_1.default.debug && console.log("no custom Player spawns for " + map);
        return playerOnlySpawns;
    }
    const getClosestInfil = (X, Y, Z) => {
        let closest = Infinity;
        let selectedInfil = "";
        playerOnlySpawns.forEach(({ Infiltration, Position: { x, y, z } }) => {
            const dist = (0, exports.getDistance)(X, Y, Z, x, y, z);
            if (!!Infiltration && dist < closest) {
                closest = dist;
                selectedInfil = Infiltration;
            }
        });
        return selectedInfil;
    };
    const playerSpawns = SpawnZoneChanges_1.PlayerSpawns[map].map((coords, index) => ({
        BotZoneName: "",
        Categories: ["Player"],
        ColliderParams: {
            _parent: "SpawnSphereParams",
            _props: {
                Center: {
                    x: 0,
                    y: 0,
                    z: 0,
                },
                Radius: 1,
            },
        },
        isCustom: true,
        CorePointId: 0,
        DelayToCanSpawnSec: 4,
        Id: uuidv4(),
        Infiltration: getClosestInfil(coords.x, coords.y, coords.z),
        Position: coords,
        Rotation: (0, exports.random360)(),
        Sides: ["Pmc"],
    }));
    // TODO: Check infils
    // console.log(map);
    // console.log(playerOnlySpawns[0], playerSpawns[0]);
    return [...playerOnlySpawns, ...playerSpawns];
};
exports.BuildCustomPlayerSpawnPoints = BuildCustomPlayerSpawnPoints;
const getClosestZone = (params, x, y, z) => {
    if (Array.isArray(params) &&
        !params.filter(({ BotZoneName }) => BotZoneName).length)
        return "";
    return (getSortedSpawnPointList(params, x, y, z).find(({ BotZoneName }) => !!BotZoneName)?.BotZoneName || "");
};
exports.getClosestZone = getClosestZone;
const removeClosestSpawnsFromCustomBots = (CustomBots, SpawnPointParams, map, mapConfigMap) => {
    if (!CustomBots[map] || !CustomBots[map].length) {
        console.log(map, "Is empty");
        return;
    }
    const coords = CustomBots[map];
    const { mapCullingNearPointValuePlayer, mapCullingNearPointValuePmc, mapCullingNearPointValueScav } = config_json_1.default.mapConfigs[mapConfigMap];
    const mapCullingNearPointValue = (mapCullingNearPointValuePlayer +
        mapCullingNearPointValuePmc +
        mapCullingNearPointValueScav) / 3;
    let filteredCoords = coords.filter(({ x: X, y: Y, z: Z }) => !SpawnPointParams.some(({ Position: { z, x, y } }) => {
        return mapCullingNearPointValue > (0, exports.getDistance)(X, Y, Z, x, y, z);
    }));
    const okayList = new Set();
    filteredCoords = [...coords].filter(({ x: X, y: Y, z: Z }, index) => {
        const result = !coords.some(({ z, x, y }) => {
            const dist = (0, exports.getDistance)(X, Y, Z, x, y, z);
            return (mapCullingNearPointValue * 1.3 > dist &&
                dist !== 0 &&
                !okayList.has("" + x + y + z));
        });
        if (!result)
            okayList.add("" + X + Y + Z);
        return result;
    });
    console.log(map, coords.length, ">", filteredCoords.length, "culled", coords.length - filteredCoords.length, "spawns");
    return filteredCoords;
};
exports.removeClosestSpawnsFromCustomBots = removeClosestSpawnsFromCustomBots;
