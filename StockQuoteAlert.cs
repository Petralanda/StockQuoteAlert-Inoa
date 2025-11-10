using System.Text.Json;  // Para JsonSerializer
using System.Net;  // Para NetworkCredential
using System.Net.Mail;
using System.Text.Json.Nodes; // parse da resposta json da requisicao

class StockQuoteAlert
{
    public class PriceTimeData
    {
        public float Price { get; set; }
        public string Time { get; set; } = string.Empty;
        public PriceTimeData(float price, string time)
        {
            Price = price;
            Time = time;
        }
    }

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
    }   
    public static Configs getConfigs()
    {
        string jsonString = File.ReadAllText("config.json");
        var cfg = JsonSerializer.Deserialize<Configs>(jsonString);
        if (cfg == null) throw new InvalidOperationException("config.json inválido.");
        return cfg;
    }

    static async Task sendEmail(string subject, string body)
    {
        Configs config = getConfigs();

        using (var smtp = new SmtpClient())
        {
            smtp.Host = config.Host;
            smtp.Port = config.Port;
            smtp.EnableSsl = config.EnableSsl;
            smtp.UseDefaultCredentials = config.UseDefaultCredentials;
            smtp.Credentials = new NetworkCredential(config.FromAddress, config.Password);

            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress(config.FromAddress, "Alert-Status-Quote");
                msg.To.Add(config.ToAddress);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = false;

                try
                {
                    await smtp.SendMailAsync(msg);
                    Console.WriteLine("Email enviado com sucesso.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                    throw;
                }
            }
        }
    }
    static async Task<PriceTimeData?> getMarketPrice(string ticket, string token = "")
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
                string time = result["regularMarketTime"]!.GetValue<string>();

                return new PriceTimeData(price, time);
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"Erro: {e.Message}");
                return null;
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
        float buy = float.Parse(args[1]);
        float sell = float.Parse(args[2]);

        var config = getConfigs();
        
        string token = config.brapi_token;
        bool hasReachedBuy = false; 
        bool hasReachedSell = false; 
        bool buyAlertSent = false;   
        bool sellAlertSent = false; 
        string body = "";
        int i = 0;

        Console.WriteLine($"Monitorando {ticket} - Compra: R$ {buy:F2} | Venda: R$ {sell:F2}");

        while (true)
        {
            var result = await getMarketPrice(ticket, token);
            
            if(result == null) {
                Console.WriteLine("Erro ao obter a cotação. Tentando novamente...");
                break;
            }
            
            Console.WriteLine($"[{i++}] {ticket} = R$ {result.Price:F2} (Horário: {result.Time})");

            // logica para alerta de compra
            if(result.Price >= buy)
            {
                hasReachedBuy = true;
                if (!buyAlertSent)
                {
                    body = $"Ação {ticket} atingiu o valor de compra: R$ {result.Price:F2}";
                    // await sendEmail($"Alerta de Compra - {ticket}", body);
                    Console.WriteLine($"ALERTA DE COMPRA ENVIADO");
                    buyAlertSent = true;
                    sellAlertSent = false; 
                }
            }
            // so pode enviar novo alerta de compra se o preço subir novamente apos ter caido abaixo do valor de compra
            else if (hasReachedBuy && result.Price < buy)
            {
                buyAlertSent = false;
            }
            // logica para alerta de venda
            if(result.Price <= sell)
            {
                hasReachedSell = true;
                if (!sellAlertSent)
                {
                    body = $"Ação {ticket} atingiu o valor de venda: R$ {result.Price:F2} em {result.Time}";
                    // await sendEmail($"Alerta de Venda - {ticket}", body);
                    Console.WriteLine("ALERTA DE VENDA ENVIADO");
                    sellAlertSent = true;
                    buyAlertSent = false;
                }
            }
            // so pode enviar novo alerta de venda se o preço descer novamente apos ter subido acima do valor de venda
            else if (hasReachedSell && result.Price > sell)
            {
                sellAlertSent = false;
            }
            
            await Task.Delay(10000); // aguarda 10 segundos antes da próxima consulta para evitar limite de requisicoes
        }

    }
}
