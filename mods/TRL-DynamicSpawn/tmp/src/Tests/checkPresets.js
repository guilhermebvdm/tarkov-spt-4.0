"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const config_json_1 = __importDefault(require("../../config/config.json"));
function checkPresetLogic(container) {
    const Logger = container.resolve("WinstonLogger");
    for (const key in presetWeightings) {
        if (presets[key] === undefined) {
            Logger.error(`\n[MOAR]: No preset found in PresetWeightings.json for preset "${key}" in Presets.json`);
        }
    }
    for (const key in presets) {
        const preset = presets[key];
        for (const id in preset) {
            if (config_json_1.default[id] === undefined) {
                Logger.error(`\n[MOAR]: No associated key found in config.json called "${id}" for preset "${key}" in Presets.json`);
            }
        }
    }
}
exports.default = checkPresetLogic;
