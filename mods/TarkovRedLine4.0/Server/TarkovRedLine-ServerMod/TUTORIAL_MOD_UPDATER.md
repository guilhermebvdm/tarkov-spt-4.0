# 📦 RedLine Mod Updater - Tutorial de Configuração

## Visão Geral

O sistema de atualização de mods permite que o servidor distribua automaticamente mods para os jogadores via launcher. O servidor mantém um repositório de referência (`mods_repo`) e o launcher verifica, baixa e sincroniza os arquivos.

---

## Configuração do Servidor

### Arquivo: `config.json`

```json
{
    "serverVersion": "1.5.5",
    "modsRepoPath": "D:\\Tarkov\\mods_repo",
    "managedPaths": [
        "BepInEx/plugins",
        "BepInEx/patchers"
    ]
}
```

### Campos

| Campo | Descrição |
|---|---|
| `serverVersion` | Versão do servidor exibida no launcher |
| `modsRepoPath` | Caminho absoluto da pasta com os arquivos de referência |
| `managedPaths` | Lista de pastas/arquivos gerenciados pelo sistema de update |

---

## Estrutura do `mods_repo`

A pasta `mods_repo` deve espelhar a **estrutura da pasta raiz do jogo**:

```
mods_repo/
├── BepInEx/
│   ├── plugins/
│   │   ├── SAIN/
│   │   │   ├── SAIN.dll
│   │   │   └── config.json
│   │   ├── Fika.Core.dll
│   │   └── MeuMod.dll
│   └── patchers/
│       └── MeuPatcher.dll
└── user/
    └── mods/
        └── MinhaMod/
            └── package.json
```

---

## Como Funciona

### 1. Verificação (automática ao abrir o launcher)
- O launcher busca o **manifesto** do servidor (lista de todos os arquivos com hashes MD5)
- Compara cada arquivo do manifesto com o arquivo local
- Identifica: **faltantes**, **desatualizados** e **extras** (para deletar)

### 2. Ações por Arquivo

| Situação | Ação |
|---|---|
| Arquivo no manifesto, **não existe** local | ⬇️ Baixa do servidor |
| Arquivo no manifesto, hash **diferente** | 🔄 Atualiza (baixa novamente) |
| Arquivo no manifesto, hash **igual** | ✅ Nenhuma ação |
| Arquivo local **não está** no manifesto, dentro de `managedPaths` | 🗑️ **Deleta** |
| Arquivo local **não está** no manifesto, fora de `managedPaths` | ⏭️ Ignora |

### 3. Download e Instalação
- Ao clicar "Atualizar", o launcher baixa os arquivos necessários
- Arquivos extras dentro das pastas gerenciadas são removidos
- A barra de progresso mostra o andamento em tempo real

---

## `managedPaths` - Pastas Gerenciadas

O campo `managedPaths` define **quais pastas o sistema tem permissão de limpar**. Arquivos locais que estão dentro dessas pastas mas **não existem no `mods_repo`** serão **deletados automaticamente** durante a atualização.

### Exemplos

```json
"managedPaths": ["BepInEx/plugins"]
```
→ Apenas `BepInEx/plugins/` é gerenciado. Mods que você removeu do `mods_repo` serão deletados do cliente.

```json
"managedPaths": ["BepInEx/plugins", "BepInEx/patchers", "user/mods"]
```
→ Três pastas são gerenciadas.

```json
"managedPaths": []
```
→ Nenhuma pasta gerenciada. O sistema **nunca deleta** arquivos — apenas adiciona e atualiza.

### ⚠️ Cuidado
- **NÃO inclua** a raiz do jogo (`""` ou `"."`) — isso poderia deletar arquivos essenciais do jogo
- Certifique-se de que o `mods_repo` contém **todos** os mods desejados nas pastas gerenciadas
- Arquivos fora do `managedPaths` **nunca** são tocados pelo sistema

---

## Endpoints do Servidor

| Endpoint | Método | Descrição |
|---|---|---|
| `/launcher/mods/manifest` | GET | Retorna manifesto com arquivos, hashes e managedPaths |
| `/launcher/mods/download?file=<path>` | GET | Baixa um arquivo específico |
| `/launcher/mods/refresh` | GET | Força recálculo do manifesto |

---

## Atualizar o Repositório

1. Copie os mods atualizados para `mods_repo/` mantendo a estrutura de pastas
2. Acesse `http://SERVIDOR:7075/launcher/mods/refresh` para forçar atualização do manifesto (ou aguarde 5 minutos)
3. Os jogadores receberão as atualizações automaticamente ao abrir o launcher
