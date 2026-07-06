import { ILogger } from "@spt/models/spt/utils/ILogger";
import { DependencyContainer } from "tsyringe";
import config from "../../config/config.json";
import presets from "../../config/Presets.json";
import presetWeightings from "../../config/PresetWeightings.json";

export default function checkPresetLogic(container: DependencyContainer) {
  const Logger = container.resolve<ILogger>("WinstonLogger");

  for (const key in presetWeightings) {
    if (presets[key] === undefined) {
      Logger.error(
        `\n[MOAR]: No preset found in PresetWeightings.json for preset "${key}" in Presets.json`
      );
    }
  }

  for (const key in presets) {
    const preset = presets[key];
    for (const id in preset) {
      if (config[id] === undefined) {
        Logger.error(
          `\n[MOAR]: No associated key found in config.json called "${id}" for preset "${key}" in Presets.json`
        );
      }
    }
  }
}
