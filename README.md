# Projeto Dioflix (Flix Deal) - Arquitetura Serverless no Azure

Este repositório contém o desenvolvimento prático do desafio de projeto "Dioflix" (ou Flix Deal) proposto no Bootcamp da DIO. O foco do projeto é construir o back-end de uma plataforma de catálogo de vídeos utilizando uma arquitetura 100% Serverless na Microsoft Azure.

## 📌 Status do Projeto
**Em andamento** (Concluído até o desenvolvimento da função `GetMovieDetail`).

## 🚀 O que foi construído até o momento

### 1. Provisionamento de Infraestrutura
A base do projeto foi criada na nuvem da Azure contendo:
- **Resource Group**: `FLIXDIO` para agrupar os recursos lógicos.
- **Azure Cosmos DB**: Banco de dados NoSQL (API para NoSQL) operando em modo Serverless (`cosmosdb-flixdiodev001`) para armazenar os metadados dos filmes.
- **Storage Account**: Contas de armazenamento (`storageflixdiodev001`) com contêineres Blob configurados com acesso anônimo de leitura para hospedar os arquivos físicos (vídeos e miniaturas).
- **API Management**: Criado para gerenciar e rotear futuramente as requisições (ainda em configuração).

### 2. Azure Functions (Back-end)
Foram desenvolvidas três microsserviços em .NET utilizando o modelo *Azure Functions Isolated Worker*:

1. **`fnPostDataStorage`**: Função com gatilho HTTP (POST) responsável por receber os arquivos binários (`multipart/form-data`) e fazer o upload diretamente para o contêiner apropriado (vídeo ou imagem) no Blob Storage, retornando as URIs públicas dos arquivos gerados.
2. **`fnPostDatabase`**: Função HTTP (POST) que recebe um payload JSON com as informações do filme (título, ano, URL do vídeo, URL da thumbnail) e utiliza a extensão/binding do Cosmos DB para salvar um novo documento na coleção de filmes.
3. **`fnGetMovieDetail`**: Função HTTP (GET) que recebe um parâmetro `id` via *Query String* e faz a busca direta no Cosmos DB, retornando os detalhes em JSON do filme específico cadastrado.

## 📚 Documentação Técnica

Para o passo a passo detalhado da implementação, decisões de arquitetura e configuração da infraestrutura, consulte o guia oficial que elaboramos durante o projeto:
👉 **[Guia Técnico de Implementação: Projeto Dioflix no Azure](./Guia.md)**

---

## 📸 Evidências de Execução (Screenshots)

Abaixo estão os registros visuais de cada etapa do desenvolvimento concluída até o momento. As imagens originais encontram-se na pasta `screenshots` na raiz do repositório.

### Infraestrutura
* ![Provisionamento Inicial de Recursos](./screenshots/provisionamento%20inicial%20de%20recursos.png)
  *Provisionamento dos recursos na Azure (Resource Group, API Management, Cosmos DB e Storage).*

### Função 1: Upload de Arquivos (Blob Storage)
* ![Primeira função criada para upload](./screenshots/primeira%20função%20criada%20para%20upload%20de%20objetos%20no%20container%20(blob).png)
  *Terminal local executando a function de upload.*
* ![Testando upload de vídeo via Postman](./screenshots/testando%20ulpload%20de%20video.png)
  *Teste bem-sucedido no Postman retornando a URL do Blob.*

### Função 2: Salvando Metadados no Cosmos DB
* ![Segunda função rodando](./screenshots/segunda%20funcao%20salvar%20no%20banco.png)
  *Terminal local executando a function de banco de dados.*
* ![Postman salvando no banco](./screenshots/salvando%20no%20banco%20ok.png)
  *Chamada POST via Postman enviando o JSON com as URLs geradas.*
* ![Cosmos DB OK 2](./screenshots/salvando%20no%20banco%20ok%202.png)
  *Verificação do registro recém-criado pelo Data Explorer na Azure.*
* ![Cargas no Banco](./screenshots/exemplos%20de%20cargas%20no%20banco.png)
  *Visualização de múltiplos filmes inseridos na coleção de itens.*

### Função 3: Resgatando Detalhes do Filme
* ![Terceira função rodando](./screenshots/terceira%20funcao%20get%20detail%20no%20banco.png)
  *Terminal local executando a API de `GetMovieDetail`.*
* ![Resultado do GET no banco](./screenshots/resultado%20do%20get%20no%20banco.png)
  *Retorno de sucesso (Status 200 OK) no Postman trazendo o documento filtrado por ID.*

---
*Desafio de Projeto desenvolvido como parte do Bootcamp da DIO.*

