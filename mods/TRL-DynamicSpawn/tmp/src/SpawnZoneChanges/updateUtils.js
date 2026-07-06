"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.updateAllBotSpawns = exports.deleteBotSpawn = exports.updateBotSpawn = exports.updateJsonFile = void 0;
const spawnZoneUtils_1 = require("../Spawning/spawnZoneUtils");
const fs = require("fs");
const path = require("path");
const currentDirectory = process.cwd();
// Function to update JSON file
const updateJsonFile = (filePath, callback, successMessage) => {
    // Read the JSON file
    fs.readFile(filePath, "utf8", (err, data) => {
        if (err) {
            console.error("Error reading the file:", err);
            return;
        }
        // Parse the JSON data
        let jsonData;
        try {
            jsonData = JSON.parse(data);
        }
        catch (parseError) {
            console.error("Error parsing JSON data:", parseError);
            return;
        }
        callback(jsonData);
        // Update the JSON object
        // Write the updated JSON object back to the file
        fs.writeFile(filePath, JSON.stringify(jsonData, null, 2), "utf8", (writeError) => {
            if (writeError) {
                console.error("Error writing the file:", writeError);
                return;
            }
            console.log(successMessage);
        });
    });
};
exports.updateJsonFile = updateJsonFile;
const updateBotSpawn = (map, value, type) => {
    map = map.toLowerCase();
    (0, exports.updateJsonFile)(`${currentDirectory}/user/mods/DewardianDev-MOAR/config/Spawns/${type}Spawns.json`, (jsonData) => {
        value.y = value.y + 0.5;
        if (jsonData[map]) {
            jsonData[map].push(value);
        }
        else {
            jsonData[map] = [value];
        }
    }, "Successfully added one bot spawn to " + map);
};
exports.updateBotSpawn = updateBotSpawn;
const deleteBotSpawn = (map, value, type) => {
    map = map.toLowerCase();
    (0, exports.updateJsonFile)(`${currentDirectory}/user/mods/DewardianDev-MOAR/config/Spawns/${type}Spawns.json`, (jsonData) => {
        if (jsonData[map]) {
            const { x: X, y: Y, z: Z } = value;
            let nearest = undefined;
            let nearDist = Infinity;
            jsonData[map].forEach(({ x, y, z }, index) => {
                const dist = (0, spawnZoneUtils_1.getDistance)(x, y, z, X, Y, Z);
                if (dist < nearDist) {
                    nearest = index;
                    nearDist = dist;
                }
            });
            if (nearest) {
                jsonData[map].splice(nearest, 1);
            }
            else {
                console.log("No nearest spawn on " + map);
            }
        }
    }, "Successfully removed one bot spawn from ");
};
exports.deleteBotSpawn = deleteBotSpawn;
const updateAllBotSpawns = (values, targetType) => (0, exports.updateJsonFile)(`${currentDirectory}/user/mods/DewardianDev-MOAR/config/Spawns/${targetType}.json`, (jsonData) => {
    Object.keys(jsonData).forEach((map) => (jsonData[map] = values[map]));
}, "Successfully updated all Spawns");
exports.updateAllBotSpawns = updateAllBotSpawns;
