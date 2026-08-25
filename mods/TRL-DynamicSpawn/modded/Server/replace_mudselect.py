import re

# 1. Replace in Global.razor
with open('d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Server/Web/Pages/Global.razor', 'r', encoding='utf-8') as f:
    content = f.read()

pattern_global = r'<MudSelect T="string" Label="Active Preset".*?</MudSelect>'
replace_global = '<TRLSingleSelect Label="Active Preset" @bind-Value="Config.ActivePreset" Items="@(new List<string> { \\"Balanced\\", \\"PMC War\\", \\"Scav Infestation\\", \\"Quiet Raid\\", \\"Warzone\\", \\"Random\\" })" />'

content = re.sub(pattern_global, replace_global, content, flags=re.DOTALL)
with open('d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Server/Web/Pages/Global.razor', 'w', encoding='utf-8') as f:
    f.write(content)

# 2. Replace in Events.razor
with open('d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Server/Web/Pages/Events.razor', 'r', encoding='utf-8') as f:
    content = f.read()

pattern_events_bosses = r'<MudSelect T="string" Label="Selected Bosses".*?</MudSelect>'
replace_events_bosses = '<TRLMultiSelect Label="Selected Bosses" @bind-SelectedValues="selectedBosses" Items="@(new List<string> { \\"All\\", \\"Random\\", \\"bossKnight\\", \\"bossTagilla\\", \\"bossKilla\\", \\"bossGluhar\\", \\"bossSanitar\\", \\"bossKolontay\\", \\"bossBully\\", \\"bossBoar\\", \\"bossPartisan\\" })" />'
content = re.sub(pattern_events_bosses, replace_events_bosses, content, flags=re.DOTALL)

pattern_events_maps = r'<MudSelect T="string" Label="Selected Maps".*?</MudSelect>'
replace_events_maps = '<TRLMultiSelect Label="Selected Maps" @bind-SelectedValues="selectedMaps" Items="@(new List<string> { \\"All\\", \\"Random\\", \\"bigmap\\", \\"factory4_day\\", \\"factory4_night\\", \\"interchange\\", \\"laboratory\\", \\"lighthouse\\", \\"rezervbase\\", \\"shoreline\\", \\"tarkovstreets\\", \\"woods\\", \\"sandbox\\" })" />'
content = re.sub(pattern_events_maps, replace_events_maps, content, flags=re.DOTALL)

with open('d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Server/Web/Pages/Events.razor', 'w', encoding='utf-8') as f:
    f.write(content)

# 3. Replace in BossConfig.razor
with open('d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Server/Web/Pages/BossConfig.razor', 'r', encoding='utf-8') as f:
    content = f.read()

# Pattern for MudSelect in BossConfig
pattern_boss_zones = r'<MudSelect T="string" MultiSelection="true" @bind-SelectedValues="([^"]+)" .*?</MudSelect>'
def repl_boss_zones(match):
    prop = match.group(1)
    if prop == "CustomsZones":
        enum_list = "BotZones.CustomsZones"
    elif prop == "FactoryZones":
        enum_list = "BotZones.FactoryZones"
    elif prop == "FactoryNightZones":
        enum_list = "BotZones.FactoryNightZones"
    elif prop == "InterchangeZones":
        enum_list = "BotZones.InterchangeZones"
    elif prop == "LaboratoryZones":
        enum_list = "BotZones.LaboratoryZones"
    elif prop == "LighthouseZones":
        enum_list = "BotZones.LighthouseZones"
    elif prop == "ReserveZones":
        enum_list = "BotZones.ReserveZones"
    elif prop == "ShorelineZones":
        enum_list = "BotZones.ShorelineZones"
    elif prop == "StreetsZones":
        enum_list = "BotZones.StreetsZones"
    elif prop == "WoodsZones":
        enum_list = "BotZones.WoodsZones"
    elif prop == "GroundZeroZones":
        enum_list = "BotZones.GroundZeroZones"
    else:
        enum_list = "BotZones.CustomsZones"
    
    return f'<TRLMultiSelect Label="Spawn Zones" @bind-SelectedValues="{prop}" Items="@({enum_list}.ToList())" />'

content = re.sub(pattern_boss_zones, repl_boss_zones, content, flags=re.DOTALL)
with open('d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TRL-DynamicSpawn/Server/Web/Pages/BossConfig.razor', 'w', encoding='utf-8') as f:
    f.write(content)

print("Replacement complete.")
