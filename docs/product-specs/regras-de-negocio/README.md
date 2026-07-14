# Catálogo de regras de negócio

Este diretório é a fonte única de regra de negócio do produto. As RNs vivem em um arquivo por jornada do corretor, com IDs estáveis `RN-NNN`. Nenhuma RN é implementada antes de ser aprovada pela PO, e todo teste que cobre uma RN carrega o ID dela ([QUALITY_SCORE.md](../../QUALITY_SCORE.md)).

## Como uma RN entra no catálogo

O catálogo cresce **junto com o desenvolvimento**: ao pegar uma atividade que toca comportamento de negócio, o primeiro passo é levantar e refinar a RN **junto com a PO** e catalogá-la antes de escrever código. Não há migração em massa de regras nem uma fila de aprovação separada — a RN nasce refinada e aprovada no início da atividade, e é isso que tira a PO do caminho crítico. A descoberta vem da conversa com a PO e do entendimento do negócio; nada é regra até a PO aprovar.

1. Copiar o [`_template.md`](_template.md) para o arquivo da jornada (um arquivo por jornada, ex.: `cotacao.md`) e redigir a RN em linguagem de negócio, com os termos do [glossário](../glossario.md) — sem path de código no corpo.
2. Refinar com a PO na abertura da atividade; a RN recebe ID definitivo na jornada e passa a poder ser implementada.
3. Testes que implementam a RN carregam o ID dela; a sintaxe e o mapa de rastreabilidade (derivado do código, nunca declarado à mão) estão no [QUALITY_SCORE.md](../../QUALITY_SCORE.md).

## Convenções do catálogo

- ID estável `RN-NNN`: sequência única no catálogo (não reinicia por jornada), três dígitos, atribuído na aprovação e nunca reaproveitado. **O arquivo agrupa (`usuarios.md`), o ID identifica**: é por ele que um teste aponta para uma regra específica — a base da rastreabilidade e do gate de "RN vinculada ao código" ([QUALITY_SCORE.md](../../QUALITY_SCORE.md)). Por isso cada regra é numerada, mesmo já estando num arquivo por jornada.
- Formato de cada RN (ver [`_template.md`](_template.md)): título com o ID, **Descrição**, **Pré-condições**, **Critério de aceitação**, **Casos limite** — sem path de código, sem status declarado à mão.
- RN removida é marcada `[REMOVIDA em YYYY-MM-DD]` no lugar do corpo; ID nunca é reaproveitado.
- RN nova segue o processo acima: refinada com a PO no início da atividade, com validação anti-tecnicismo. A ferramenta de entrevista é livre (ADR-003).
