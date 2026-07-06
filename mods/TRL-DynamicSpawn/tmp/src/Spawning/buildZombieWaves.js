"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const config_json_1 = __importDefault(require("../../config/config.json"));
const constants_1 = require("./constants");
const utils_1 = require("./utils");
function buildZombieWaves(config, locationList, bots) {
    let { debug, zombieWaveDistribution, zombieWaveQuantity, zombieHealth } = config;
    const zombieBodyParts = (0, utils_1.getHealthBodyPartsByPercentage)(zombieHealth);
    utils_1.zombieTypes.forEach((type) => {
        bots.types?.[type]?.health?.BodyParts?.forEach((_, index) => {
            bots.types[type].health.BodyParts[index] = zombieBodyParts;
        });
    });
    for (let indx = 0; indx < locationList.length; indx++) {
        const location = locationList[indx].base;
        const mapSettingsList = Object.keys(config_json_1.default.mapConfigs);
        const map = mapSettingsList[indx];
        const { zombieWaveCount } = config_json_1.default.mapConfigs?.[constants_1.configLocations[indx]];
        // if (location.Events?.Halloween2024?.MaxCrowdAttackSpawnLimit)
        //   location.Events.Halloween2024.MaxCrowdAttackSpawnLimit = 100;
        // if (location.Events?.Halloween2024?.CrowdCooldownPerPlayerSec)
        //   location.Events.Halloween2024.CrowdCooldownPerPlayerSec = 60;
        // if (location.Events?.Halloween2024?.CrowdCooldownPerPlayerSec)
        //   location.Events.Halloween2024.CrowdsLimit = 10;
        // if (location.Events?.Halloween2024?.CrowdAttackSpawnParams)
        //   location.Events.Halloween2024.CrowdAttackSpawnParams = [];
        if (!zombieWaveCount)
            return;
        const escapeTimeLimitRatio = Math.round(locationList[indx].base.EscapeTimeLimit / constants_1.defaultEscapeTimes[map]);
        const zombieTotalWaveCount = Math.round(zombieWaveCount * zombieWaveQuantity * escapeTimeLimitRatio);
        config.debug &&
            escapeTimeLimitRatio !== 1 &&
            console.log(`${map} Zombie wave count changed from ${zombieWaveCount} to ${zombieTotalWaveCount} due to escapeTimeLimit adjustment`);
        const zombieWaves = (0, utils_1.buildZombie)(zombieTotalWaveCount, location.EscapeTimeLimit * 60, zombieWaveDistribution, 9999);
        debug &&
            console.log(constants_1.configLocations[indx], " generated ", zombieWaves.length, "Zombies");
        location.BossLocationSpawn.push(...zombieWaves);
        // console.log(zombieWaves[0], zombieWaves[7]);
    }
}
exports.default = buildZombieWaves;
