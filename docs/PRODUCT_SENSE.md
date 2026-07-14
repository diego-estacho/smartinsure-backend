# Sentido de produto

Como pensar o produto ao tomar decisões do dia a dia. A palavra final de negócio é da PO; este arquivo existe para que dev e agente errem menos antes de perguntar.

- **O produto**: plataforma do corretor para cotação e emissão de seguros, começando por Seguro Garantia. Independente de seguradora: seguradoras são integrações plugáveis, e a corretora é o cliente.
- **O usuário é o corretor.** A jornada dele ordena a construção: oferta → cotações → proposta/emissão → pós-emissão. Feature que não encurta nem destrava essa jornada precisa de justificativa.
- **Vocabulário é produto** ([glossário](product-specs/glossario.md)): os termos valem em código, API e UI. Inversão de vocabulário é bug de produto, não detalhe técnico.
- **Pontos de dinheiro** (cálculo, emissão, pagamento, transição de status) carregam o maior peso de verificação e review — erro aqui custa dinheiro de cliente.
- **Regra de negócio só existe catalogada**: aprovada pela PO, com ID no [catálogo de RNs](product-specs/regras-de-negocio/README.md). Comportamento sem RN é suposição.
- **Decisão aberta bloqueia implementação** ([open-decisions](product-specs/open-decisions.md)): na dúvida, parar e registrar a pergunta com dono sugerido — nunca inventar.
