# Jornada: <nome da jornada>

> Este é o template do catálogo. Copie-o para o arquivo da jornada (um arquivo por jornada, ex.: `cotacao.md`, `usuarios.md`) e escreva uma seção `##` por RN. Arquivos começados com `_` (como este) são meta e não entram no catálogo.

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

## RN-NNN — <título curto da regra>

**Descrição.** O que a regra determina, em uma ou duas frases, do ponto de vista do negócio.

**Pré-condições.** O estado necessário para a regra valer (ex.: oferta em rascunho; corretora habilitada para a seguradora).

**Critério de aceitação.** O comportamento observável que prova que a regra foi cumprida — redigido de forma que vire teste (cada teste que a implementa carrega o ID `RN-NNN`).

**Casos limite.** Fronteiras e exceções conhecidas (valores nulos, prazos vencidos, status inesperado da seguradora).

<!-- RN removida: substituir o corpo por "[REMOVIDA em AAAA-MM-DD] — motivo"; o ID nunca é reaproveitado. -->
