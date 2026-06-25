# StanceSync — Propriedades (F12 / BepInEx ConfigurationManager)

> **Plugin:** `hazelify.StanceSync` — hazelify.StanceSync v1.0.1<br>
> **Fonte:** [original/Plugin.cs](original/Plugin.cs)<br>

Todas as propriedades usam `ConfigurationManagerAttributes` apenas com `Order` (nenhuma é **(Avançada)** / `IsAdvanced`) — todas aparecem com "Advanced settings" desligado. A tabela está na ordem de exibição (`Order` decrescente: 4 → 3 → 2 → 1).

## Core

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| Sync leaning with shoulder swapping? | Sincronizar inclinação com troca de ombro? | `bool` | `true` | — | Ao inclinar para a esquerda, o personagem deve trocar de ombro automaticamente? |
| Sync leaning reset? | Sincronizar reset da inclinação? | `bool` | `true` | — | Ao deixar de inclinar para a esquerda, o personagem deve reverter automaticamente a troca de ombro? |
| Disable synced lean while aiming? | Desabilitar inclinação sincronizada ao mirar? | `bool` | `false` | — | Ao mirar pela mira (ADS), desabilita completamente a inclinação sincronizada. |
| Disable synced lean during optic ADS? | Desabilitar inclinação sincronizada ao mirar com óptica? | `bool` | `false` | — | Ao mirar pela mira (ADS), desabilita a inclinação sincronizada apenas ao mirar com uma óptica ampliada. Todas as outras ópticas — como red dots, colimadores ou miras de ferro — permitem a inclinação sincronizada. Isto sobrescreve `Desabilitar inclinação sincronizada ao mirar?`. Se você quer desabilitar a inclinação sincronizada ao mirar com QUALQUER óptica, habilite a outra opção e desabilite esta. Se esta opção estiver habilitada, mas `Desabilitar inclinação sincronizada ao mirar?` não estiver, o comportamento de sincronização sempre funcionará por padrão. |
