import PresetWeightings from "../config/PresetWeightings.json";
import Presets from "../config/Presets.json";
import { globalValues } from "./GlobalValues";

export const saveToFile = (data, filePath) => {
  var fs = require("fs");
  let dir = __dirname;
  let dirArray = dir.split("\\");
  const directory = `${dirArray[dirArray.length - 4]}/${dirArray[dirArray.length - 3]
    }/${dirArray[dirArray.length - 2]}/`;
  fs.writeFile(
    directory + filePath,
    JSON.stringify(data, null, 4),
    function (err) {
      if (err) throw err;
    }
  );
};

export const cloneDeep = (objectToClone: any) =>
  JSON.parse(JSON.stringify(objectToClone));

export const getRandomPresetOrCurrentlySelectedPreset = () => {
  switch (true) {
    case globalValues.forcedPreset.toLowerCase() === "custom":
      return {};
    case !globalValues.forcedPreset:
      globalValues.forcedPreset = "random";
      break;
    case globalValues.forcedPreset === "random":
      break;

    default:
      return Presets[globalValues.forcedPreset];
  }

  const all = [];

  const itemKeys = Object.keys(PresetWeightings);

  for (const key of itemKeys) {
    for (let i = 0; i < PresetWeightings[key]; i++) {
      all.push(key);
    }
  }

  const preset: string = all[Math.round(Math.random() * (all.length - 1))];
  globalValues.currentPreset = preset;
  return Presets[preset];
};

export const kebabToTitle = (str: string): string =>
  str
    .split("-")
    .map((word) => word.slice(0, 1).toUpperCase() + word.slice(1))
    .join(" ");
