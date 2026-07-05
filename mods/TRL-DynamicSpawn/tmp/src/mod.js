"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const config_json_1 = require("../config/config.json");
const Spawning_1 = require("./Spawning/Spawning");
const config_json_2 = __importDefault(require("../config/config.json"));
const GlobalValues_1 = require("./GlobalValues");
const routes_1 = require("./Routes/routes");
const checkPresets_1 = __importDefault(require("./Tests/checkPresets"));
const setupSpawn_1 = require("./SpawnZoneChanges/setupSpawn");
class Moar {
    preSptLoad(container) {
        if (config_json_1.enableBotSpawning) {
            (0, routes_1.setupRoutes)(container);
        }
    }
    postDBLoad(container) {
        if (config_json_1.enableBotSpawning) {
            (0, setupSpawn_1.setupSpawns)(container);
        }
    }
    postSptLoad(container) {
        if (config_json_1.enableBotSpawning) {
            (0, checkPresets_1.default)(container);
            GlobalValues_1.globalValues.baseConfig = config_json_2.default;
            GlobalValues_1.globalValues.overrideConfig = {};
            const logger = container.resolve("WinstonLogger");
            logger.info("\n[MOAR]: Starting up, may the bots ever be in your favour!");
            (0, Spawning_1.buildWaves)(container);
        }
    }
}
module.exports = { mod: new Moar() };
