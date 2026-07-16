# ADR-059: Gate de exec-plan no harness (decisão de triagem cobrada)

## Status
Aceito (harness, 2026-07-16)

## Contexto
O "Fluxo por tarefa" (AGENTS.md, PLANS.md) manda decidir se a tarefa leva exec-plan antes de
codar. Por ser só prosa, o passo pode ser pulado — código de produto entra sem a triagem.
Pelo ADR-003 (agnóstico de IA), o harness valida o resultado, não a boa vontade da ferramenta;
a garantia tem de rodar e reprovar. Espelha a decisão equivalente já registrada no harness do
repositório de front.

Exigir exec-plan para **toda** mudança geraria documentação inútil (ex.: ajuste trivial).

Opções consideradas:
1. Só reforçar a prosa — já existia e não impede o esquecimento.
2. Gate rígido: todo diff de código exige exec-plan — gera doc desnecessária, atrito.
3. Gate que cobra a **decisão de triagem**, não o artefato (escolhida).

## Decisão
- O `check-harness.py` reprova quando o diff da branch (vs `main`) toca `src/**` (código de
  produto .NET) **sem** (a) um exec-plan em `docs/exec-plans/active/`, **ou** (b) uma dispensa
  declarada em algum commit da branch: trailer `Exec-plan: dispensado — <motivo>` (também
  aceita `Exec-plan: <slug-do-plano>`).
- Base indeterminada (sem git, sem `main`) → **aviso**, não erro: não trava dev local nem o CI
  que clona só um repo. O gate morde no PR/CI, onde há base.
- Registrar a decisão de exec-plan é obrigatório; **o plano em si só quando o trabalho é não-trivial.**

## Consequências
- Trivial passa com uma linha no commit; não-trivial exige o plano antes de implementar.
- A decisão (plano ou dispensa justificada) fica auditável no histórico — agnóstica de ferramenta.
- Rejeitado: gate rígido (doc desnecessária) e reforço só documental (não é blindagem).
