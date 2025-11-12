# Stock Quote Alert

Sistema de monitoramento de cotações de ações da B3 com alertas automáticos por email quando os preços atingem valores de compra ou venda definidos.

## Descrição

Este projeto monitora continuamente o preço de uma ação específica através da API BRAPI e envia alertas por email quando:
- O preço atinge ou ultrapassa o valor de **compra** definido
- O preço atinge ou fica abaixo do valor de **venda** definido

O sistema possui lógica inteligente para evitar envio de múltiplos alertas consecutivos, enviando novo alerta apenas quando o preço retorna e ultrapassa novamente o limite estabelecido.

## Tecnologias

- C# (.NET)
- System.Net.Mail (envio de emails via SMTP)
- System.Net.Http (requisições HTTP)
- System.Net;  (Para NetworkCredential)
- System.Text.Json (processamento JSON)
- System.Globalization (garantir padronizacao dos valores passados na linha de comando na execucao)
- API BRAPI (cotações da B3)

## Pré-requisitos

- .NET SDK e runtime instalado (versão 9.0 ou superior recomendada)
- Ambiente Windows para utilizar o .exe conforme orientado
- Token da API BRAPI (obtenha em [brapi.dev](https://brapi.dev))
- Conta de email com acesso SMTP configurado

##  Configuração

### 1. Criar arquivo `config.json`

Crie um arquivo `config.json` na raiz do projeto com o seguinte conteúdo:

```json
{
    "Host": "smtp.<dominio>.com",
    "Port": 587,
    "EnableSsl": true,
    "UseDefaultCredentials": false,
    "FromAddress": "<your_user>@<dominio>.com",
    "Password": "<your_password>",
    "ToAddress": "<to_user>@<dominio>.com",
    "brapiToken": "<your_brapi_token>",
    "setTimeOut": 10000 
}
```

#### Parâmetros de Configuração:

- **Host**: Servidor SMTP do seu provedor de email
  - Gmail: `smtp.gmail.com`
  - Outlook: `smtp-mail.outlook.com`
  - Yahoo: `smtp.mail.yahoo.com`
- **Port**: Porta SMTP (geralmente 587 para TLS)
- **EnableSsl**: Habilitação de SSL/TLS (recomendado: `true`)
- **UseDefaultCredentials**: Usar credenciais padrão (geralmente: `false`)
- **FromAddress**: Email remetente
- **Password**: Senha do email ou senha de aplicativo
- **ToAddress**: Email destinatário dos alertas
- **brapi_token**: Token de acesso da API BRAPI
- **setTimeOut**: Intervalo de tempo em milisegundos entre as requisicoes

## Compilação

Caso ja tenha o .NET runtime instalado, pode utilizar o comando a seguir:

```bash
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true -o .
```

Porém pode utilizar outro comando de build mais pesada com as libs base e sem precisar do .NET runtime, dado por:

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o .
```

## Uso

Execute o programa passando três argumentos:

```bash
.\stock-quote-alert.exe <TICKET> <preco_venda> <preco_compra> 
```

### Parâmetros:

- **TICKET**: Código da ação (ex: PETR4, VALE3, ITUB4)
- **preco_venda**: Preço limite para alerta de venda (primeiro argumento)
- **preco_compra**: Preço limite para alerta de compra (segundo argumento)


## Arquitetura e Funções

### 1. `GetConfigs()`
Função estática responsável por:
- Leitura do arquivo `config.json`
- Deserialização das configurações em objeto `Configs`
- Validação da existência das configurações
- Retorno das configurações para uso nas demais funções

### 2. `SendEmail(string subject, string body)`
Função assíncrona para envio de emails que:
- Carrega configurações SMTP via `GetConfigs()`
- Configura cliente SMTP com credenciais e parâmetros de segurança
- Cria e envia mensagem de email com assunto e corpo personalizados
- Trata exceções de envio e exibe feedback no console
- Utiliza `using` para disposal adequado de recursos

### 3. `GetMarketPrice(string ticket, string token)`
Função assíncrona para obtenção de cotações que:
- Constrói URL da API BRAPI com ticker da ação e token opcional
- Executa requisição HTTP GET para a API
- Processa resposta JSON e extrai o preço regular do mercado
- Retorna preço como `float` ou `-1` em caso de erro
- Trata exceções de rede e parsing JSON

### 4. `Main(string[] args)`
Função principal que orquestra o monitoramento:
- **Validação**: Verifica argumentos da linha de comando (ticker, preço venda, preço compra)
- **Inicialização**: Carrega configurações e inicializa variáveis de controle
- **Loop Principal**: Executa indefinidamente o monitoramento com as seguintes etapas:
  - Consulta preço atual via `GetMarketPrice()`
  - Exibe informações no console com contador de tentativas
  - **Lógica de Compra**: Se `preço < limite_compra` e alerta não enviado → envia email de compra
  - **Lógica de Venda**: Se `preço > limite_venda` e alerta não enviado → envia email de venda  
  - **Reset de Flags**: Reseta flags quando preço retorna aos limites para permitir novos alertas
  - **Delay**: Aguarda intervalo configurável (`setTimeOut`) antes da próxima consulta

## Funcionamento Detalhado

### Fluxo de Monitoramento
1. Sistema consulta cotação da ação no intervalo configurado (padrão: 10 segundos)
2. Compara preço atual com limites de compra e venda definidos
3. Envia alertas por email quando condições são atendidas
4. Previne spam através de flags de controle (`buyAlertSent`, `sellAlertSent`)
5. Permite novos alertas apenas após preço sair e retornar às condições

### Lógica de Alertas
- **Alerta de Compra**: Disparado quando preço < limite de compra (oportunidade de compra)
- **Alerta de Venda**: Disparado quando preço > limite de venda (oportunidade de venda)
- **Ambos alertas podem ser enviados na mesma iteração** se condições forem atendidas
- **Reset automático**: Flags são resetados quando preço normaliza

## Formato dos Alertas

### Alerta de Compra
- **Assunto**: `Alerta de Compra - [TICKET]`
- **Corpo**: `Ação [TICKET] está com preço baixo para compra: R$ [PREÇO], dado o limite de compra [LIMITE]`
- **Console**: `"compra enviado com sucesso"`

### Alerta de Venda
- **Assunto**: `Alerta de Venda - [TICKET]`
- **Corpo**: `Ação [TICKET] está com preço alto para venda: R$ [PREÇO], dado o limite de venda [LIMITE]`
- **Console**: `"venda enviado com sucesso"`

### Exemplo de Saída Console
```
Monitorando PETR4 - venda: R$ 35,00 | compra: R$ 28,00
[1ª tentativa] PETR4 = R$ 32,36
[2ª tentativa] PETR4 = R$ 27,90
Alerta de compra enviado com sucesso
[3ª tentativa] PETR4 = R$ 36,10
Alerta de venda enviado com sucesso
```


## Considerações finais

- Certifique-se de que editar o `config.json` com suas credenciais e configurações SMTP
- A API BRAPI no plano gratuito possui limites de requisições e suas ações são atualizadas apenas de 30 em 30 minutos, sendo ela a escolhida apenas para fins de testes para desenvolvimento e por falta de opções melhores.
