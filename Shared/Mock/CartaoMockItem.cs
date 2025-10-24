namespace Shared.Mock
{
    public class CartaoMockItem
    {
        public string Numero { get; set; } = string.Empty;
        public string Ano { get; set; } = string.Empty;
        public string Mes { get; set; } = string.Empty;
        public string CodigoSeguranca { get; set; } = string.Empty;
        public string Bin { get; set; } = string.Empty;
        public string Bandeira { get; set; } = string.Empty;
        public string Validade
        {
            get
            {
                return string.Concat(Ano,Mes);
            }
        }

        public CartaoMockItem()
        {
            Random random = new Random();
            Ano = (DateTime.Now.Year + random.Next(2, 5)).ToString();
            Mes = random.Next(1, 12).ToString("00");
            CodigoSeguranca = random.Next(111, 999).ToString();
        }
    }
}
