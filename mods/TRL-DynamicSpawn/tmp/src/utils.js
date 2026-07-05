"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.kebabToTitle = exports.getRandomPresetOrCurrentlySelectedPreset = exports.cloneDeep = exports.saveToFile = void 0;
const GlobalValues_1 = require("./GlobalValues");
const saveToFile = (data, filePath) => {
    var fs = require("fs");
    let dir = __dirname;
    let dirArray = dir.split("\\");
    const directory = `${dirArray[dirArray.length - 4]}/${dirArray[dirArray.length - 3]}/${dirArray[dirArray.length - 2]}/`;
    fs.writeFile(directory + filePath, JSON.stringify(data, null, 4), function (err) {
        if (err)
            throw err;
    });
};
exports.saveToFile = saveToFile;
const cloneDeep = (objectToClone) => JSON.parse(JSON.stringify(objectToClone));
exports.cloneDeep = cloneDeep;
const getRandomPresetOrCurrentlySelectedPreset = () => {
    switch (true) {
        case GlobalValues_1.globalValues.forcedPreset.toLowerCase() === "custom":
            return {};
        case !GlobalValues_1.globalValues.forcedPreset:
            GlobalValues_1.globalValues.forcedPreset = "random";
            break;
        case GlobalValues_1.globalValues.forcedPreset === "random":
            break;
        default:
            return Presets[GlobalValues_1.globalValues.forcedPreset];
    }
    const all = [];
    const itemKeys = Object.keys(PresetWeightings);
    for (const key of itemKeys) {
        for (let i = 0; i < PresetWeightings[key]; i++) {
            all.push(key);
        }
    }
    const preset = all[Math.round(Math.random() * (all.length - 1))];
    GlobalValues_1.globalValues.currentPreset = preset;
    return Presets[preset];
};
exports.getRandomPresetOrCurrentlySelectedPreset = getRandomPresetOrCurrentlySelectedPreset;
const kebabToTitle = (str) => str
    .split("-")
    .map((word) => word.slice(0, 1).toUpperCase() + word.slice(1))
    .join(" ");
exports.kebabToTitle = kebabToTitle;
