# EFT Decompiled — Referência Interna

Código C# descompilado a partir de assemblies binários do Escape from Tarkov, usado como **referência de leitura** para identificar propriedades, métodos e estruturas internas do jogo ao desenvolver/ajustar mods.

## ⚠️ Aviso legal

- O conteúdo desta pasta é **propriedade intelectual da Battlestate Games (BSG)**.
- **Apenas para análise interna.** Não redistribuir, não publicar, não copiar em repositórios públicos.
- **Não compilar ou usar para criar reimplementações.** Uso restrito a inspeção de API para mods que rodam sobre o cliente original (BepInEx/Harmony patches).
- Este repositório deve permanecer **privado**.

## Origem

| Assembly | Caminho original | SHA256 | Data |
|---|---|---|---|
| `Assembly-CSharp.dll` | `D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll` | `faef6f0b9f142f9d047495ec3dccfd5d6974ac048368dc7045955cf54b117982` | 2026-05-06 |

Descompilado com [ilspycmd](https://github.com/icsharpcode/ILSpy) v10.0.1 em modo projeto (`-p`), preservando estrutura de namespaces e gerando `.csproj` consumível por IDE para navegação.

## Estrutura

```
references/eft-decompiled/
├── README.md                  ← este arquivo
└── Assembly-CSharp/           ← saída do ilspycmd (projeto completo)
    ├── Assembly-CSharp.csproj
    ├── EFT/
    ├── GClass*.cs             ← classes ofuscadas
    └── ...
```

## Como usar

- **Buscar propriedades:** abrir [Assembly-CSharp/](Assembly-CSharp/) no VSCode/IDE e usar Find in Files (`Ctrl+Shift+F`).
- **Stamina:** procurar por `Stamina`, `HandsStamina`, `AimDrainRate`, `Physical` em [Assembly-CSharp/EFT/](Assembly-CSharp/EFT/).
- **Movimentação:** `MovementContext`, `MovementSpeed`.
- **Animação de arma:** `ProceduralWeaponAnimation`, `Spring`.

Classes com nome `GClass####` são tipos ofuscados — pesquisar por suas referências (membros, retornos) para entender o papel.

## Atualização

Quando atualizar a versão do EFT:

1. Substituir DLL: copiar nova `Assembly-CSharp.dll` da pasta `Managed` do jogo.
2. Capturar novo SHA256 e atualizar a tabela acima.
3. Apagar `Assembly-CSharp/` antigo e re-rodar:
   ```bash
   ilspycmd "<caminho>/Assembly-CSharp.dll" -p -o references/eft-decompiled/Assembly-CSharp
   ```
