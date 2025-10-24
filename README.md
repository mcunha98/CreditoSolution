# Credito Solution

Solução criada para aplicação de testes com mensageria utilizando RabbitMq para inscrição nos eventos.

## Visão Proposta

Como no descritivo da atividade não havia definicação clara de como encadear os eventos foi criado o projeto seguindo o padrão:

- Cadastrar cliente
- Gerar Proposta Automaticamente (status = Em Analise)
- Executar via service post para api/propostas/decidir com aprovado sendo true/false
- Se aprovado, emite cartões de acordo com o score da proposta

## Calculo de Score da Proposta

Para realizar o calculo foi usada a idade do cliente e a renda para montar uma escala de score que atenda os requisitos definidos
 
