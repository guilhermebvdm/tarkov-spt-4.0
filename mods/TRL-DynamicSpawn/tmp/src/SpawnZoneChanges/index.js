"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.PmcSpawns = exports.SniperSpawns = exports.ScavSpawns = exports.PlayerSpawns = void 0;
// context/index.js
var playerSpawns_json_1 = require("../../config/Spawns/playerSpawns.json");
Object.defineProperty(exports, "PlayerSpawns", { enumerable: true, get: function () { return __importDefault(playerSpawns_json_1).default; } });
var scavSpawns_json_1 = require("../../config/Spawns/scavSpawns.json");
Object.defineProperty(exports, "ScavSpawns", { enumerable: true, get: function () { return __importDefault(scavSpawns_json_1).default; } });
var sniperSpawns_json_1 = require("../../config/Spawns/sniperSpawns.json");
Object.defineProperty(exports, "SniperSpawns", { enumerable: true, get: function () { return __importDefault(sniperSpawns_json_1).default; } });
var pmcSpawns_json_1 = require("../../config/Spawns/pmcSpawns.json");
Object.defineProperty(exports, "PmcSpawns", { enumerable: true, get: function () { return __importDefault(pmcSpawns_json_1).default; } });
