using System.Text.Json.Nodes; // parse da resposta json da requisicao

class Program
{
    static async Task<SortedDictionary<string, float>> getStockSeries(string ticket, string token = "")
    {
        string url = $"https://brapi.dev/api/quote/{ticket}";
        if (!string.IsNullOrEmpty(token)) url += $"?token={token}";

        using HttpClient client = new HttpClient();

        try
        {
            string json = await client.GetStringAsync(url);

            var root = JsonNode.Parse(json)!;

            var result = root["results"]![0]!;
            float price = result["regularMarketPrice"]!.GetValue<float>();
            string time = result["regularMarketTime"]!.GetValue<string>();

            SortedDictionary<string, float> res = new SortedDictionary<string, float>();
            res.Add(time, price);

            return res;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erro: {e.Message}");
            return null;
        }
    }

    // static SortedDictionary<string, float> getMaxMin(SortedDictionary<string, float> map)
    // {
        
    // }

    static async Task Main()
    {
        string ticket = "PETR4";
        string token = "";
        int i = 0;
        SortedDictionary<string, float> stockSerie = new SortedDictionary<string, float>();

        while (i < 10)
        {
            var res = await getStockSeries(ticket, token);
            foreach (var kv in res)
            {
                // se já existir a mesma timestamp, atualiza o preço
                stockSerie[kv.Key] = kv.Value;
            }
            i++;
        }
        foreach (var kv in stockSerie)
        {
            Console.WriteLine($"[{kv.Key}] {ticket} = {kv.Value}");
        }

    }
}
