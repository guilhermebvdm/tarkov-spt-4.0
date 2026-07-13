# references/

Material de referência **read-only** — não é código do projeto e não deve ser editado. O metadado canônico (upstream, commit pinado, LFS, licença) vive em [manifest.json](./manifest.json); **este README não o duplica**.

## Obter as referências (máquina nova)

As fontes gitignored são clonadas com um comando (as demais já vêm versionadas no repo):

```bash
node scripts/setup-references.js           # clona o que faltar
node scripts/setup-references.js --check    # confere presença
```

O script clona cada fonte conforme o `pin` do manifest, roda `git lfs pull` quando aplicável e remove o `.git` dos snapshots.

## Fontes

- **`eft-decompiled/`** — Assembly C# do cliente EFT descompilado (🥇 verdade do cliente). Versionado.

  > ## ⚠️ ARMADILHA: o dump está INCOMPLETO — **102 namespaces VAZIOS**
  >
  > **NUNCA conclua "esse tipo/método não existe" a partir desta pasta.** Ela tem 4561 arquivos, mas **102 diretórios de namespace estão vazios** — entre eles `EFT.HealthSystem`, `EFT.Animations`, `EFT.InventoryLogic`, `EFT.CameraControl`. Tipos centrais como `ActiveHealthController`, `ProceduralWeaponAnimation` e `HealthEffectsComponent` **não estão aqui**, embora existam na DLL.
  >
  > **Causa (diagnosticada em 2026-07-13):** o `ilspycmd` em **modo projeto (`-p`)** aborta ao topar num método que não consegue descompilar (`Error decompiling @060093AD BackendAbstractClass.GetTemplates` → `ArgumentNullException: Parameter 'annotation'`) e **deixa namespaces inteiros vazios**. Confirmado nas versões **10.0.1 e 10.1.0** — atualizar o tool **não** resolve.
  >
  > **O custo real disso:** o item 050 do CustomClasses declarou dois perks "impossíveis" (`Rapid Care`/`Swift Surgeon` "precisam de transpiler"; `Mobile Surgery` "não localizável no estático") — **os dois eram perfeitamente alcançáveis**. O falso-negativo veio daqui.
  >
  > **WORKAROUND (funciona sempre) — descompile o TIPO direto da DLL real:**
  > ```bash
  > export PATH="$PATH:$HOME/.dotnet/tools"
  > ilspycmd "D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll" -t EFT.HealthSystem.ActiveHealthController
  > ```
  > O **nome totalmente qualificado é obrigatório** (`-t ActiveHealthController` sozinho falha com "Could not find type definition").
  >
  > Antes de escrever "NÃO CONFIRMADO" ou "não existe" sobre saúde, animação, inventário ou câmera: **rode o comando acima**.
- **`spt-source/`** — código-fonte do servidor SPT 4.0 (🥇 verdade do servidor: serviços, helpers, fórmulas, rotas). Gitignored (~856 MB), pinado no commit que corresponde ao SPT em `D:/SPT`. Ao atualizar o SPT, atualize o `pin` no manifest e re-rode o setup — senão a lógica diverge do runtime testado.
- **`fika-{server,plugin,headless}/`** — código do FIKA (coop): servidor C#, plugin cliente C# (contém `Fika.Core`) e cliente headless TS. Gitignored, pinados por commit.
- **`SPT-Waypoints-1.8.2/`** — mod DrakiaXYZ-Waypoints, referência de navegação/waypoints de bots. Versionado.
- **`graphs/`** — grafos AST gerados (output, não fonte externa) — ver [graphs/README.md](./graphs/README.md).

> Ordem de citação de evidência (qual fonte vale primeiro) e a regra "grafo aponta, leitura do `arquivo.cs:linha` prova": [AGENTS.md](../AGENTS.md) e [WORKFLOW.md](../WORKFLOW.md).
