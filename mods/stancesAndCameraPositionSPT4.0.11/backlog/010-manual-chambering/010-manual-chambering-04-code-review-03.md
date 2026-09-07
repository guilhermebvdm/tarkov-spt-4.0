# 010 — Manual Chambering, Dry Rack & Manual Pump · Code Review 03

**Mod:** stancesAndCameraPositionSPT4.0.11  
**Spec funcional:** [010-manual-chambering-01-spec.md](010-manual-chambering-01-spec.md)  
**Spec técnica:** [010-manual-chambering-02-spec-tech.md](010-manual-chambering-02-spec-tech.md)  
**Data:** 2026-09-04  

> Análise crítica do plano de implementação para limpeza do modelo 3D em câmara vazia, suporte a Dry Rack (manuseio no seco), inibição da timeline de empty reload, suporte a Manual Pump para escopetas no F12 e enriquecimento de feedback em Dry Fire. Cada achado recebe um ID `CR-03-MM` permanente.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 2 · 🟡 Médios: 2 · 🟢 Menores: 1 · ✅ Total: 5

---

## Índice

| ID | Categoria | Impacto | Título | Status |
| :--- | :--- | :--- | :--- | :--- |
| **CR-03-01** | B — Bug latente | 🟠 Forte | Transição de Empty Reload: preferir seleção nativa de Tactical Reload via Mecanim a corte abrupto | Aceito no Plano |
| **CR-03-02** | B — Bug latente | 🟠 Forte | Preservação do fluxo de Malfunctions durante Dry Rack e acionamento de câmara | Aceito no Plano |
| **CR-03-03** | D — Arquitetura | 🟡 Médio | Limpeza segura de GameObjects filhos em `patron_in_weapon` sem quebrar armas sem câmara física | Aceito no Plano |
| **CR-03-04** | D — Arquitetura | 🟡 Médio | Detecção restritiva de Escopetas Pump-Action para não afetar escopetas semi/automáticas | Aceito no Plano |
| **CR-03-05** | E — Legibilidade | 🟢 Menor | Feedback de Dry Fire: garantir coerência entre `Weapon.Armed` e som de clique percussor | Aceito no Plano |

---

## Pontos da Revisão

### CR-03-01 · Cat B — Bug latente · 🟠 Forte

**Transição de Empty Reload: preferir seleção nativa de Tactical Reload via Mecanim a corte abrupto**

**Local:** [`Assembly-CSharp/EFT/Player.cs:8480-8508`](../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L8480-L8508) e `ManualChamberingPatches.cs:StartReloadResetPatch`

**Problema:**
Se o código tentar cortar a timeline de animação forçando `SwitchToIdlingState()` no evento `OnMagInsertedToWeapon`, a animação da Unity pode sofrer um "snap" (corte seco) se a mão esquerda do operador ainda estiver concluindo o movimento de saída do poço do carregador em direção ao guarda-mão.

**Por que importa:**
Gera quebra de fluidez visual e pode travar a pose dos braços do jogador.

**Sugestão:**
Garantir que a inibição da manipulação do ferrolho seja feita no **primeiro frame do reload** (`GClass2016.Start` / `StartReloadResetPatch`):
- Forçar `FirearmsAnimator_0.SetAmmoInChamber(1f)` e `FirearmsAnimator_0.SetBoltCatch(false)`.
- Isso faz o Mecanim selecionar organicamente o clipe de **Tactical Reload** do Tarkov, que é uma timeline completa, suave e projetada pela BSG para apenas trocar o carregador e repousar a mão no guarda-mão, sem tocar no ferrolho.

**Decisão:**
- `[x]` Aceitar sugestão (Incorporada ao plano)

---

### CR-03-02 · Cat B — Bug latente · 🟠 Forte

**Preservação do fluxo de Malfunctions durante Dry Rack e acionamento de câmara**

**Local:** [`Assembly-CSharp/Class1730.cs:872-882`](../../references/eft-decompiled/Assembly-CSharp/Class1730.cs#L872-L882) e `ManualChamberingPatches.cs:ManualChamberingInputPatch`

**Problema:**
No Tarkov vanilla, se a arma estiver com pane mecânica (`Weapon.MalfState.State != Weapon.EMalfunctionState.None`, como *Jam*, *Feed* ou *Misfire*), o comando de câmara aciona o exame ou resolução da pane. Se o patch de Dry Rack interceptar o comando incondicionalmente quando a câmara estiver vazia, pode suprimir a resolução de panes de alimentação (*Feed* com câmara 0).

**Por que importa:**
O jogador ficaria incapaz de diagnosticar ou resolver a pane com o comando nativo.

**Sugestão:**
Inserir guarda explícita no `ManualChamberingInputPatch`:
```csharp
if (fc.Weapon.MalfState.State != Weapon.EMalfunctionState.None)
{
    return true; // Deixa o pipeline vanilla de resolução de pane assumir o comando
}
```

**Decisão:**
- `[x]` Aceitar sugestão (Incorporada ao plano)

---

### CR-03-03 · Cat D — Arquitetura · 🟡 Médio

**Limpeza segura de GameObjects filhos em `patron_in_weapon` sem quebrar armas sem câmara física**

**Local:** [`Assembly-CSharp/WeaponManagerClass.cs:385-389`](../../references/eft-decompiled/Assembly-CSharp/WeaponManagerClass.cs#L385-L389)

**Problema:**
Nem todas as armas no EFT possuem os mesmos ossos na hierarquia do prefab (por exemplo, lançadores de granada montados, pistolas de sinalização ou armas especiais). Chamar indexadores fixos `Transform_0[0]` sem checagem de limites pode disparar `IndexOutOfRangeException` ou `NullReferenceException`.

**Por que importa:**
Pode causar erros no console durante a checagem de câmara ou troca de armas.

**Sugestão:**
Encapsular a limpeza com guardas defensivas:
```csharp
var wm = fc.weaponManagerClass;
if (wm != null && wm.Transform_0 != null && wm.Transform_0.Length > 0 && wm.Transform_0[0] != null)
{
    wm.DestroyPatronInWeapon(0);
    Transform t = wm.Transform_0[0];
    for (int i = t.childCount - 1; i >= 0; i--)
    {
        AssetPoolObject.ReturnToPool(t.GetChild(i).gameObject);
    }
}
```

**Decisão:**
- `[x]` Aceitar sugestão (Incorporada ao plano)

---

### CR-03-04 · Cat D — Arquitetura · 🟡 Médio

**Detecção restritiva de Escopetas Pump-Action para não afetar escopetas semi/automáticas**

**Local:** `ManualChamberingPatches.cs` (novo utilitário `IsPumpActionShotgun`)

**Problema:**
O Tarkov possui escopetas semi-automáticas (Saiga-12, MP-153, MP-155, MTs-255 no modo revólver) e automáticas (AA-12). Se o Manual Pump for aplicado de forma genérica para todas as escopetas, armas semi-automáticas pararão de ciclar automaticamente a cada tiro.

**Por que importa:**
Quebraria o funcionamento de armas semi-automáticas populares.

**Sugestão:**
Definir um conjunto estrito de templates de escopetas de bomba (Pump Action):
- Remington Model 870 (`weapon_remington_model_870_12g` / `5a7828548dc32e5a9c28b516`)
- Izhmash MP-133 (`weapon_izhmeh_mr133_12g` / `54491c4f4bdc2db1078b4568`)
- TOZ KS-23M (`weapon_toz_ks-23m_23x75` / `5e8488fa988a99591e728351`)

Apenas essas armas receberão a retenção do ciclo de bomba.

**Decisão:**
- `[x]` Aceitar sugestão (Incorporada ao plano)

---

### CR-03-05 · Cat E — Legibilidade · 🟢 Menor

**Feedback de Dry Fire: garantir coerência entre `Weapon.Armed` e som de clique percussor**

**Local:** [`Assembly-CSharp/EFT/Player.cs:5488-5491`](../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L5488-L5491)

**Problema:**
Em armas de ação simples e percussor lançado, uma vez que o gatilho foi puxado com a câmara vazia, o percussor cai e o mecanismo fica desarmado. Se o jogador continuar clicando no botão do mouse repetidamente, não deve soar múltiplos cliques de disparo como se fosse uma arma de brinquedo, mas sim simular o "gatilho morto" real.

**Por que importa:**
Maximiza o realismo mecânico do simulador.

**Sugestão:**
Permitir o som de Dry Fire apenas enquanto a arma estiver no estado armado (`Weapon.Armed == true`). Ao acionar o primeiro clique no seco, setar `Weapon.Armed = false`. O próximo clique só poderá ocorrer após o jogador ciclar o ferrolho (Dry Rack ou Manual Chambering), que restaura `Weapon.Armed = true`.

**Decisão:**
- `[x]` Aceitar sugestão (Incorporada ao plano)

---

## Conclusão da Revisão

* 🔴 **Bloqueadores: 0** — O plano está tecnicamente consistente e não possui impedimentos arquiteturais.
* 🟠 **Pontos Fortes: 2** — Integrados diretamente ao plano de execução (CR-03-01 e CR-03-02).
* 🟡 **Pontos Médios: 2** — Guardas defensivas e lista estrita de escopetas pump validadas (CR-03-03 e CR-03-04).
* 🟢 **Melhoria: 1** — Lógica de gatilho armado/morto no Dry Fire adicionada (CR-03-05).

**Status:** ✅ **Plano revisado, validado e incorporado.**
