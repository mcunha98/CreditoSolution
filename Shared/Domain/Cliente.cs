namespace Shared.Domain
{
    public class Cliente
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public DateTime? Nascimento { get; set; } = null;

        private decimal _renda;

        public ClienteSegmento Segmento { get; set; } = ClienteSegmento.BaixaRenda;
        public DateTime Criado { get; set; } = DateTime.Now;
        public DateTime? Alterado { get; set; } = null;

        public decimal Renda
        {
            get => _renda;
            set
            {
                _renda = value;
                if (value < 500m)
                    Segmento = ClienteSegmento.BaixaRenda;
                else if (value >= 500m && value <= 1200m)
                    Segmento = ClienteSegmento.MediaRenda;
                else
                    Segmento = ClienteSegmento.AltaRenda;
                if (value <= 0)
                    Segmento = ClienteSegmento.DevedoresDuvidosos;
            }
        }
    }
}
