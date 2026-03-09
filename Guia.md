Guia Técnico de Implementação: Projeto Dioflix no Azure elaborado a partir da apresentação do projeto na DIO

Como Arquiteto de Soluções, este guia foi estruturado para fornecer uma implementação robusta e escalável da plataforma Dioflix. Utilizaremos uma arquitetura serverless baseada em Azure Functions (Isolated Worker Model), Azure Cosmos DB (NoSQL) e Azure Blob Storage, focando em boas práticas de performance, como injeção de dependência Singleton e otimização de conexões.


--------------------------------------------------------------------------------


1. Preparação da Infraestrutura na Cloud (Azure Portal)

A configuração correta dos recursos é vital para evitar erros de permissão (403 Forbidden) e problemas de escalabilidade.

1.1 Resource Group

Crie um Grupo de Recursos para centralizar a governança:

* Nome: FlixDeal
* Região: Selecione a mesma para todos os recursos (ex: East US).

1.2 Azure Cosmos DB (NoSQL)

Utilizaremos a API NoSQL para persistência flexível dos metadados dos filmes.

* Account Name: bflixdealdev001 (Nomes devem ser globalmente únicos).
* API: NoSQL.
* Capacidade: Serverless (Ideal para o modelo de custo por execução do projeto).
* Banco de Dados: FlixDeal.
* Container: movies.
* Partition Key: /id (Crucial para a indexação e busca eficiente).

1.3 Storage Account e Acesso Anônimo

A Storage Account armazenará os binários de vídeo e imagem. Atenção: O acesso anônimo é um processo de duas etapas.

1. Criação: Nome flixdealdev001, tipo Standard, redundância LRS.
2. Etapa 1 (Nível de Conta): Após criar, vá em Configuration (ou Data Protection) e marque "Allow anonymous access on individual containers". Clique em Save.
3. Etapa 2 (Nível de Container): Crie dois containers: images e videos. No momento da criação (ou em Change Access Level), defina o nível de acesso para "Blob (anonymous read access for blobs only)".


--------------------------------------------------------------------------------


2. Configuração do Ambiente de Desenvolvimento

Para garantir a interoperabilidade entre o frontend e o backend, as dependências e configurações locais devem seguir o padrão Isolated Worker.

2.1 Dependências NuGet

Execute os comandos abaixo no terminal do seu projeto:

dotnet add package Azure.Storage.Blobs
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.CosmosDB
dotnet add package Microsoft.Azure.Cosmos
dotnet add package Newtonsoft.Json


2.2 Configurações Locais (local.settings.json)

Evite colisões de nomes. O AzureWebJobsStorage é para o host da Function; crie chaves distintas para suas connection strings.

* CORS: Deve ser uma propriedade de nível superior (dentro de Host), não dentro de Values.

{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDBConnection": "AccountEndpoint=https://bflixdealdev001.documents.azure.com:443/;AccountKey=SUA_CHAVE_AQUI;",
    "StorageConnection": "DefaultEndpointsProtocol=https;AccountName=flixdealdev001;AccountKey=SUA_CHAVE_AQUI;EndpointSuffix=core.windows.net"
  },
  "Host": {
    "CORS": "*"
  }
}



--------------------------------------------------------------------------------


3. Ajustes Globais da Application (Program.cs)

Como seniores, devemos nos preocupar com a Socket Exhaustion (esgotamento de conexões). O CosmosClient deve ser registrado como Singleton para permitir o reuso do pool de conexões TCP. Além disso, ajustaremos o servidor Kestrel para suportar uploads de vídeo.

No arquivo Program.cs, dentro da configuração do IHostBuilder:

.ConfigureServices(services => {
    // Aumento do limite para 100MB para suportar upload de vídeos
    services.Configure<KestrelServerOptions>(options => {
        options.Limits.MaxRequestBodySize = 104857600; 
    });

    // Injeção de Dependência Singleton para performance e otimização de pool
    services.AddSingleton(sp => {
        string connectionString = Environment.GetEnvironmentVariable("CosmosDBConnection");
        return new CosmosClient(connectionString);
    });
})



--------------------------------------------------------------------------------


4. Function 1: DataStorage (Upload de Mídia)

Esta Function gerencia o processamento de arquivos via multipart/form-data.

* Lógica de Roteamento: A Function deve inspecionar o cabeçalho file-type. Se for "image", aponta para o container images; se for "video", para videos.
* Uso do Stream: Utilize HttpRequest.ReadFormAsync() para acessar os arquivos.
* Snippet Lógico:


--------------------------------------------------------------------------------


5. Function 2: SaveMovie (Persistência no Cosmos DB)

Aqui persistimos os metadados. O erro comum "Value cannot be null. Partition Key" ocorre quando o objeto enviado ao binding não possui a propriedade que mapeia para a Partition Key do container.

5.1 Modelo de Dados (POJO)

O campo id deve estar presente para satisfazer a Partition Key /id.

public class Movie {
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; }
    public string Year { get; set; }
    public string Video { get; set; }
    public string Thumb { get; set; }
}


5.2 Implementação do Binding

Utilize o CosmosDBOutput para simplificar a escrita. Certifique-se de que o id foi gerado antes do retorno da função.


--------------------------------------------------------------------------------


6. Functions de Recuperação: GetAllMovies e GetMovieDetail

A consulta de dados utiliza o CosmosClient injetado via Singleton para máxima performance.

* GetAllMovies: Executa SELECT * FROM c. Utilize um FeedIterator para percorrer os resultados de forma assíncrona.
* GetMovieDetail: Recebe o id via Query String. Use uma Query parametrizada: SELECT * FROM c WHERE c.id = @id.
* Response Pattern: Sempre utilize req.CreateResponse(HttpStatusCode.OK) e serialize o objeto no corpo para garantir que o frontend receba o JSON com o Content-Type correto.


--------------------------------------------------------------------------------


7. Integração e Validação Final

A validação garante que a cadeia de comunicação entre o backend e a nuvem está operante.

1. Execução de Múltiplas Funções: No Visual Studio, configure a solução para rodar múltiplos projetos simultaneamente ou inicie instâncias separadas do Host.
2. Teste de Fluxo via Postman:
  * Passo 1: POST para DataStorage enviando um arquivo no Body (form-data) e o header file-type. Guarde a blobUri.
  * Passo 2: POST para SaveMovie enviando um JSON com o título e a blobUri obtida anteriormente.
3. Verificação no Portal:
  * Cosmos DB Explorer: O documento deve aparecer com o id (GUID) preenchido.
  * Storage Browser: O arquivo deve estar no container correto e ser acessível via browser sem autenticação (testando o acesso anônimo).
4. Dica de Performance: Monitore as conexões estabelecidas. Graças ao Singleton do CosmosClient, você notará que o número de conexões ativas se mantém estável mesmo sob múltiplas requisições.
