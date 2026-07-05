"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.buildBossWaves = void 0;
const config_json_1 = __importDefault(require("../../config/config.json"));
const constants_1 = require("./constants");
const utils_1 = require("./utils");
const utils_2 = require("../utils");
function buildBossWaves(config, locationList) {
    let { randomRaiderGroup, randomRaiderGroupChance, randomRogueGroup, randomRogueGroupChance, mainBossChanceBuff, bossInvasion, bossInvasionSpawnChance, disableBosses, bossOpenZones, gradualBossInvasion, } = config;
    const bossList = constants_1.mainBossNameList.filter((bossName) => !["bossKnight"].includes(bossName));
    const allBosses = {};
    for (const key in locationList) {
        locationList[key].base.BossLocationSpawn.forEach((boss) => {
            if (!allBosses[boss.BossName]) {
                allBosses[boss.BossName] = boss;
            }
        });
    }
    // CreateBossList
    const bosses = {};
    for (let indx = 0; indx < locationList.length; indx++) {
        // Disable Bosses
        if (disableBosses && !!locationList[indx].base?.BossLocationSpawn) {
            locationList[indx].base.BossLocationSpawn = [];
        }
        else {
            //Remove all other spawns from pool now that we have the spawns zone list
            locationList[indx].base.BossLocationSpawn = locationList[indx].base.BossLocationSpawn.filter((boss) => !constants_1.bossesToRemoveFromPool.has(boss.BossName));
            // Performance changes
            if (config_json_1.default.EnableBossPerformanceImprovements) {
                locationList[indx].base.BossLocationSpawn.forEach((Boss, bIndex) => {
                    if (Boss.BossChance < 1)
                        return;
                    if (!!constants_1.bossPerformanceHash[Boss.BossName || ""]) {
                        const varsToUpdate = constants_1.bossPerformanceHash[Boss.BossName];
                        // make it so bossPartisan has a random spawn time
                        if (Boss.BossName === "bossPartisan") {
                            const max = locationList[indx].base.EscapeTimeLimit;
                            varsToUpdate.Time = Math.floor(Math.random() * 50 * max);
                            // console.log(varsToUpdate, max * 60)
                        }
                        locationList[indx].base.BossLocationSpawn[bIndex] = {
                            ...Boss,
                            ...varsToUpdate,
                        };
                    }
                });
            }
            const location = locationList[indx];
            const defaultBossSettings = config_json_1.default.mapConfigs?.[constants_1.configLocations[indx]]?.defaultBossSettings;
            // Sets bosses spawn chance from settings
            if (location?.base?.BossLocationSpawn &&
                defaultBossSettings &&
                Object.keys(defaultBossSettings)?.length) {
                const filteredBossList = Object.keys(defaultBossSettings).filter((name) => defaultBossSettings[name]?.BossChance !== undefined);
                if (filteredBossList?.length) {
                    filteredBossList.forEach((bossName) => {
                        location.base.BossLocationSpawn =
                            location.base.BossLocationSpawn.map((boss) => ({
                                ...boss,
                                ...(boss.BossName === bossName
                                    ? { BossChance: defaultBossSettings[bossName].BossChance }
                                    : {}),
                            }));
                    });
                }
            }
            if (randomRaiderGroup) {
                const raiderWave = (0, utils_1.buildBossBasedWave)(randomRaiderGroupChance, "1,2,2,2,3", "pmcBot", "pmcBot", "", locationList[indx].base.EscapeTimeLimit);
                location.base.BossLocationSpawn.push(raiderWave);
            }
            if (randomRogueGroup) {
                const rogueWave = (0, utils_1.buildBossBasedWave)(randomRogueGroupChance, "1,2,2,2,3", "exUsec", "exUsec", "", locationList[indx].base.EscapeTimeLimit);
                location.base.BossLocationSpawn.push(rogueWave);
            }
            //Add each boss from each map to bosses object
            const filteredBosses = location.base.BossLocationSpawn?.filter(({ BossName }) => constants_1.mainBossNameList.includes(BossName));
            if (filteredBosses.length) {
                for (let index = 0; index < filteredBosses.length; index++) {
                    const boss = filteredBosses[index];
                    if (!bosses[boss.BossName] ||
                        (bosses[boss.BossName] &&
                            bosses[boss.BossName].BossChance < boss.BossChance)) {
                        bosses[boss.BossName] = { ...boss };
                    }
                }
            }
        }
    }
    if (!disableBosses) {
        // Make boss Invasion
        if (bossInvasion) {
            if (bossInvasionSpawnChance) {
                bossList.forEach((bossName) => {
                    if (bosses[bossName])
                        bosses[bossName].BossChance = bossInvasionSpawnChance;
                });
            }
            for (let key = 0; key < locationList.length; key++) {
                //Gather bosses to avoid duplicating.
                const duplicateBosses = [
                    ...locationList[key].base.BossLocationSpawn.filter(({ BossName, BossZone }) => bossList.includes(BossName)).map(({ BossName }) => BossName),
                    "bossKnight", // So knight doesn't invade
                ];
                //Build bosses to add
                const bossesToAdd = (0, utils_1.shuffle)(Object.values(bosses))
                    .filter(({ BossName }) => !duplicateBosses.includes(BossName))
                    .map((boss, j) => ({
                    ...boss,
                    BossZone: "",
                    BossEscortAmount: boss.BossEscortAmount === "0" ? boss.BossEscortAmount : "1",
                    ...(gradualBossInvasion ? { Time: j * 20 + 1 } : {}),
                }));
                // UpdateBosses
                locationList[key].base.BossLocationSpawn = [
                    ...locationList[key].base.BossLocationSpawn,
                    ...bossesToAdd,
                ];
            }
        }
        let hasChangedBossSpawns = false;
        // console.log(Object.keys(allBosses));
        constants_1.configLocations.forEach((mapName, index) => {
            const bossLocationSpawn = locationList[index].base.BossLocationSpawn;
            const mapBossConfig = (0, utils_2.cloneDeep)(config_json_1.default.eliteConfig[mapName] || {});
            // if (Object.keys(mapBossConfig).length === 0) console.log(name, "empty");
            const adjusted = new Set([]);
            bossLocationSpawn.forEach(({ BossName, BossChance }, bossIndex) => {
                if (typeof mapBossConfig[BossName] === "number") {
                    if (BossChance !== mapBossConfig[BossName]) {
                        if (!hasChangedBossSpawns) {
                            console.log(`\n[MOAR]: --- Adjusting default boss spawn rates from _config.eliteConfig.json --- `);
                            hasChangedBossSpawns = true;
                        }
                        console.log(`[MOAR]: ${mapName} ${BossName}: ${locationList[index].base.BossLocationSpawn[bossIndex].BossChance} => ${mapBossConfig[BossName]}`);
                        locationList[index].base.BossLocationSpawn[bossIndex].BossChance =
                            mapBossConfig[BossName];
                    }
                    adjusted.add(BossName);
                }
            });
            const bossesToAdd = Object.keys(mapBossConfig)
                .filter((adjustName) => !adjusted.has(adjustName) && !!allBosses[adjustName])
                .map((bossName) => {
                `[MOAR]: Adding non-default boss ${bossName} to ${constants_1.originalMapList[index]}`;
                const newBoss = (0, utils_2.cloneDeep)(allBosses[bossName] || {});
                newBoss.BossChance = mapBossConfig[bossName];
                // console.log(
                //   "Adding boss",
                //   bossName,
                //   "to ",
                //   originalMapList[index],
                //   "spawn chance =>",
                //   mapBossConfig[bossName]
                // );
                return newBoss;
            });
            // console.log(bossesToAdd);
            if (bossOpenZones || mainBossChanceBuff) {
                locationList[index].base?.BossLocationSpawn?.forEach((boss, key) => {
                    if (bossList.includes(boss.BossName)) {
                        if (bossOpenZones) {
                            locationList[index].base.BossLocationSpawn[key] = {
                                ...locationList[index].base.BossLocationSpawn[key],
                                BossZone: "",
                            };
                        }
                        if (!!boss.BossChance && mainBossChanceBuff > 0) {
                            locationList[index].base.BossLocationSpawn[key] = {
                                ...locationList[index].base.BossLocationSpawn[key],
                                BossChance: boss.BossChance + mainBossChanceBuff > 100
                                    ? 100
                                    : Math.round(boss.BossChance + mainBossChanceBuff),
                            };
                        }
                    }
                });
            }
            locationList[index].base.BossLocationSpawn = [
                ...locationList[index].base.BossLocationSpawn,
                ...bossesToAdd,
            ];
            bossesToAdd.length &&
                console.log(`[MOAR] Adding the following bosses to map ${constants_1.configLocations[index]}: ${bossesToAdd.map(({ BossName }) => BossName)}`);
            // console.log(locationList[index].base.BossLocationSpawn.length);
            const bossesToSkip = new Set(["sectantPriest", "pmcBot"]);
            // Apply the percentages on all bosses, cull those that won't spawn, make all bosses 100 chance that remain.
            locationList[index].base.BossLocationSpawn = locationList[index].base.BossLocationSpawn.map(({ BossChance, BossName, TriggerId }, bossIndex) => {
                if (BossChance < 1) {
                    return locationList[index].base.BossLocationSpawn[bossIndex];
                }
                if (!TriggerId &&
                    !bossesToSkip.has(BossName) &&
                    BossChance < 100) {
                    if (BossChance / 100 < Math.random()) {
                        locationList[index].base.BossLocationSpawn[bossIndex].BossChance = 0;
                        locationList[index].base.BossLocationSpawn[bossIndex].ForceSpawn =
                            false;
                        locationList[index].base.BossLocationSpawn[bossIndex].IgnoreMaxBots = false;
                    }
                    else {
                        locationList[index].base.BossLocationSpawn[bossIndex].BossChance = 100;
                    }
                }
                return locationList[index].base.BossLocationSpawn[bossIndex];
            }).filter(({ BossChance, BossName, ...rest }) => {
                if (BossChance < 1) {
                    return false;
                }
                return true;
            });
            // if (mapName === "lighthouse") {
            //   console.log(
            //     locationList[index].base.BossLocationSpawn.map(
            //       ({ BossName, BossChance }) => ({ BossName, BossChance })
            //     )
            //   );
            // }
        });
        if (hasChangedBossSpawns) {
            console.log(`[MOAR]: --- Adjusting default boss spawn rates complete --- \n`);
        }
    }
}
exports.buildBossWaves = buildBossWaves;
