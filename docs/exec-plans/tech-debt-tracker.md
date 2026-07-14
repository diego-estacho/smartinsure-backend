# Tech debt tracker

Dívida técnica aceita conscientemente é registrada aqui com dono e gatilho de revisão (processo em [PLANS.md](../PLANS.md)).

| ID | Descrição | Motivo de aceitar | Dono | Gatilho de revisão |
|---|---|---|---|---|
| TD-001 | `check-harness.py` e `doctor.py` duplicados nos dois repos, sem módulo compartilhado | repos separados clonados isoladamente no CI (ADR-001) impedem import cruzado; custo de manter em sincronia é baixo hoje | harness (Diego) | 3ª divergência entre os lints (regra das 3 ocorrências) ou surgimento de um 3º repo |
