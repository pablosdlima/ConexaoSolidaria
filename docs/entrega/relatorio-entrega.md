# Relatório de Entrega — Hackathon 11NETT

**Projeto:** Conexão Solidária — plataforma de gestão de doadores e campanhas da ONG Esperança Solidária

## Grupo

**Nome do grupo:** _________________________

| Participante | Username no Discord |
|---|---|
| Pablo Victor Simões de Lima | ____________ |
| ____________ | ____________ |
| ____________ | ____________ |

## Links

| Entregável | Link |
|---|---|
| Repositório (público) | https://github.com/pablosdlima/ConexaoSolidaria |
| Documentação | https://github.com/pablosdlima/ConexaoSolidaria/tree/main/docs |
| Diagrama de arquitetura | https://github.com/pablosdlima/ConexaoSolidaria/blob/main/docs/arquitetura.md |
| Justificativa dos bancos de dados (PDF) | https://github.com/pablosdlima/ConexaoSolidaria/blob/main/docs/entrega/justificativa-bancos-de-dados.pdf |
| Manual de deploy | https://github.com/pablosdlima/ConexaoSolidaria/blob/main/docs/manual-de-deploy.md |
| Pipeline de CI (execuções) | https://github.com/pablosdlima/ConexaoSolidaria/actions |
| Vídeo de demonstração | ____________ |

## Resumo da solução

Dois microsserviços em .NET 10 — **CampanhasAPI** (PostgreSQL) e **DoacoesWorker** (MongoDB) —
comunicando-se por eventos no **RabbitMQ**, atrás de um **API Gateway** (Ocelot), rodando em
**Kubernetes** com **Prometheus + Grafana**, entregues por **GitHub Actions** (build, 68 testes de
unidade, imagens Docker no GHCR com tag do commit).

Requisitos funcionais: JWT com roles GestorONG/Doador; CRUD de campanhas restrito ao gestor
com as regras de data e meta; cadastro público de doador com e-mail único, CPF validado e senha
BCrypt; painel público de campanhas ativas com o valor arrecadado; doação assíncrona — a API
publica o evento, o Worker soma de forma idempotente.

Bônus: testes de unidade no CI; API Gateway.
