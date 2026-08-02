---
name: trl-mod-publishing
description: Regras de publicação pública de mods SPT no Forge (forge.sp-tarkov.com) e o padrão de identidade TRL (GUID, nome do plugin, assembly, pasta no BepInEx, arquivo de config, versão). Use durante /prepare-mod-for-publish, ao renomear/rebrandear um mod, ao criar um mod TRL novo, ou sempre que a tarefa mencionar publicar, distribuir ou lançar um mod para terceiros. Complementa `repo-workflow-best-practices` (fluxo interno) e `spt-mod-best-practices` (lifecycle).
---

# TRL Mod Publishing

Duas coisas: **(1)** o que o Forge exige de um mod público e **(2)** o padrão de identidade dos mods TRL.

> ⚠️ **A fonte é externa e muda.** O canônico é <https://forge.sp-tarkov.com/content-guidelines>. Nas citações
> abaixo, `§N.N` é a seção de lá. **Reconfirmar a redação vigente na fase 1** de toda auditoria — mudança de
> política do Forge não avisa ninguém, e esta skill pode estar desatualizada sem parecer.
>
> **Dois níveis de confiança, marcados item a item:**
> - 📌 **citação verificada** — texto literal conferido na fonte. Pode sustentar um bloqueio 🔴 sozinho.
> - 📄 **leitura resumida** — paráfrase de uma leitura anterior, sem conferência literal. **Não** sustenta 🔴
>   sozinha: o achado sai como "confirmar antes de publicar" até alguém verificar e promover a 📌.

> **Escopo.** Vale para mod que vai a público. Mod interno do servidor TRL não precisa passar aqui — mas adotar
> a identidade (§4) desde o início evita a renomeação dolorosa depois (§4.4).

## 1. Portões de elegibilidade — antes de qualquer faxina de código

São de política, não de engenharia: nenhum se resolve escrevendo código melhor. **Sempre a primeira fase.**

| Portão | Regra | Como verificar |
|---|---|---|
| **Licença** | 📌 §6.1: OSI-approved (MIT, Apache 2.0, GPL, BSD…) "provide established legal frameworks for code distribution"; Creative Commons é apontada como apropriada para **conteúdo não-código** — "documentation, artwork, and media files" | Ler `LICENSE`. **CC cobrindo o código** (BY, BY-NC, BY-SA) não é reprovação automática pela letra da regra, mas é a licença errada para código pela diretriz: tratar como **decisão humana** e buscar relicenciamento OSI |
| **Permissão do autor** | 📌 §6.2, literal: *"Submission of existing user-contributed content without obtaining permission from the original authors is strictly prohibited."* | Ler `mod.json` → `upstream_url`. Havendo upstream, exigir o **registro** da autorização (link/print/mensagem) — não a lembrança de alguém |
| **Política de IA** | 📌 §4.2, literal: *"The Forge does not accept mods that have been substantially or entirely written by AI coding agents."* e *"Any usage of LLM-based assistance… requires that the 'Contains AI Content' flag be enabled in the mod properties."* | **Decisão humana declarada e registrada.** Nunca inferir de histórico de commits. O flag é obrigatório com qualquer uso de LLM |
| **Origem dos assets** | 📄 Imagem, áudio, ícone e bundle embutidos precisam de licença compatível ou substituição | Listar todo binário não-código e cobrar a origem de cada um |
| **Conteúdo proibido** | 📄 Sem ofuscação/anti-debug; nada usável no EFT online; nada que modifique o sistema fora da pasta SPT; sem coleta de dados não declarada; sem exigência de pagamento (doação opcional pode); sem coletânea de mods | Inspeção do código + declaração |

## 2. Requisitos de pacote e página

Nenhum destes foi conferido na redação literal (📄) — ver a nota de confiança no topo: sustentam achado, não
bloqueio, até alguém verificar e promover a 📌.

| Requisito | Regra |
|---|---|
| **Formato** | 📄 `.zip` ou `.7z`, **sem senha** |
| **Estrutura** | 📄 O usuário extrai na **raiz do SPT** e funciona |
| **Versionamento** | 📄 SemVer, **idêntico** em plugin, config e módulo de servidor |
| **Compatibilidade SPT** | 📄 Declarada; mod de servidor declara a restrição de versão |
| **Código-fonte** | 📄 Todo executável (`.dll`, `.exe`) exige link público (GitHub/GitLab) com **o código exato daquela build** + instruções de build para verificação independente |
| **VirusTotal** | 📄 Link de varredura por executável |
| **Rede** | 📄 Toda comunicação documentada em detalhe. **Mod que usa Fika conta** |
| **Documentação** | 📄 Instalação passo a passo, uso/configuração e **dependências por versão** |
| **Créditos** | 📌 Atribuição ao autor original, salvo dispensa explícita dele (§6.2, literal: *"Proper credit to original authors must be provided unless the authors have explicitly specified that attribution is not necessary."*) |

## 3. Prontidão de terceiro (o que quebra na prática)

Não são regras do Forge — é o que falha quando sai da máquina do autor:

1. **Instalação limpa.** O autor joga com `.cfg` calibrado há meses; o público pega os *defaults*.
2. **Sem os mods do autor.** Rodar sem Fika, sem os outros mods TRL, e ao lado dos populares (Realism, SAIN).
3. **Primeiro boot não pode abortar em silêncio.** `Config.Bind` que lança derruba o `Awake` — ver
   `spt-mod-best-practices`. Caracteres proibidos em chave: `= [ ] " ' \ tab`.
4. **Custo por frame** e **estado estático não resetado entre raids** (sintoma: "só funciona na primeira raid").
5. **Código morto e comentário fóssil** — em mod público viram issue de terceiro.
6. **Idioma.** Público majoritariamente anglófono: opção, tooltip e página em inglês. Bilíngue é decisão, não acidente.

## 4. Padrão de identidade TRL

Toda a identidade deriva de **um nome canônico**: `TRL-<PascalCase>` (ex.: `TRL-StancesAndMobility`).

### 4.1 Tabela canônica

| Onde | Formato | Exemplo |
|---|---|---|
| Nome canônico | `TRL-<PascalCase>` | `TRL-StancesAndMobility` |
| GUID (`BepInPlugin`) | `com.trl.<tudominúsculo>` | `com.trl.stancesandmobility` |
| Nome do plugin (`BepInPlugin`) | = canônico | `TRL-StancesAndMobility` |
| `AssemblyName` (`.csproj`) | = canônico | `TRL-StancesAndMobility` |
| Pasta em `BepInEx/plugins/` | = canônico | `BepInEx/plugins/TRL-StancesAndMobility/` |
| Arquivo de config | `<GUID>.cfg` (derivado pelo BepInEx) | `com.trl.stancesandmobility.cfg` |
| Pasta do mod no repo | = canônico | `mods/TRL-StancesAndMobility/` |
| Prefixo de log | `[<canônico>]` | `[TRL-StancesAndMobility]` |
| Namespace C# | `TarkovRedLine.<PascalCase>` | `TarkovRedLine.StancesAndMobility` |
| `mod.json` do repo | `forge_url`, `license`, `spt_version` preenchidos | — |

**Encaixe com o Forge:** GUID `com.<usuário>.<mod>` e nome `<Usuário>-<Mod>`, só letras/números e **um único**
traço. Com o usuário `TRL`, o nome canônico já está conforme.

**Mod de servidor no mesmo pacote:** o GUID **tem que bater** com o do cliente; nome e autor do lado servidor
aceitam **só letras e números** — use `TRLStancesAndMobility`.

**Versão em três lugares que precisam bater:** `[BepInPlugin(..., "X.Y.Z")]` (é o que o F12 mostra, canônico) ·
`<Version>`/`<AssemblyVersion>`/`<FileVersion>` no `.csproj` (sem isso a DLL sai `1.0.0.0`) · `CHANGELOG.md`.

### 4.2 ⚠️ Renomear identidade quebra a config de todo mundo

O BepInEx identifica cada ajuste pelo par **(seção, chave)** dentro de um arquivo derivado do **GUID**:

- **Trocar o GUID** → `.cfg` novo em branco: **todos os ajustes do usuário voltam ao padrão**.
- **Renomear seção ou chave** → aquela entrada vira órfã e volta ao padrão (as demais sobrevivem).

**Sempre que renomear, entregar junto** o `.cfg` novo já preenchido, distribuído como espelho (no TRL, canal
`config-server` do launcher, que sobrescreve — o canal `config` só cria quando falta e não serve para migração),
e anunciar a quebra no changelog.

## 5. Checklist mínimo

Ordem obrigatória — §1 reprova cedo e barato:

- [ ] **§1** licença · permissão do autor (se fork) · política de IA declarada · origem dos assets · nada proibido
- [ ] **§4** identidade unificada em **todas** as linhas da tabela §4.1 · versão nos 3 lugares · plano de migração de config
- [ ] **§3** instalação limpa · sem os mods do autor · sem estado vazando entre raids · sem código morto
- [ ] **§2** pacote extraível na raiz · fonte pública com build reproduzível · VirusTotal · rede documentada · dependências · créditos