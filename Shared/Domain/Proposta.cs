using Shared.Domain;

namespace Shared.Domain
{
    public class Proposta
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClienteId { get; set; } 

        public PropostaStatus Status { get; set; } = PropostaStatus.EmAnalise;

        public int Score { get; set; } = 0;

        public decimal Valor { get; set; } = 0m;

        public DateTime Criado { get; set; } = DateTime.Now;

        public DateTime? Alterado { get; set; } = null;
    }
}
