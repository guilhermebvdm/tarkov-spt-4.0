import { Ixyz } from "@spt/models/eft/common/Ixyz";
import config from "../config/config.json";
import {
  ILocationBase,
  ISpawnPointParam,
} from "@spt/models/eft/common/ILocationBase";

export class globalValues {
  public static baseConfig: typeof config = undefined;
  public static overrideConfig: Partial<typeof config> = undefined;
  public static locationsBase: ILocationBase[] = undefined;
  public static currentPreset: string = "";
  public static forcedPreset: string = "random";
  public static addedMapZones: Record<number, string[]> = {};
  public static indexedMapSpawns: Record<number, ISpawnPointParam[]> = {};
  public static playerSpawn: ISpawnPointParam;
}
