# Constituição do harness

Os princípios que orientam este harness — a lei que todo dev, todo agente e todo framework de desenvolvimento respeita, seja qual for a ferramenta. Convenção que contradiga um destes princípios precisa de uma decisão registrada como ADR ([docs/adr/](adr/)) explicando o porquê.

1. **Docs versionados são a fonte de verdade.** Conflito entre chat, memória e arquivos: os arquivos vencem.
2. **AGENTS.md é mapa, não enciclopédia.** Aponta para as fontes; não as duplica. O lint limita o tamanho.
3. **Verificação mecânica vale mais que regra em prosa.** Fronteira testada reprova o PR; fronteira que só existe em texto tende a ser violada.
4. **Entrevista antes de código.** Comportamento de negócio fechado torna-se RN aprovada pela PO — e decisão difícil de reverter vira ADR — antes da primeira linha. Ajuste sem regra de negócio não exige RN.
5. **O resultado é obrigatório; a ferramenta é livre.** Agnosticismo de fornecedor de IA e de framework de desenvolvimento ([ADR-003](adr/003-agnostico-de-ia-e-framework.md)): o que se exige é convenção respeitada, RN, ADR e gates verdes.
6. **Menor incremento vertical verificável.** Fatia fina de ponta a ponta, com evidência, vale mais que camada completa sem tela.
7. **Vocabulário por decreto.** Termo novo entra primeiro no glossário (aprovação da PO), depois no código.
8. **Dois padrões nunca convivem.** A remoção do padrão antigo entra no mesmo PR que introduz o novo.
9. **Dívida técnica só existe registrada** — com dono e gatilho de revisão; caso contrário tende a tornar-se comportamento permanente.
10. **Segurança e integridade no servidor, nunca no cliente**: credencial, validação de dinheiro e permissão são garantias do servidor.
11. **Artefato de framework é efêmero; a verdade é canônica.** As pastas próprias de cada framework de desenvolvimento não são versionadas ([ADR-004](adr/004-fronteira-do-artefato-de-framework.md)); o resultado é aterrissado nos docs (glossário, RN, constituição, ADR) — nunca uma fonte de verdade concorrente.
