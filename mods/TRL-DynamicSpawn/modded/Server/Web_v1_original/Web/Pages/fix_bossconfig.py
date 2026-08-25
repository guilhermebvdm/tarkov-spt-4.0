import re

with open('BossConfig.razor', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace justify-center with justify-start and add padding left to indent them nicely
# Also make the select outlined like in ABPS, and remove the weird popup class if it didn't work.

def replacer(match):
    comment = match.group(1)
    map_title = match.group(2)
    chance_prop = match.group(3)
    zones_prop = match.group(4)
    zones_enum = match.group(5)
    
    return f'''                      <!-- {comment} -->
                      <MudText Typo="Typo.subtitle1" Class="mb-1 mt-6"><b>{map_title}</b></MudText>
                      <MudDivider Class="mb-4 mud-divider" />
                      
                      <MudGrid Class="align-center mb-2 px-4">
                          <MudItem xs="4" sm="3" Class="pl-sm-12">
                              <MudText Typo="Typo.body2">@ChanceLabel</MudText>
                          </MudItem>
                          <MudItem xs="2" sm="1">
                              <MudButton Variant="Variant.Filled" Color="Color.Dark" Size="Size.Small" Class="px-3">@{chance_prop}</MudButton>
                          </MudItem>
                          <MudItem xs="6" sm="6">
                              <MudSlider @bind-Value="{chance_prop}" Min="0" Max="100" Color="Color.Primary" Size="Size.Medium" ValueLabel="false" />
                          </MudItem>
                          <MudItem xs="0" sm="2" />
                      </MudGrid>
                      
                      <MudGrid Class="align-center mb-6 px-4">
                          <MudItem xs="4" sm="3" Class="pl-sm-12">
                              <MudText Typo="Typo.body2">Spawn Zones</MudText>
                          </MudItem>
                          <MudItem xs="8" sm="7">
                              <MudSelect T="string" MultiSelection="true" @bind-SelectedValues="{zones_prop}" Label="Spawn Zones" Variant="Variant.Outlined" AnchorOrigin="Origin.BottomCenter" Class="mt-0" PopoverClass="dark-dropdown" translate="no">
                                  @foreach (var zone in {zones_enum}) {{ <MudSelectItem T="string" Value="@zone" translate="no">@zone</MudSelectItem> }}
                              </MudSelect>
                          </MudItem>
                          <MudItem xs="0" sm="2" />
                      </MudGrid>'''

pattern = re.compile(
    r'<!-- (.*?) -->\s*'
    r'<MudText Typo="Typo\.body1" Class="mb-1 mt-6"><b>(.*?)</b></MudText>\s*'
    r'<MudDivider Class="mb-4 mud-divider" />\s*'
    r'<MudGrid Class="align-center mb-2 px-4">\s*'
    r'<MudItem xs="4" sm="3" Class="d-flex justify-center pr-4">\s*'
    r'<MudText Typo="Typo\.body2">@ChanceLabel</MudText>\s*'
    r'</MudItem>\s*'
    r'<MudItem xs="2" sm="1">\s*'
    r'<MudButton Variant="Variant\.Filled" Color="Color\.Dark" Size="Size\.Small" Class="px-3">@(CurrentBoss\.SpawnChance\.[A-Za-z0-9_]+)</MudButton>\s*'
    r'</MudItem>\s*'
    r'<MudItem xs="6" sm="6">\s*'
    r'<MudSlider @bind-Value="CurrentBoss\.SpawnChance\.[A-Za-z0-9_]+" Min="0" Max="100" Color="Color\.Primary" Size="Size\.Medium" ValueLabel="false" />\s*'
    r'</MudItem>\s*'
    r'<MudItem xs="0" sm="2" />\s*'
    r'</MudGrid>\s*'
    r'<MudGrid Class="align-center mb-6 px-4">\s*'
    r'<MudItem xs="4" sm="3" Class="d-flex justify-center pr-4">\s*'
    r'<MudText Typo="Typo\.body2">Spawn Zones</MudText>\s*'
    r'</MudItem>\s*'
    r'<MudItem xs="8" sm="7">\s*'
    r'<MudSelect T="string" MultiSelection="true" @bind-SelectedValues="([A-Za-z0-9_]+)" Label="Spawn Zones" Variant="Variant\.Text" AnchorOrigin="Origin\.BottomCenter" Class="mt-0" PopoverClass="dark-dropdown" translate="no">\s*'
    r'@foreach \(var zone in (BotZones\.[A-Za-z0-9_]+)\) \{ <MudSelectItem T="string" Value="@zone" translate="no">@zone</MudSelectItem> \}\s*'
    r'</MudSelect>\s*'
    r'</MudItem>\s*'
    r'<MudItem xs="0" sm="2" />\s*'
    r'</MudGrid>', re.DOTALL)

new_content = pattern.sub(replacer, content)

with open('BossConfig.razor', 'w', encoding='utf-8') as f:
    f.write(new_content)

print("Done replacing.")
