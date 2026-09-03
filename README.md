# AutoMatch - Marketplace de Veículos Usados

> **Unidade Curricular:** Laboratório de Aplicações Web e Bases de Dados (LEI)  
> **Instituição:** Universidade de Trás-os-Montes e Alto Douro (UTAD)  

## Autores
* **Tiago Ribeiro**
* **Francisco Rodrigues**
* **Pedro Vieira**

---

## Sobre o Projeto
O **AutoMatch** é uma aplicação web desenvolvida no âmbito da unidade curricular de Laboratório de Aplicações Web e Bases de Dados. Consiste num portal de **Marketplace de veículos usados**, permitindo a interação entre diferentes tipos de utilizadores (Compradores, Vendedores e Administradores) para a compra, venda, reserva e visita de automóveis.

O projeto foi desenvolvido ao longo de três fases principais:
1. **Fase 1:** Análise e Especificação (Requisitos Funcionais/Não Funcionais, Modelo Conceptual E-R e Modelo Funcional/Casos de Uso).
2. **Fase 2:** Modelação Lógica e Física (Modelo Relacional, Script SQL Server, Diagrama de Base de Dados e Mockups em Figma).
3. **Fase 3:** Implementação Prática (Arquitetura MVC em ASP.NET Core, Integração com EF Core, Lógica Funcional, Backoffice e `DbInitializer`).

---

## Funcionalidades Principais por Perfil

### Utilizadores Não Autenticados
* Registo de conta e início de sessão (`Login`).
* Consulta de informações sobre a plataforma ("About Us" e Ajuda).
* Pesquisa avançada de veículos com múltiplos filtros (marca, modelo, preço, ano, quilometragem, combustível, transmissão, etc.).
* Visualização detalhada de anúncios de automóveis.

### Compradores
* Gestão de perfil pessoal (atualização de dados e eliminação de conta).
* Subscrição de notificações e preferências de marcas favoritas.
* Reserva de veículos e marcação de visitas/test-drives.
* Simulação de checkout de compra (cálculo de custos, taxas, seguros e planos de pagamento).
* Comunicação direta com vendedores através do sistema de mensagens.
* Acesso a documentos digitais do automóvel após a compra e acompanhamento do estado das encomendas.

### Vendedores
* Candidatura a Vendedor (sujeita a aprovação administrativa).
* Criação, edição, pausa e remoção de anúncios de veículos, incluindo gestão de fotografias e documentos encriptados.
* Definição e atualização do estado dos anúncios (ativo, reservado, vendido, pausado).
* Acesso a listagens de veículos reservados/vendidos e visualização de estatísticas de vendas.

### Administradores (Backoffice)
* Autenticação reforçada e gestão global da plataforma.
* Visualização e atualização de perfis de utilizador, bloqueio/ativação de contas.
* Moderação de conteúdos e gestão de denúncias de anúncios.
* Análise e aprovação/rejeição de candidaturas a vendedor.
* Acesso a estatísticas globais (crescimento de utilizadores, visão geral de anúncios e relatórios de atividades).

---

## Arquitetura e Tecnologias
* **Arquitetura:** Model-View-Controller (MVC) utilizando o framework **ASP.NET Core**.
* **Base de Dados:** Microsoft SQL Server.
* **Mapeamento Objeto-Relacional (ORM):** Entity Framework Core (com suporte a *Migrations*).

---

## Credenciais de Teste (`DbInitializer`)
A aplicação inclui um inicializador automático da base de dados (`DbInitializer`) que povoa o sistema com dados de teste e utilizadores pré-configurados:

| Perfil | Email | Password |
| :--- | :--- | :--- |
| **Administrador** | `admin@gmail.com` | `password123` |
| **Utilizador Comum** | `user@gmail.com` | `password123` |
| **Vendedor 1** | `seller1@gmail.com` | `password123` |
| **Vendedor 2** | `seller2@gmail.com` | `password123` |

