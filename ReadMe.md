# POC - Monitoramento de Logs e Traces com .NET, Jaeger, OpenTelemetry, Prometheus, Grafana, Loki, Elasticsearch e Kibana

## Visão Geral

Este projeto é uma Prova de Conceito (POC) para demonstrar como integrar monitoramento de logs e traces em uma aplicação .NET utilizando as principais ferramentas do ecossistema de observabilidade. O objetivo é centralizar logs, métricas e traces distribuídos, facilitando o diagnóstico e acompanhamento do sistema.

A arquitetura utiliza Docker Compose para orquestrar os seguintes serviços:

- **Jaeger**: Coleta e visualiza traces distribuídos.
- **OpenTelemetry Collector**: Recebe, processa e exporta traces.
- **Prometheus**: Coleta e armazena métricas.
- **Grafana**: Visualiza métricas e logs.
- **Loki**: Armazena e consulta logs.
- **Elasticsearch**: Armazena logs estruturados.
- **Kibana**: Visualiza e pesquisa logs no Elasticsearch.
- **App (.NET)**: Aplicação exemplo instrumentada para gerar logs e traces.

## Como Funciona

- A aplicação .NET ([dotnet-app/Program.cs](dotnet-app/Program.cs)) está instrumentada com OpenTelemetry para gerar traces e com Serilog para logs estruturados.
- Traces são exportados via OTLP para o OpenTelemetry Collector, que encaminha para o Jaeger.
- Logs são enviados para o console, arquivos, Elasticsearch e podem ser integrados ao Loki.
- Prometheus coleta métricas do OpenTelemetry Collector.
- Grafana pode ser configurado para visualizar métricas do Prometheus e logs do Loki.
- Kibana permite explorar os logs armazenados no Elasticsearch.

## Como Executar

1. **Pré-requisitos**: Docker e Docker Compose instalados.
2. **Suba os serviços**:
   ```sh
   docker-compose up --build

Acesse os serviços:

Serviço	         URL Local	                  Descrição
App (.NET)	      http://localhost:5000         Endpoint principal da aplicação
Jaeger UI	      http://localhost:16686   	   Visualização de traces
Prometheus	      http://localhost:9090	      Visualização de métricas
Grafana	         http://localhost:3000	      Dashboards de métricas/logs
Kibana	         http://localhost:5601	      Visualização de logs (ES)
Elasticsearch	   http://localhost:9200	      API do Elasticsearch
Endpoints da Aplicação
GET /
Retorna uma mensagem de teste e gera logs de vários níveis para observabilidade.


Configuração dos Logs e Traces
Logs são enviados para:
Console
Arquivos em dotnet-app/logs/
Elasticsearch (índice: dotnet-logs-YYYY.MM.DD)
Traces são exportados via OTLP para Jaeger.
Métricas são expostas pelo OpenTelemetry Collector para Prometheus.
Usuários Padrão
Grafana:
Usuário: admin
Senha: admin
Arquivos Importantes
docker-compose.yml: Orquestra todos os serviços.
dotnet-app/Program.cs: Instrumentação da aplicação.
otel-collector-config.yaml: Configuração do OpenTelemetry Collector.
prometheus.yaml: Configuração do Prometheus.
Observações
O Elasticsearch está configurado sem autenticação para facilitar testes.
O OpenTelemetry Collector está configurado para receber traces via OTLP (gRPC e HTTP).
Os logs podem ser visualizados tanto no Kibana quanto no Grafana (via Loki).
Referências
Jaeger Documentation
OpenTelemetry
Prometheus
Grafana
Loki
Elasticsearch
Kibana
Para dúvidas ou sugestões, consulte os arquivos de configuração ou abra uma issue.
