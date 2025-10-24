using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Shared.Domain
{
    public class Cartao
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClienteId { get; set; } = Guid.Empty;

        public Guid PropostaId { get; set; } = Guid.Empty;

        public string Numero { get; set; } = string.Empty;
        
        public string Bandeira { get; set; } = string.Empty;

        public string Validade { get; set; } = string.Empty;

        public decimal Limite { get; set; } = 0m;

        public DateTime Criado { get; set; } = DateTime.Now;

        public DateTime? Alterado { get; set; } = null;
    }
}
