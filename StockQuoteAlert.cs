using System.Text.Json;  // Para JsonSerializer
using System.Net;  // Para NetworkCredential
using System.Net.Mail;
using System.Text.Json.Nodes; // parse da resposta json da requisicao
using System.Globalization; // garantir que o buy e o sell passando em args seja padrao global

class StockQuoteAlert
{
        public class Configs
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public bool UseDefaultCredentials { get; set; } = false;
        public string FromAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ToAddress { get; set; } = string.Empty;
        public string brapi_token { get; set; } = string.Empty;
        public long setTimeOut { get; set; } = 10000;
    }   
    public static Configs GetConfigs()
    {
        string jsonString = File.ReadAllText("config.json");
        var cfg = JsonSerializer.Deserialize<Configs>(jsonString);
        if (cfg == null) 
        {
            throw new InvalidOperationException("config.json inválido.");
        }
        return cfg;
    }

    static async Task SendEmail(string subject, string body)
    {
        Configs config = GetConfigs();

        using (var smtp = new SmtpClient())
        {
            smtp.Host = config.Host;
            smtp.Port = config.Port;
            smtp.EnableSsl = config.EnableSsl;
            smtp.UseDefaultCredentials = config.UseDefaultCredentials;
            smtp.Credentials = new NetworkCredential(config.FromAddress, config.Password);

            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress(config.FromAddress, "Stock Quote Alert");
                msg.To.Add(config.ToAddress);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = false;

                try
                {
                    await smtp.SendMailAsync(msg);
                    Console.Write("Email de ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                    throw;
                }
            }
        }
    }
    static async Task<float> GetMarketPrice(string ticket, string token = "")
    {
        string url = $"https://brapi.dev/api/quote/{ticket}";
        if (!string.IsNullOrEmpty(token)) url += $"?token={token}";

        using (var client = new HttpClient())
        {
            try
            {
                string json = await client.GetStringAsync(url);

                var root = JsonNode.Parse(json)!;

                var result = root["results"]![0]!;
                float price = result["regularMarketPrice"]!.GetValue<float>();

                return price;
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"Erro: {e.Message}");
                return -1;
            }
        }
    }

    static async Task Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Uso correto: stock-quote-alert.exe <TICKET> <preco_compra> <preco_venda>");
            return;
        }
        string ticket = args[0]; 
        float sell = float.Parse(args[1], CultureInfo.InvariantCulture);
        float buy = float.Parse(args[2], CultureInfo.InvariantCulture);

        var config = GetConfigs();

        string token = config.brapi_token;
        long setTimeOut = config.setTimeOut;
        bool buyAlertSent = false;   
        bool sellAlertSent = false; 
        string body = "";
        int i = 1;

        Console.WriteLine($"Monitorando {ticket} - venda: R$ {sell:F2} | compra: R$ {buy:F2}");

        while (true)
        {
            var price = await GetMarketPrice(ticket, token);
            
            if(price == -1) {
                Console.WriteLine("Erro ao obter a cotação. Tentando novamente..."); // debug
                break;
            }
            
            Console.WriteLine($"[{i++} Tentativa] {ticket} = R$ {price:F2} ");

            // logica para alerta de compra (preço baixo, oportunidade de compra)
            if(price < buy && !buyAlertSent)
            {
                body = $"Ação {ticket} está com preço baixo para compra: R$ {price:F2}, dado o limite de compra {buy:F2}";
                await SendEmail($"Alerta de Compra - {ticket}", body);
                Console.WriteLine("compra enviado com sucesso");
                buyAlertSent = true;
            }
            // logica para alerta de venda (preço alto, oportunidade de venda)
            if(price > sell && !sellAlertSent)
            {
                body = $"Ação {ticket} está com preço alto para venda: R$ {price:F2}, dado o limite de venda {sell:f2}";
                await SendEmail($"Alerta de Venda - {ticket}", body);
                Console.WriteLine("venda enviado com sucesso");
                sellAlertSent = true;
            }
            
            // reseta o flag de compra quando o preço volta para cima do limite
            if (buyAlertSent && price >= buy)
            {
                buyAlertSent = false;
            }
            
            // reseta o flag de venda quando o preço volta para baixo do limite
            if (sellAlertSent && price <= sell)
            {
                sellAlertSent = false;
            }
            
            await Task.Delay(TimeSpan.FromMilliseconds(setTimeOut)); // aguarda setTimeOut em ms ate a proxima tentativa
        }

    }
}
