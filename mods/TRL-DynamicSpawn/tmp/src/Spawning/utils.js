"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.looselyShuffle = exports.enforceSmoothing = exports.getRandomInArray = exports.setEscapeTimeOverrides = exports.getHealthBodyPartsByPercentage = exports.buildZombie = exports.buildBotWaves = exports.getRandomZombieType = exports.getRandomDifficulty = exports.zombieTypesCaps = exports.zombieTypes = exports.buildBossBasedWave = exports.shuffle = exports.getDifficulty = exports.waveBuilder = void 0;
const ILocationBase_1 = require("@spt/models/eft/common/ILocationBase");
const config_json_1 = __importDefault(require("../../config/config.json"));
const constants_1 = require("./constants");
const waveBuilder = (totalWaves, timeLimit, waveDistribution, wildSpawnType, difficulty, isPlayer, maxSlots, combinedZones = [], specialZones = [], offset, starting, moreGroups) => {
    if (totalWaves === 0)
        return [];
    const averageTime = timeLimit / totalWaves;
    const firstHalf = Math.round(averageTime * (1 - waveDistribution));
    const secondHalf = Math.round(averageTime * (1 + waveDistribution));
    let timeStart = offset || 0;
    const waves = [];
    let maxSlotsReached = Math.round(1.3 * totalWaves);
    while (totalWaves > 0 &&
        (waves.length < totalWaves || specialZones.length > 0)) {
        const accelerate = totalWaves > 5 && waves.length < totalWaves / 3;
        const stage = Math.round(waves.length < Math.round(totalWaves * 0.5)
            ? accelerate
                ? firstHalf / 3
                : firstHalf
            : secondHalf);
        const min = !offset && waves.length < 1 ? 0 : timeStart;
        const max = !offset && waves.length < 1 ? 0 : timeStart + 60;
        if (waves.length >= 1 || offset)
            timeStart = timeStart + stage;
        const BotPreset = (0, exports.getDifficulty)(difficulty);
        // console.log(wildSpawnType, BotPreset);
        // Math.round((1 - waves.length / totalWaves) * maxSlots) || 1;
        let slotMax = Math.round((moreGroups ? Math.random() : Math.random() * Math.random()) * maxSlots);
        if (slotMax < 1)
            slotMax = 1;
        let slotMin = (Math.round(Math.random() * slotMax) || 1) - 1;
        if (wildSpawnType === "marksman" && slotMin < 1)
            slotMin = 1;
        waves.push({
            BotPreset,
            BotSide: getBotSide(wildSpawnType),
            SpawnPoints: getZone(specialZones, combinedZones, waves.length >= totalWaves),
            isPlayers: isPlayer,
            slots_max: slotMax,
            slots_min: slotMin,
            time_min: min,
            time_max: max,
            WildSpawnType: wildSpawnType,
            number: waves.length,
            sptId: wildSpawnType + waves.length,
            SpawnMode: ["regular", "pve"],
        });
        maxSlotsReached -= slotMax;
        // if (wildSpawnType === "assault") console.log(slotMax, maxSlotsReached);
        if (maxSlotsReached <= 0)
            break;
    }
    // console.log(waves.map(({ slots_min }) => slots_min));
    return waves;
};
exports.waveBuilder = waveBuilder;
const getZone = (specialZones, combinedZones, specialOnly) => {
    if (!specialOnly && combinedZones.length)
        return combinedZones[Math.round((combinedZones.length - 1) * Math.random())];
    if (specialZones.length)
        return specialZones.pop();
    return "";
};
const getDifficulty = (diff) => {
    const randomNumb = Math.random() + diff;
    switch (true) {
        case randomNumb < 0.55:
            return "easy";
        case randomNumb < 1.4:
            return "normal";
        case randomNumb < 1.85:
            return "hard";
        default:
            return "impossible";
    }
};
exports.getDifficulty = getDifficulty;
const shuffle = (array) => {
    let currentIndex = array.length, randomIndex;
    // While there remain elements to shuffle.
    while (currentIndex != 0) {
        // Pick a remaining element.
        randomIndex = Math.floor(Math.random() * currentIndex);
        currentIndex--;
        // And swap it with the current element.
        [array[currentIndex], array[randomIndex]] = [
            array[randomIndex],
            array[currentIndex],
        ];
    }
    return array;
};
exports.shuffle = shuffle;
const getBotSide = (spawnType) => {
    switch (spawnType) {
        case "pmcBEAR":
            return "Bear";
        case "pmcUSEC":
            return "Usec";
        default:
            return "Savage";
    }
};
const buildBossBasedWave = (BossChance, BossEscortAmount, BossEscortType, BossName, BossZone, raidTime) => {
    return {
        BossChance,
        BossDifficult: "normal",
        BossEscortAmount,
        BossEscortDifficult: "normal",
        BossEscortType,
        BossName,
        BossPlayer: false,
        BossZone,
        Delay: 0,
        ForceSpawn: false,
        IgnoreMaxBots: true,
        RandomTimeSpawn: false,
        Time: raidTime ? Math.round(Math.random() * (raidTime * 5)) : -1,
        Supports: null,
        TriggerId: "",
        TriggerName: "",
        SpawnMode: ["regular", "pve"],
    };
};
exports.buildBossBasedWave = buildBossBasedWave;
exports.zombieTypes = [
    "infectedassault",
    "infectedpmc",
    "infectedlaborant",
    "infectedcivil",
];
exports.zombieTypesCaps = [
    "infectedAssault",
    "infectedPmc",
    "infectedLaborant",
    "infectedCivil",
];
const getRandomDifficulty = (num = 1.5) => (0, exports.getDifficulty)(Math.round(Math.random() * num * 10) / 10);
exports.getRandomDifficulty = getRandomDifficulty;
const getRandomZombieType = () => exports.zombieTypesCaps[Math.round((exports.zombieTypesCaps.length - 1) * Math.random())];
exports.getRandomZombieType = getRandomZombieType;
const buildBotWaves = (botTotal, escapeTimeLimit, maxGroup, groupChance, bossZones, difficulty, botType, ForceSpawn, botDistribution, spawnDelay = 0) => {
    if (!botTotal)
        return [];
    const pushToEnd = botDistribution > 1;
    const pullFromEnd = botDistribution < 1;
    const botToZoneTotal = bossZones.length / botTotal;
    const isMarksman = botType === "marksman";
    const isPMC = botType === "pmcUSEC" || botType === "pmcBEAR";
    let startTime = pushToEnd
        ? Math.round((botDistribution - 1) * escapeTimeLimit)
        : spawnDelay;
    escapeTimeLimit = pullFromEnd
        ? Math.round(escapeTimeLimit * botDistribution)
        : Math.round(escapeTimeLimit - startTime);
    const averageTime = Math.round(escapeTimeLimit / botTotal);
    const waves = [];
    let maxSlotsReached = botTotal;
    if (maxGroup < 1)
        maxGroup = 1;
    while (botTotal > 0) {
        const allowGroup = groupChance > Math.random();
        let bossEscortAmount = allowGroup
            ? Math.round(maxGroup * Math.random())
            : 0;
        if (bossEscortAmount < 0 ||
            (bossEscortAmount > 0 && bossEscortAmount + 1 > maxSlotsReached)) {
            bossEscortAmount = 0;
        }
        const totalCountThisWave = isMarksman ? 1 : bossEscortAmount + 1;
        const totalCountThusFar = botTotal - maxSlotsReached;
        const BossDifficult = (0, exports.getDifficulty)(difficulty);
        waves.push({
            BossChance: 100,
            BossDifficult,
            BossEscortAmount: bossEscortAmount.toString(),
            BossEscortDifficult: BossDifficult,
            BossEscortType: botType,
            BossName: botType,
            BossPlayer: false,
            BossZone: bossZones[Math.floor(totalCountThusFar * botToZoneTotal)] || "",
            ForceSpawn,
            IgnoreMaxBots: ForceSpawn,
            RandomTimeSpawn: false,
            Time: startTime,
            Supports: null,
            TriggerId: "",
            TriggerName: "",
            SpawnMode: isPMC ? ["pve"] : ["regular", "pve"],
        });
        startTime += Math.round(totalCountThisWave * averageTime);
        maxSlotsReached -= 1 + (isMarksman ? 0 : bossEscortAmount);
        if (maxSlotsReached <= 0)
            break;
    }
    // isMarksman &&
    //   console.log(
    //     // bossZones,
    //     botType,
    //     bossZones.length,
    //     waves.map(({ Time, BossZone }) => ({ Time, BossZone }))
    //   );
    return waves;
};
exports.buildBotWaves = buildBotWaves;
const buildZombie = (botTotal, escapeTimeLimit, botDistribution, BossChance = 100) => {
    if (!botTotal)
        return [];
    const pushToEnd = botDistribution > 1;
    const pullFromEnd = botDistribution < 1;
    let startTime = pushToEnd
        ? Math.round((botDistribution - 1) * escapeTimeLimit)
        : 0;
    escapeTimeLimit = pullFromEnd
        ? Math.round(escapeTimeLimit * botDistribution)
        : Math.round(escapeTimeLimit - startTime);
    const averageTime = Math.round(escapeTimeLimit / botTotal);
    const waves = [];
    let maxSlotsReached = botTotal;
    while (botTotal > 0) {
        const allowGroup = 0.2 > Math.random();
        let bossEscortAmount = allowGroup ? Math.round(4 * Math.random()) : 0;
        if (bossEscortAmount < 0)
            bossEscortAmount = 0;
        const totalCountThisWave = bossEscortAmount + 1;
        const main = (0, exports.getRandomZombieType)();
        waves.push({
            BossChance,
            BossDifficult: "normal",
            BossEscortAmount: "0",
            BossEscortDifficult: "normal",
            BossEscortType: main,
            BossName: main,
            BossPlayer: false,
            BossZone: "",
            Delay: 0,
            IgnoreMaxBots: false,
            RandomTimeSpawn: false,
            Time: startTime,
            Supports: new Array(bossEscortAmount).fill("").map(() => ({
                BossEscortType: (0, exports.getRandomZombieType)(),
                BossEscortDifficult: ["normal"],
                BossEscortAmount: "1",
            })),
            TriggerId: "",
            TriggerName: "",
            SpawnMode: ["regular", "pve"],
        });
        startTime += Math.round(totalCountThisWave * averageTime);
        maxSlotsReached -= 1 + bossEscortAmount;
        if (maxSlotsReached <= 0)
            break;
    }
    // console.log(waves)
    return waves;
};
exports.buildZombie = buildZombie;
const getHealthBodyPartsByPercentage = (num) => {
    const num35 = Math.round(35 * num);
    const num100 = Math.round(100 * num);
    const num70 = Math.round(70 * num);
    const num80 = Math.round(80 * num);
    return {
        Head: {
            min: num35,
            max: num35,
        },
        Chest: {
            min: num100,
            max: num100,
        },
        Stomach: {
            min: num100,
            max: num100,
        },
        LeftArm: {
            min: num70,
            max: num70,
        },
        RightArm: {
            min: num70,
            max: num70,
        },
        LeftLeg: {
            min: num80,
            max: num80,
        },
        RightLeg: {
            min: num80,
            max: num80,
        },
    };
};
exports.getHealthBodyPartsByPercentage = getHealthBodyPartsByPercentage;
const setEscapeTimeOverrides = (locationList, mapConfig, logger, config) => {
    for (let index = 0; index < locationList.length; index++) {
        const mapSettingsList = Object.keys(config_json_1.default.mapConfigs);
        const map = mapSettingsList[index];
        const override = config_json_1.default.mapConfigs[map].EscapeTimeLimitOverride;
        const hardcodedEscapeLimitMax = 5;
        if (!override &&
            locationList[index].base.EscapeTimeLimit / constants_1.defaultEscapeTimes[map] >
                hardcodedEscapeLimitMax) {
            const maxLimit = constants_1.defaultEscapeTimes[map] * hardcodedEscapeLimitMax;
            logger.warning(`[MOAR] EscapeTimeLimit set too high on ${map}\nEscapeTimeLimit changed from ${locationList[index].base.EscapeTimeLimit} => ${maxLimit}\n`);
            locationList[index].base.EscapeTimeLimit = maxLimit;
        }
        if (override && locationList[index].base.EscapeTimeLimit !== override) {
            console.log(`[Moar] Set ${map}'s Escape time limit to ${override} from ${locationList[index].base.EscapeTimeLimit}\n`);
            locationList[index].base.EscapeTimeLimit = override;
            locationList[index].base.EscapeTimeLimitCoop = override;
            locationList[index].base.EscapeTimeLimitPVE = override;
        }
        if (config.startingPmcs &&
            locationList[index].base.EscapeTimeLimit / constants_1.defaultEscapeTimes[map] > 2) {
            logger.warning(`[MOAR] Average EscapeTimeLimit is too high (greater than 2x) to enable starting PMCS\nStarting PMCS has been turned off to prevent performance issues.\n`);
            config.startingPmcs = false;
        }
    }
};
exports.setEscapeTimeOverrides = setEscapeTimeOverrides;
const getRandomInArray = (arr) => arr[Math.floor(Math.random() * arr.length)];
exports.getRandomInArray = getRandomInArray;
const enforceSmoothing = (locationList) => {
    for (let index = 0; index < locationList.length; index++) {
        const waves = locationList[index].base.BossLocationSpawn;
        const Bosses = [];
        let notBosses = [];
        const notBossesSet = new Set([
            "infectedLaborant",
            "infectedAssault",
            "infectedCivil",
            ILocationBase_1.WildSpawnType.ASSAULT,
            ILocationBase_1.WildSpawnType.MARKSMAN,
            "pmcBEAR",
            "pmcUSEC",
        ]);
        for (const wave of waves) {
            if (notBossesSet.has(wave.BossName)) {
                notBosses.push(wave);
            }
            else {
                Bosses.push(wave);
            }
        }
        let first = Infinity, last = -Infinity;
        notBosses.forEach((notBoss) => {
            first = Math.min(notBoss.Time, first);
            last = Math.max(notBoss.Time, last);
        });
        if (first < 15)
            first = 15;
        notBosses = notBosses.sort((a, b) => a.Time - b.Time);
        // console.log(notBosses.map(({ Time }) => Time))
        let start = first;
        const smoothingDistribution = config_json_1.default.mapConfigs[constants_1.configLocations[index]]
            .smoothingDistribution;
        const increment = (Math.round((last - first) / notBosses.length) * 2) * smoothingDistribution;
        for (let index = 0; index < notBosses.length; index++) {
            const ratio = (index + 1) / notBosses.length;
            // console.log(ratio);
            notBosses[index].Time = start;
            let inc = Math.round(increment * ratio);
            if (inc < 10)
                inc = 5;
            start += inc;
        }
        // console.log(
        //   configLocations[index],
        //   last,
        //   notBosses.map(({ Time, BossName }) => ({ BossName, Time }))
        // );
        locationList[index].base.BossLocationSpawn = [...Bosses, ...notBosses];
    }
};
exports.enforceSmoothing = enforceSmoothing;
const looselyShuffle = (arr, shuffleStep = 3) => {
    const n = arr.length;
    const halfN = Math.floor(n / 2);
    for (let i = shuffleStep - 1; i < halfN; i += shuffleStep) {
        // Pick a random index from the second half of the array to swap with the current index
        const randomIndex = halfN + Math.floor(Math.random() * (n - halfN));
        // Swap the elements at the current index and the random index
        const temp = arr[i];
        arr[i] = arr[randomIndex];
        arr[randomIndex] = temp;
    }
    return arr;
};
exports.looselyShuffle = looselyShuffle;
