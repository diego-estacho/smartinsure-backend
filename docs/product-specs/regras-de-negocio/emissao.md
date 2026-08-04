# Jornada: Emissão (solicitar a emissão da Apólice)

Cada RN é uma seção com o ID no título e os quatro blocos abaixo. O ID é `RN-NNN` numa **sequência única do catálogo** (não reinicia por jornada), estável e nunca reaproveitado. Linguagem de negócio, termos do [glossário](../glossario.md), sem path de código no corpo.

> Escopo desta entrega (etapa de emissão — Passo 5): a partir da Cotação escolhida **Pronta para emissão** (RN-059), completar a minuta, ajustar a taxa se necessário, escolher a forma de pagamento, aceitar o Termo e **solicitar a emissão** da Apólice à Seguradora. A plataforma registra a Apólice como **Emissão solicitada** — o número da apólice, o arquivo e os boletos **não** são buscados nesta fase. Ficam **fora desta fase** (demanda própria): a confirmação da emissão junto à Seguradora (consulta/reconciliação do número, arquivo e boletos), a listagem e a página de detalhes da Apólice, a assinatura da Contragarantia (CCG), o encaminhamento de Cotação em Análise de subscrição (followup), a co-corretagem (distribuição de participação) e o envio de documentos exigidos pela Seguradora. Refinada em 2026-08-03 por entrevista com o dono do produto; **ratificação da PO pendente** ([OPEN-07](../open-decisions.md)). Segue aberta a re-avaliação do veredito por cláusula particular ([OPEN-21](../open-decisions.md)).

## RN-500 — Solicitação da emissão a partir da Cotação escolhida

**Descrição.** O corretor solicita a emissão da Apólice a partir da **Cotação escolhida** do Grupo de Cotação, e somente quando ela está *Pronta para emissão*. Cotação em Análise, Indisponível/Recusada ou Não-reconhecida não é emitida pela plataforma nesta fase.

**Pré-condições.** Grupo de Cotação com Cotação escolhida (RN-059) cujo resultado é *Pronta para emissão* (RN-058). Usuário com Permissão de emitir (RN-513).

**Critério de aceitação.** A etapa de emissão só é alcançável com uma Cotação escolhida *Pronta para emissão*; nas demais classificações a plataforma não oferece o emitir e informa o motivo. A solicitação de emissão sempre se refere à Cotação escolhida do Grupo — nunca a uma irmã, nunca a mais de uma. Ao acionar "Emitir", a plataforma verifica, **antes** de acionar a Seguradora: minuta completa (RN-502), Termo aceito (RN-506), Contragarantia resolvida (RN-501), forma de pagamento válida (RN-505) e ausência de solicitação anterior (RN-507); qualquer verificação reprovada bloqueia com o motivo específico, sem acionar a Seguradora.

**Casos limite.** Grupo sem Cotação escolhida: emissão indisponível. Cotação escolhida em *Análise de subscrição* — seguível pela RN-059 — **não** é emitida aqui: o encaminhamento para acompanhamento é demanda própria, e a plataforma informa isso ao corretor em vez de oferecer o emitir. Escolha trocada por outra Cotação antes de emitir: valem os dados da nova escolhida.

## RN-501 — Contragarantia exigida bloqueia a emissão enquanto não assinada

**Descrição.** Quando a Cotação escolhida registra que a Seguradora **exige Contragarantia (CCG)** e não há assinatura registrada, a plataforma **bloqueia** a solicitação de emissão com motivo explícito. A exigência é capturada na cotação (RN-058) e só é enforçada aqui (RN-059).

**Pré-condições.** Cotação escolhida *Pronta para emissão* que registra exigência de Contragarantia.

**Critério de aceitação.** Cotação que exige Contragarantia **sem** assinatura registrada não permite emitir: a plataforma informa que a Seguradora exige Contragarantia assinada e não aciona a Seguradora. Cotação que exige Contragarantia **com** assinatura registrada segue o fluxo normal de emissão. Cotação que não exige Contragarantia não sofre nenhuma verificação adicional.

**Casos limite.** A assinatura/contrato da Contragarantia é demanda própria (fora desta fase) — nesta fase o bloqueio é um beco sem saída explicado ao corretor, não um caminho acionável na plataforma. O limite máximo sem necessidade de Contragarantia, quando informado pela Seguradora, é dado informativo e não altera o bloqueio.

## RN-502 — Minuta completa e envio definitivo dos termos no emitir

**Descrição.** O preenchimento da minuta — **Tags da minuta** e **Cláusulas particulares** —, opcional na etapa de cotações (RN-062), torna-se **obrigatório** na emissão: todas as Tags da minuta da Cotação escolhida têm de estar preenchidas. Ao emitir, a plataforma **reenvia** os termos e as cláusulas atuais à Seguradora imediatamente antes de solicitar a emissão.

**Pré-condições.** Cotação escolhida com minuta disponível (Tags e Cláusulas particulares da Modalidade — RN-047, RN-048).

**Critério de aceitação.** Existindo Tag da minuta sem valor, a emissão é bloqueada, apontando quais faltam, e a Seguradora não é acionada. Com todas preenchidas, a plataforma envia os termos e cláusulas **vigentes na tela** à Seguradora e só então solicita a emissão — de modo que a Apólice reflita o que o corretor está vendo, mesmo que ele tenha alterado a minuta depois de baixá-la na etapa de cotações. Falha no envio dos termos interrompe a emissão com o motivo do provedor, e nada é registrado como solicitado.

**Casos limite.** Cotação cuja Seguradora não oferece Tag alguma: nada a preencher, segue direto. Cláusula particular marcada não é reavaliada quanto ao veredito da Cotação nesta fase ([OPEN-21](../open-decisions.md)) — o resultado obtido na cotação é preservado; se a Seguradora desviar a emissão por causa da cláusula, vale a RN-511.

## RN-503 — Endereço do Segurado da oferta

**Descrição.** O endereço do Segurado usado na emissão é **escolhido pelo corretor na etapa do Segurado** e **replicado para a oferta** no momento em que o Grupo de Cotação é criado: a oferta passa a ter o seu próprio endereço do Segurado, independente de alterações posteriores no cadastro da Pessoa. Endereço é sempre mantido no cadastro do Segurado — a oferta não edita endereço.

**Pré-condições.** Segurado selecionado com ao menos um endereço cadastrado.

**Critério de aceitação.** Ao criar o Grupo de Cotação, a plataforma replica o endereço escolhido do Segurado para a oferta e é essa réplica que abastece a emissão. Alterar o endereço no cadastro do Segurado **não** altera sozinho a réplica da oferta; para refletir a correção, o corretor volta à etapa do Segurado e confirma o endereço, e a plataforma re-replica os valores atuais — sem descartar as Cotações já obtidas e sem criar Grupo novo (endereço não é dado-base, RN-060). Ao solicitar a emissão, os valores da réplica são registrados na Apólice e são eles que vão à Seguradora.

**Casos limite.** Segurado sem endereço, ou endereço incompleto para o que a Seguradora exige: a emissão é bloqueada com o motivo, e a correção é feita no cadastro do Segurado. Troca do Segurado da oferta é dado-base (RN-060) e cria Grupo novo, que replica o endereço do novo Segurado. Endereço excluído do cadastro depois da réplica: a oferta segue válida com os valores replicados.

## RN-504 — Ajuste da taxa na emissão

**Descrição.** Na etapa de emissão o corretor pode **alterar a taxa** da Cotação escolhida. A alteração é submetida à Seguradora, que devolve prêmio, comissão e opções de parcelamento recalculados — e são esses valores que passam a valer para a Cotação escolhida e para a emissão. Prêmio e comissão não são digitados pelo corretor nem calculados pela plataforma.

**Pré-condições.** Cotação escolhida *Pronta para emissão*, na etapa de emissão.

**Critério de aceitação.** Ao confirmar uma taxa nova, a plataforma a submete à Seguradora e substitui, na Cotação escolhida, o prêmio, a taxa, o percentual e o valor de comissão e as opções de parcelamento pelos valores devolvidos — a Cotação escolhida passa a valer com os novos números, inclusive quando o corretor volta à etapa de cotações. A plataforma valida apenas o formato (valor numérico maior que zero); o limite aceitável é veredito da Seguradora. Recusa da Seguradora preserva os valores anteriores e apresenta o motivo dela ao corretor. A alteração de taxa **não** invalida as Cotações do Grupo nem cria Grupo novo — taxa não é dado-base (RN-060). As Cotações irmãs permanecem com os valores originalmente cotados.

**Casos limite.** Taxa igual à vigente: nada é submetido. Falha de comunicação ao submeter a taxa: valores anteriores preservados, emissão não solicitada, corretor pode tentar de novo. Alteração de taxa após a solicitação de emissão: recusada (RN-507). Limite de taxa por Corretora/Habilitação de Seguradora é pendência registrada ([OPEN-22](../open-decisions.md)).

## RN-505 — Forma de pagamento: parcelamento e vencimento da primeira parcela

**Descrição.** A forma de pagamento da Apólice é escolhida entre as **opções informadas pela Seguradora na própria Cotação**: número de parcelas e dias possíveis para o vencimento da primeira parcela. A plataforma não inventa nem calcula opções de pagamento.

**Pré-condições.** Cotação escolhida com opções de parcelamento e de vencimento informadas pela Seguradora.

**Critério de aceitação.** A etapa de emissão apresenta exatamente as opções de parcelamento e de vencimento da primeira parcela registradas na Cotação escolhida — e o servidor **recusa** parcelamento ou vencimento que não conste nessas listas, ainda que enviado diretamente. Escolha de parcelamento e de vencimento é obrigatória para emitir. Alterada a taxa (RN-504), valem as opções devolvidas pela Seguradora naquele momento, e uma escolha anterior que deixou de existir é descartada, exigindo nova escolha.

**Casos limite.** Cotação sem opção de parcelamento informada: a plataforma emite à vista, sem oferecer escolha. Cotação sem dia de vencimento informado: emite sem vencimento escolhido, deixando o padrão da Seguradora valer. Parcelamento com juros informado pela Seguradora é exibido como tal, sem cálculo da plataforma.

## RN-506 — Termo e declaração: aceite obrigatório e registro

**Descrição.** Emitir exige o **aceite explícito do Termo e declaração** da Seguradora da Cotação escolhida. A plataforma apresenta o texto do Termo vigente daquela Seguradora, registra o aceite do Usuário e informa à Seguradora que houve aceite.

**Pré-condições.** Seguradora da Cotação escolhida com Termo vigente cadastrado.

**Critério de aceitação.** O emitir só é liberado após o aceite explícito do Termo — marcar aceite é ação do Usuário, nunca padrão. No aceite, a plataforma registra **quem** aceitou, **quando**, o **conteúdo exato do texto aceito** e o **agente de acesso** (navegador/dispositivo informado), e comunica o aceite à Seguradora antes de solicitar a emissão. O registro do aceite é preservado ainda que a solicitação de emissão falhe depois — o aceite aconteceu. Seguradora sem Termo vigente cadastrado: emissão bloqueada com o motivo, sem acionar a Seguradora.

**Casos limite.** Termo alterado entre o aceite e a solicitação de emissão: vale e fica registrado o texto que foi exibido e aceito. Nova tentativa de emissão da mesma Cotação exige novo aceite, e cada aceite é registrado. Quem fornece e mantém o texto do Termo de cada Seguradora é pendência registrada ([OPEN-23](../open-decisions.md)); nesta fase o catálogo nasce com um texto único atribuído às Seguradoras.

## RN-507 — Uma única solicitação de emissão por Cotação

**Descrição.** Cada Cotação admite **uma única** solicitação de emissão. Solicitação repetida — segundo clique, reenvio, nova tentativa após sucesso — é recusada pela plataforma, sem acionar a Seguradora.

**Pré-condições.** Cotação escolhida com solicitação de emissão já registrada.

**Critério de aceitação.** Havendo Apólice registrada para a Cotação, a plataforma recusa nova solicitação e apresenta a situação atual, **sem** acionar a Seguradora. Durante o processamento de uma solicitação, o emitir fica indisponível ao corretor. A plataforma **não** repete automaticamente a solicitação em caso de falha ou tempo-limite, porque solicitar a emissão não é operação repetível na Seguradora (mesma razão da RN-057); nova tentativa é sempre ação explícita do corretor.

**Casos limite.** Falha entre a resposta da Seguradora e o registro na plataforma pode deixar a emissão solicitada na Seguradora sem registro aqui; nesse caso a nova tentativa do corretor recebe da Seguradora a informação de que já existe pedido de emissão, e a mensagem é apresentada a ele. A **reconciliação** desse caso — confirmar junto à Seguradora e registrar a Apólice — é demanda própria, junto com a confirmação da emissão (fora desta fase). Grupo cuja Cotação escolhida já teve emissão solicitada não aceita troca de escolha nem alteração de taxa.

## RN-508 — Situação do Grupo de Cotação até a emissão solicitada

**Descrição.** O Grupo de Cotação percorre três situações nesta fase: **Rascunho** (montado no wizard), **Cotado** (Cotações obtidas das Seguradoras) e **Emissão solicitada** (emissão da Cotação escolhida pedida à Seguradora). A plataforma não afirma que a Apólice está emitida — afirma o que sabe: que a emissão foi solicitada.

**Pré-condições.** Grupo de Cotação existente (RN-050).

**Critério de aceitação.** O Grupo em Rascunho passa a **Cotado** quando as Cotações são obtidas (RN-057) e a **Emissão solicitada** quando a solicitação de emissão é aceita pela Seguradora e registrada. Falha em qualquer verificação ou na comunicação com a Seguradora **mantém** o Grupo em Cotado — não existe situação intermediária de "emitindo". Grupo em Emissão solicitada não aceita alteração de dado-base (RN-060), troca de Cotação escolhida (RN-059) nem alteração de taxa (RN-504).

**Casos limite.** A situação **Emitida** — Apólice confirmada pela Seguradora, com número e arquivo — não existe nesta fase: entra com a confirmação da emissão (demanda própria). Grupo criado antes desta regra permanece válido, com a situação derivada do que tem registrado.

## RN-509 — Cancelamento das Cotações irmãs após a emissão solicitada

**Descrição.** Solicitada a emissão de uma Cotação, as **demais Cotações do mesmo Grupo** são canceladas junto às Seguradoras, com o motivo de que outra Cotação do Grupo foi emitida — a proposta aberta numa Seguradora tende a reter Limite de Crédito do Tomador, e não há caso de negócio para mantê-la viva depois da emissão.

**Pré-condições.** Solicitação de emissão aceita e registrada para a Cotação escolhida (RN-500).

**Critério de aceitação.** Após o registro da solicitação de emissão, a plataforma cancela, junto às respectivas Seguradoras, as demais Cotações do Grupo que estejam em condição de cancelamento, informando o motivo. Falha no cancelamento de uma ou mais irmãs **não** desfaz nem invalida a emissão solicitada: o insucesso é registrado e o corretor não é bloqueado.

**Casos limite.** Cotações irmãs já indisponíveis, recusadas ou não-reconhecidas não são canceladas. Cotações de Grupos anteriores do mesmo fork (RN-060) **não** são canceladas — pertencem a outro Grupo e seguem a expiração por inatividade do provedor (RN-061). O cancelamento por saída da cotação e por expiração de tempo segue fora de escopo, em demanda própria.

## RN-510 — Documentos exigidos pela Seguradora

**Descrição.** Quando a Seguradora informa, junto à Cotação, **documentos exigidos** para a emissão, a plataforma os apresenta ao corretor na etapa de emissão, de forma informativa. O envio dos arquivos não é feito pela plataforma nesta fase.

**Pré-condições.** Cotação escolhida com documentos exigidos informados pela Seguradora.

**Critério de aceitação.** Os documentos exigidos informados pela Seguradora são exibidos ao corretor na etapa de emissão, identificados como exigência daquela Seguradora, sem impedir a solicitação de emissão. Cotação sem documentos informados não exibe o bloco.

**Casos limite.** Seguradora que recusa a emissão por falta de documento: vale a RN-511, e o corretor providencia o envio fora da plataforma nesta fase. O envio de documentos pela plataforma é demanda própria.

## RN-511 — O veredito da emissão é da Seguradora

**Descrição.** A plataforma **não** replica as regras de aceitação da Seguradora: quando as verificações da plataforma passam, a solicitação é enviada e o veredito é dela. Recusa, desvio para análise ou qualquer resultado que não seja aceitação da emissão é apresentado ao corretor com o motivo informado pela Seguradora, e o Grupo permanece Cotado.

**Pré-condições.** Verificações da plataforma aprovadas (RN-500) e solicitação enviada à Seguradora.

**Critério de aceitação.** A plataforma envia a solicitação e, recebendo recusa ou resultado diferente de emissão aceita, **não** registra Apólice, mantém o Grupo em Cotado (RN-508) e apresenta ao corretor o motivo informado pela Seguradora. Situações previsíveis mas de decisão da Seguradora — vigência com início no passado, cláusula particular que impede emissão automática, política de taxa, exigência de documento, restrição cadastral do Tomador ou do Segurado — **não** são bloqueadas pela plataforma: são enviadas e decididas por ela.

**Casos limite.** Resultado que a plataforma não reconhece é apresentado como falha na emissão, com o retorno da Seguradora, e registrado para revisão — nunca tratado como emissão aceita. Ausência de motivo na recusa: informa a falha sem detalhamento.

## RN-512 — A emissão usa a Habilitação de Seguradora da cotação

**Descrição.** A emissão é solicitada à Seguradora pela **mesma Habilitação de Seguradora da Corretora que obteve a Cotação**, ainda que essa Habilitação tenha sido inativada depois de cotar. A Cotação carrega a Habilitação que a originou.

**Pré-condições.** Cotação obtida por uma Habilitação de Seguradora da Corretora (RN-056, RN-064).

**Critério de aceitação.** Ao solicitar a emissão, a plataforma usa a Habilitação de Seguradora registrada na Cotação escolhida — não a resolve novamente — e não bloqueia a emissão por inativação posterior dessa Habilitação. A oferta permanece emitível pela Corretora que a cotou.

**Casos limite.** Habilitação de Seguradora removida (não apenas inativada): a emissão é bloqueada com o motivo, pois não há credencial de integração para acionar a Seguradora. Recusa da Seguradora por credencial inválida ou revogada recai na RN-511.

## RN-513 — Permissão de emitir

**Descrição.** Solicitar a emissão de uma Apólice exige **Permissão própria**, distinta das de criar e editar o Grupo de Cotação — é o ato que gera obrigação financeira.

**Pré-condições.** Usuário autenticado com Corretora ativa (RN-064).

**Critério de aceitação.** Usuário sem a Permissão de emitir não vê nem consegue acionar o emitir; o servidor recusa a solicitação ainda que enviada diretamente. A Permissão é declarada no catálogo fixo de Permissões (jornada [Perfis e Permissões](perfis-e-permissoes.md) — referência sem ID por causa da colisão de numeração registrada em [OPEN-24](../open-decisions.md)) e marcável nos Perfis, e nasce concedida aos Perfis que já podem criar Grupo de Cotação.

**Casos limite.** Quais Perfis efetivamente emitem é ajuste de configuração, não de código, e segue sob [OPEN-03](../open-decisions.md). Usuário com vínculo em mais de uma Corretora emite pela Corretora ativa (RN-064).

## RN-514 — Registro da Apólice solicitada

**Descrição.** Aceita a solicitação pela Seguradora, a plataforma registra a **Apólice** da oferta: a referência devolvida pela Seguradora, o número da proposta, os valores efetivamente emitidos, a forma de pagamento escolhida, o endereço do Segurado enviado, o aceite do Termo e quem solicitou a emissão, com data e hora.

**Pré-condições.** Solicitação de emissão aceita pela Seguradora para a Cotação escolhida.

**Critério de aceitação.** Há no máximo **uma** Apólice por Cotação (RN-507), vinculada ao Grupo de Cotação e à Cotação escolhida. O registro guarda os valores vigentes no momento da emissão — prêmio, taxa e comissão, inclusive os recalculados por ajuste de taxa (RN-504) —, o parcelamento e o vencimento escolhidos (RN-505), a réplica do endereço do Segurado enviada (RN-503), a referência ao aceite do Termo (RN-506), o Usuário solicitante e o instante da solicitação. Ao concluir, a plataforma confirma ao corretor que a emissão foi solicitada, identificando a oferta pelo número da proposta devolvido pela Seguradora.

**Casos limite.** Número da apólice, arquivo da apólice e boletos **não** são registrados nesta fase — a Seguradora os disponibiliza depois, e buscá-los é demanda própria (confirmação da emissão). A Apólice registrada não é editável pelo corretor; correção depende da Seguradora.
