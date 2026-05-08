# /add-backlog-item

Cria um novo item no backlog de um mod e dispara o `/create-spec` em sequência.

## Uso

```
/add-backlog-item <mod> <descrição livre>
```

- `<mod>` — nome da pasta em `mods/` (ex: `stancesAndCameraPositionSPT4.0.11`). Validar que existe.
- `<descrição livre>` — qualquer texto descrevendo a feature/correção. Pode ser informal.

## O que fazer

1. **Validar o mod.** Se `mods/<mod>/` não existir, listar mods disponíveis e parar.

2. **Resumir a descrição (enxuto).** A partir do texto livre do usuário, derivar:
   - **Título** em pt-BR, kebab-case, máx. 5 palavras (ex: `recoil-baixo-em-mira`).
   - **Resumo** em 1–2 frases (≤ 200 chars). Direto ao ponto, sem preâmbulo.

3. **Calcular `NNN`.** Listar `mods/<mod>/backlog/` e identificar o maior diretório que casa `^[0-9]{3}-`. Próximo = maior + 1, padded a 3 dígitos. Se vazio, começa em `001`.

4. **Criar/atualizar `mod-backlog.md`.**
   - Se `mods/<mod>/backlog/mod-backlog.md` não existe: renderizar `.agents/templates/mod-backlog.md.tmpl` substituindo `{{MOD}}`.
   - Adicionar nova linha logo após o cabeçalho da tabela:
     ```markdown
     | NNN | Título | Resumo curto | [NNN-slug/](./NNN-slug/) | ⚪ |
     ```
     Status emojis: ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

5. **Criar pasta** `mods/<mod>/backlog/NNN-<slug>/` (vazia).

6. **Confirmar ao usuário** com preview:
   ```
   ✓ Item NNN — <Título> adicionado em <mod>
     Pasta: mods/<mod>/backlog/NNN-<slug>/
     Próximo: invocando /create-spec...
   ```

7. **Invocar `/create-spec`** automaticamente passando o path da pasta criada como referência. Não pedir confirmação adicional.

## Regras

- **Sempre passar `<mod>` explicitamente.** Não tentar inferir.
- Se a descrição for muito vaga para resumir (< 10 palavras de conteúdo), pedir clarificação antes.
- Slug em pt-BR sem acentos, sem stopwords (`de`, `do`, `para`, `em`).
- Nunca sobrescrever uma pasta `NNN-` existente.
