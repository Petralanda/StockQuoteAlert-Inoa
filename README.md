# Status Quote Alert - Inoa

Sistema de monitoramento e alerta de cotações de ações em tempo real desenvolvido em C# .NET 9.0. O aplicativo monitora continuamente o preço de uma ação específica e envia alertas por email quando os valores de compra ou venda são atingidos.

## Funcionalidades

- **Monitoramento em tempo real** de cotações via API da BrAPI
- **Alertas inteligentes** com lógica anti-spam para compra e venda
- **Notificações por email** automáticas 
- **Console logging** com timestamp das consultas
- **Configuração flexível** via arquivo JSON

## Tecnologias Utilizadas

- **System.Net.Http** - Cliente HTTP para requisições à API
- **System.Net.Mail** - Envio de emails via SMTP
- **System.Text.Json** - Serialização/deserialização JSON
- **BrAPI** - API de cotações do mercado brasileiro


## 🚀 Como Executar

### 1. Pré-requisitos

- **.NET 9.0 SDK** ou superior
- Conta de email com SMTP habilitado (Gmail recomendado)
- Token da BrAPI (opcional, mas recomendado para mais requisições)

### 2. Clonando o Projeto

```bash
git clone https://github.com/Petralanda/statusQuoteAlert-Inoa.git
cd statusQuoteAlert-Inoa
```

### 3. Configuração

#### 3.1. Configure o arquivo `config.json`

Copie o template e configure com suas credenciais:

```bash
cp config.template.json config.json
```

Edite o arquivo `config.json` com suas informações reais:

```json
{
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true, 
    "UseDefaultCredentials": false,
    "FromAddress": "seu_email@gmail.com",
    "Password": "sua_senha_de_app",
    "ToAddress": "destinatario@gmail.com",
    "brapi_token": "seu_token_brapi"
}
```

#### 3.2. Configuração do Gmail (Recomendado)

1. Ative a **verificação em duas etapas** na sua conta Google
2. Gere uma **senha de app** específica para este aplicativo:
   - Acesse: [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)
   - Crie uma senha de app para "Email"
   - Use essa senha no campo `Password` do config.json

#### 3.3. Token BrAPI (Opcional)

- Sem token: 100 requisições/dia
- Com token: Até 10.000 requisições/dia
- Obtenha seu token em: [brapi.dev](https://brapi.dev)

### 4. Instalação de Dependências

```bash
dotnet restore
```

### 5. Compilação

```bash
dotnet build
```


### Execução Básica

```bash
dotnet run <TICKET> <preço_compra> <preço_venda>
```

### Exemplos Práticos

```bash
# Monitorar PETR4 - comprar a R$ 22.50 ou vender a R$ 25.00
dotnet run PETR4 22.50 25.00

# Monitorar VALE3 - comprar a R$ 65.00 ou vender a R$ 70.00  
dotnet run VALE3 65.00 70.00

# Monitorar ITUB4 - comprar a R$ 32.00 ou vender a R$ 35.50
dotnet run ITUB4 32.00 35.50
```

### Parâmetros

- **TICKET**: Código da ação (ex: PETR4, VALE3, ITUB4)
- **preço_compra**: Preço limite superior para alerta de compra
- **preço_venda**: Preço limite inferior para alerta de venda

## 📊 Lógica de Funcionamento

### Fluxo Principal

1. **Inicialização**: Carrega configurações do `config.json`
2. **Validação**: Verifica parâmetros de entrada
3. **Monitoramento**: Loop infinito consultando a API a cada 10 segundos
4. **Análise**: Compara preço atual com limites definidos
5. **Alertas**: Envia emails quando condições são atendidas

### Sistema Anti-Spam

O aplicativo implementa uma lógica inteligente para evitar múltiplos alertas:

#### Alerta de Compra (Preço Alto)
- ✅ **Dispara**: Quando preço >= valor de compra (primeira vez)
- 🚫 **Bloqueia**: Novos alertas enquanto preço permanece alto
- 🔄 **Reativa**: Quando preço cai abaixo do valor de compra

#### Alerta de Venda (Preço Baixo)  
- ✅ **Dispara**: Quando preço <= valor de venda (primeira vez)
- 🚫 **Bloqueia**: Novos alertas enquanto preço permanece baixo
- 🔄 **Reativa**: Quando preço sobe acima do valor de venda

### Estados do Sistema

```
Estado Inicial: Monitorando
    ├── Preço >= Compra → Alerta Compra Enviado
    │   └── Preço < Compra → Volta ao Monitoramento
    └── Preço <= Venda → Alerta Venda Enviado  
        └── Preço > Venda → Volta ao Monitoramento
```

## 📁 Estrutura do Projeto

```
statusQuoteAlert-Inoa/
├── Program.cs              # Código principal da aplicação
├── config.json            # Arquivo de configuração
├── statusQuoteAlert-Inoa.csproj  # Arquivo de projeto .NET
├── README.md              # Este arquivo
├── bin/                   # Executáveis compilados
└── obj/                   # Arquivos temporários de build
```

## 🔍 Detalhamento do Código

### Classes Principais

#### `PriceTimeData`
```csharp
public class PriceTimeData
{
    public float Price { get; set; }
    public string Time { get; set; }
}
```
Armazena dados de preço e timestamp da consulta.

#### `Configs`
```csharp
public class Configs
{
    public string Host { get; set; }           // Servidor SMTP
    public int Port { get; set; }              // Porta SMTP
    public bool EnableSsl { get; set; }        // SSL habilitado
    public bool UseDefaultCredentials { get; set; }
    public string FromAddress { get; set; }    // Email remetente
    public string Password { get; set; }       // Senha/token do email
    public string ToAddress { get; set; }      // Email destinatário
    public string brapi_token { get; set; }    // Token da BrAPI
}
```
Contém todas as configurações do sistema.

### Métodos Principais

#### `getConfigs()`
Carrega e deserializa as configurações do arquivo JSON.

#### `sendEmail(string subject, string body)`
Envia emails via SMTP usando as configurações definidas.

#### `getMarketPrice(string ticket, string token)`
Consulta a API da BrAPI e retorna os dados da cotação atual.

#### `Main(string[] args)`
Método principal que coordena todo o fluxo da aplicação.

## 📧 Formato dos Emails

### Email de Alerta de Compra
```
Assunto: Alerta de Compra - PETR4
Corpo: Ação PETR4 atingiu o valor de compra: R$ 22.50
```

### Email de Alerta de Venda
```
Assunto: Alerta de Venda - PETR4  
Corpo: Ação PETR4 atingiu o valor de venda: R$ 25.00 em 2025-11-10T14:30:00-03:00
```

## 🐛 Tratamento de Erros

- **Arquivo de configuração inválido**: Exceção com mensagem clara
- **Falha na API**: Log de erro e nova tentativa automática
- **Erro no envio de email**: Log detalhado da exceção
- **Parâmetros inválidos**: Mensagem de uso correto

## Performance

- **Intervalo de consulta**: 10 segundos (configurável no código)
- **Timeout de requisições**: Padrão do HttpClient
- **Uso de memória**: Baixo, sem acúmulo de dados históricos
- **Rate limiting**: Respeitado pelos intervalos de consulta


## Limitações Conhecidas

- Dependente de conexão com internet
- Rate limits da API BrAPI (sem token: 100 req/dia)
- Não persiste dados históricos
- Execução em thread única

## Exemplo de Saída no Console

```
Monitorando PETR4 - Compra: R$ 22.50 | Venda: R$ 25.00
[0] PETR4 = R$ 23.15 (Horário: 2025-11-10T14:25:00-03:00)
[1] PETR4 = R$ 23.20 (Horário: 2025-11-10T14:25:10-03:00)
[2] PETR4 = R$ 22.50 (Horário: 2025-11-10T14:25:20-03:00)
ALERTA DE COMPRA ENVIADO
[3] PETR4 = R$ 22.45 (Horário: 2025-11-10T14:25:30-03:00)
```
