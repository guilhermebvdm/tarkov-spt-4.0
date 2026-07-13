# Relatório — sessão autônoma noturna (item 072)

> **Data:** 2026-07-13<br>
> **Status:** 🟢 Entregue (pendente de validação in-game)<br>
> **Responsáveis:** execução autônoma via `/g-autodev` (usuário dormindo)<br>
> **Referências:** [plan-2026-07-13...md](./plan-2026-07-13-customclasses-perks-pendentes.md), [mod-backlog.md](../mods/CustomClasses/backlog/mod-backlog.md)<br>

---

## TL;DR

**Os 4 perks foram entregues — inclusive os 2 que o item 050 tinha declarado impossíveis.** Nenhum precisou de transpiler.

O motivo de terem sido dados como impossíveis é a descoberta mais importante da noite: **o decompile do repo estava mentindo**.

## O achado que muda o repo

`references/eft-decompiled/Assembly-CSharp/` tem **102 diretórios de namespace VAZIOS** — entre eles `EFT.HealthSystem`, `EFT.Animations` e `EFT.InventoryLogic`. Tipos centrais (`ActiveHealthController`, `ProceduralWeaponAnimation`, `HealthEffectsComponent`) simplesmente **não estão lá**, embora existam na DLL.

**Causa:** o `ilspycmd -p` (modo projeto) **aborta** ao topar num método que não consegue descompilar (`BackendAbstractClass.GetTemplates` → `ArgumentNullException: 'annotation'`) e **engole namespaces inteiros**. Reproduzido nas versões **10.0.1 e 10.1.0** — **atualizar o tool não resolve** (eu tentei).

**O custo já cobrado:** o as-build do 050 concluiu que Rapid Care/Swift Surgeon *"precisam de transpiler"* e que Mobile Surgery *"não é localizável no estático"*. **Os três eram alcançáveis.** Um dos meus próprios sub-agentes chegou a escrever que `UseTimeFor` era *"uma API alegada, nunca verificada"* — o método é **público e existe** (`AHC:4290`).

**Workaround (funciona sempre)** — já documentado em `references/README.md` e na memória global:
```bash
ilspycmd "D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll" -t EFT.HealthSystem.ActiveHealthController
```
O **nome totalmente qualificado é obrigatório**.

## O que foi entregue

| Perk | Classe | Efeito | Como |
|---|---|---|---|
| 🔧 **Calm Sights** | 🎯 Caçador | sway **×0.7** | Postfix em `UpdateSwayFactors` |
| 🔧 **Rapid Care** | 🩺 Médico | cura/estabilização **×0.7** | escopo armado no `method_5` |
| 🔧 **Swift Surgeon** | 🩺 Médico | cirurgia **×0.5** | idem |
| 🔧 **Mobile Surgery** | 🩺 Médico | **anda** durante a cirurgia | desliga `EPhysicalCondition.HealingLegs` |

Commits: **`d49a5d27`** (implementação, v0.2.0) · **`d039a7bb`** (6 fixes do review, v0.2.1) · `50eed09e` (dívida do decompile) · `d1717671` (backlog/board) · `e372d724` (memória).

**Nenhum perk mostra mais "soon" no painel** — o catálogo lê o F12 ao vivo. Build **0 warnings / 0 erros**, deployado em `D:\SPT`.

## Decisões que tomei sozinho (e o porquê)

1. **Mobile Surgery ENTROU no escopo.** O plano mandava só investigar e só implementar "se surgisse evidência conclusiva". Surgiu: `Player.OnHealthEffectAdded` liga `HealingLegs` quando o efeito reporta `NoMove`, e essa flag é o **único** motivo de `CanWalk == false`. Ela tem **2 escritores** no assembly inteiro e **nenhum re-set por frame**.
2. **Não usei transpiler** (o plano já preferia isso, e a evidência confirmou que nem era preciso).
3. **Não criei branch.** A working tree é **compartilhada com outra sessão** (`TRL-ItemsManagement`); um `checkout -b` mudaria o HEAD debaixo dela. Fiquei em `main`, com commits cirúrgicos.
4. **Corrigi um bug da BSG** (F4 do review), mas **só dentro do escopo do perk**: na cirurgia o vanilla divide o *efeito* por `(1 + SurgerySpeed)` e multiplica a *animação* por `(1 + SurgerySpeed/100)` — **fator 100 de diferença**. Quem não é Médico segue com o comportamento vanilla.
5. **Parei de tentar consertar o decompile** após 2 tentativas (regra do g-autodev). Documentei o workaround em vez de queimar a noite numa limitação de ferramenta.
6. **Versão 0.1.0 → 0.2.1** (o repo tem gate de versão; feature = minor, fixes de review = patch).

## O code-review adversarial

Rodei um cético contra a implementação. Ele **matou 8 hipóteses** com evidência de IL (chegou a descompilar o HarmonyX e o Fika.Core) e trouxe **6 achados reais — todos aplicados**:

- **🟠 F1** — os perks de tempo **vazavam para o mod de cura de aliado** (`TRL-ImmersiveCombatMedicine`): ele chama `DoMedEffect` no corpo do **paciente** de dentro do nosso escopo. Agora o escopo se recusa a armar durante o redirect (lido por reflexão, sem dependência dura).
- **🟡 F2** — `Postfix` → **`[PatchFinalizer]`**. O HarmonyX não roda postfixes se o original **lançar** — e o `DoMedEffect` lança de verdade. O escopo ficaria armado e vazaria para a próxima cura, inclusive de um peer.
- **🟡 F3** — o escopo virou **contador de profundidade**, não bool (o `method_5` pode ser re-entrado).
- **🟡 F4** — o bug da BSG descrito acima.
- **🟢 F5** — no Calm Sights, a checagem de **identidade** agora vem antes da de classe (que podia disparar um **GET HTTP síncrono** rodando para cada arma da cena).
- **🟢 F6** — o trio de patches do Médico **vive ou morre junto**: se um falhar, o perk inteiro se autodesliga (efeito curto + animação vanilla é pior que nada).

Duas boas notícias do review: o HarmonyX **roda** postfixes quando um prefix pula o original (meu medo do "Armed preso pra sempre" era infundado), e o `ObservedPlayer` do Fika **nunca avalia** `HealingLegs` → o Mobile Surgery **não dessincroniza peers**.

## ⚠️ O que precisa de você (não dá para automatizar)

**Nada foi validado no jogo — um agente não joga.** Roteiro curto:

| # | Teste | O que esperar |
|---|---|---|
| **1** | Médico usa **IFAK/Salewa** | cura ~30% mais rápida, **animação acompanhando** (sem gesto cortado) |
| **2** | Médico usa **CMS/Surv12** (cirurgia) | ~metade do tempo, animação casada |
| **3** | ⭐ Médico inicia cirurgia e **aperta W** | **anda** — mas **não corre nem pula**. É o teste mais importante: o C# diz que nada mais trava o jogador, mas um lock no **Animator** (Mecanim) não é inspecionável em código |
| **4** | Caçador mira | arma oscila menos. ⚠️ O sway de **respiração NÃO muda** (é outro sistema — esse é o Iron Lungs) |
| **5** | Médico cura um **aliado** (mod Band_Aid) | tempo **normal**, sem animação cortada (é o F1) |
| **6** | Qualquer classe **não-Médico** | tudo vanilla |

## Riscos que ficaram no código

1. **Mobile Surgery / lock de animação** (teste 3) — se travar mesmo assim, o lock é no Animator e o fix seria outro. O ponto de patch alternativo já está mapeado (Postfix no getter `MovementContext.CanWalk`).
2. **Calm Sights é event-driven** — ligar/desligar o perk no F12 no meio da raid só reflete no próximo evento (mirar, trocar de arma/pose). Não é bug, é como o EFT recalcula.
3. **Band_Aid** — a convivência foi resolvida por reflexão sobre `IsRedirectingHeal`. Se aquele mod renomear o campo, o guard degrada para `false` (volta a vazar). Não é silencioso: é um campo público estável.
4. **Nenhum teste automatizado** cobre isso — não há seam para testar patch Harmony sem o jogo.

## Nada foi pushado

Todos os commits estão **locais**, em `main`, conforme a regra. `git push` continua esperando sua aprovação.
