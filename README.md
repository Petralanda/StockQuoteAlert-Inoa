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
- API BRAPI (cotações da B3)

## Pré-requisitos

- .NET SDK instalado (versão 6.0 ou superior recomendada)
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
    "brapi_token": "<your_brapi_token>"
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

## 🔨 Compilação

```bash
dotnet build
```

## Uso

Execute o programa passando três argumentos:

```bash
stock-quote-alert.exe <TICKET> <preco_compra> <preco_venda>
```

### Parâmetros:

- **TICKET**: Código da ação (ex: PETR4, VALE3, ITUB4)
- **preco_compra**: Preço alvo para alerta de compra
- **preco_venda**: Preço alvo para alerta de venda


## Funcionamento

1. O programa consulta a cotação da ação a cada **10 segundos**
2. Exibe no console o preço atual e horário da cotação
3. Quando o preço atinge o valor de compra ou venda:
   - Envia um email de alerta
   - Exibe mensagem no console
4. Novos alertas só são enviados quando:
   - **Compra**: preço cai abaixo do limite e depois sobe novamente
   - **Venda**: preço sobe acima do limite e depois cai novamente

## Formato dos Alertas

### Alerta de Compra
- **Assunto**: `Alerta de Compra - [TICKET]`
- **Corpo**: `Ação [TICKET] atingiu o valor de compra: R$ [PREÇO]`

### Alerta de Venda
- **Assunto**: `Alerta de Venda - [TICKET]`
- **Corpo**: `Ação [TICKET] atingiu o valor de venda: R$ [PREÇO] em [HORÁRIO]`

## Personalização

### Alterar Intervalo de Consulta

No código, localize a linha:
```csharp
await Task.Delay(10000); // 10 segundos
```

Modifique o valor (em milissegundos):
- 5 segundos: `5000`
- 30 segundos: `30000`
- 1 minuto: `60000`


## Considerações finais

- Certifique-se de que editar o `config.json` com suas credenciais e configurações SMTP
- A API BRAPI no plano gratuito possui limites de requisições e suas ações são atualizadas apenas de 30 em 30 minutos, sendo ela a escolhida apenas para fins de testes para desenvolvimento por falta de opções melhores.