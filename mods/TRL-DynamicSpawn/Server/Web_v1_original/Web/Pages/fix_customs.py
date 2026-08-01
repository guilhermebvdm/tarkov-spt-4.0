import re

with open('BossConfig.razor', 'r', encoding='utf-8') as f:
    content = f.read()

def replacer(match):
    return f'''<!-- CUSTOMS -->
                      <MudText Typo="Typo.subtitle1" Class="mb-1 mt-6"><b>Customs</b></MudText>
                      <MudDivider Class="mb-4 mud-divider" />
                      
                      <MudGrid Class="align-center mb-2 px-4">
                          <MudItem xs="4" sm="3" Class="pl-sm-12">
                              <MudText Typo="Typo.body2">@ChanceLabel</MudText>
                          </MudItem>
                          <MudItem xs="2" sm="1">
                              <MudButton Variant="Variant.Filled" Color="Color.Dark" Size="Size.Small" Class="px-3">@CurrentBoss.SpawnChance.Customs</MudButton>
                          </MudItem>
                          <MudItem xs="6" sm="6">
                              <MudSlider @bind-Value="CurrentBoss.SpawnChance.Customs" Min="0" Max="100" Color="Color.Primary" Size="Size.Medium" ValueLabel="false" />
                          </MudItem>
                          <MudItem xs="0" sm="2" />
                      </MudGrid>
                      
                      <MudGrid Class="align-center mb-6 px-4">
                          <MudItem xs="4" sm="3" Class="pl-sm-12">
                              <MudText Typo="Typo.body2">Spawn Zones</MudText>
                          </MudItem>
                          <MudItem xs="8" sm="7">
                              <MudSelect T="string" MultiSelection="true" @bind-SelectedValues="CustomsZones" Label="Spawn Zones" Variant="Variant.Outlined" AnchorOrigin="Origin.BottomCenter" Class="mt-0" PopoverClass="dark-dropdown" translate="no">
                                  @foreach (var zone in BotZones.CustomsZones) {{ <MudSelectItem T="string" Value="@zone" translate="no">@zone</MudSelectItem> }}
                              </MudSelect>
                          </MudItem>
                          <MudItem xs="0" sm="2" />
                      </MudGrid>'''

pattern = re.compile(
    r'<!-- CUSTOMS -->\s*'
    r'<MudText Typo="Typo\.body1" Class="mb-2 mt-2">Customs</MudText>\s*'
    r'<MudDivider Class="mb-4 mud-divider" />\s*'
    r'<MudGrid Class="align-center mb-6">\s*'
    r'<MudItem xs="1" />\s*'
    r'<MudItem xs="4"><MudPaper Class="d-flex align-center mud-width-full ma-1"\s*'
    r'Elevation="0"><MudText>@ChanceLabel</MudText></MudPaper></MudItem>\s*'
    r'<MudItem xs="1"><MudButton Variant="Variant\.Filled" Color="Color\.Dark"\s*'
    r'Size="Size\.Small">@CurrentBoss\.SpawnChance\.Customs</MudButton></MudItem>\s*'
    r'<MudItem xs="5"><MudSlider @bind-Value="CurrentBoss\.SpawnChance\.Customs" Min="0" Max="100"\s*'
    r'Color="Color\.Info" Size="Size\.Medium" ValueLabel="false" /></MudItem>\s*'
    r'<MudItem xs="1" />\s*'
    r'<MudItem xs="1" />\s*'
    r'<MudItem xs="4"><MudPaper Class="d-flex align-center mud-width-full ma-1"\s*'
    r'Elevation="0"><MudText>Spawn Zones</MudText></MudPaper></MudItem>\s*'
    r'<MudItem xs="6">\s*'
    r'<MudSelect T="string" MultiSelection="true" @bind-SelectedValues="CustomsZones" Label=""\s*'
    r'Variant="Variant\.Text" AnchorOrigin="Origin\.BottomCenter" Class="mt-0">\s*'
    r'@foreach \(var zone in BotZones\.CustomsZones\) \{ <MudSelectItem T="string"\s*'
    r'Value="@zone">@zone</MudSelectItem> \}\s*'
    r'</MudSelect>\s*'
    r'</MudItem>\s*'
    r'<MudItem xs="1" />\s*'
    r'</MudGrid>', re.DOTALL)

new_content = pattern.sub(replacer, content)

with open('BossConfig.razor', 'w', encoding='utf-8') as f:
    f.write(new_content)

print("Done replacing.")
