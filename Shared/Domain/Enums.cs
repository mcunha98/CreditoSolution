using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Domain
{
    public enum ClienteSegmento
    {
        BaixaRenda,
        MediaRenda,
        AltaRenda,
        DevedoresDuvidosos,
    }

    public enum PropostaStatus
    {
        EmAnalise = 0,
        Aprovada = 1,
        Rejeitada = 9,
    }

    public enum CartaoBandeira
    {
        Visa,
        MasterCard,
    }
}
