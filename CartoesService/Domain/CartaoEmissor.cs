using Shared.Domain;
using Shared.Mock;
using Data.Repository;
using SQLitePCL;

namespace CartoesService.Domain
{
    public class CartaoEmissor
    {
        private readonly CartaoRepository repoCartao;

        public CartaoEmissor(CartaoRepository repoCartao)
        {
            this.repoCartao = repoCartao;
        }

        public void Emitir(Proposta proposta)
        {
            int quantidade = 0;
            decimal limite = 0m;

            if (proposta.Status != PropostaStatus.Aprovada)
                throw new ArgumentException($"[CartaoService] Proposta {proposta.Id} nao esta aprovada");

            if (proposta.Score <= 100)
                throw new ArgumentException($"[CartaoService] Proposta {proposta.Id} com score insuficiente");

            Console.WriteLine($"[CartaoService] Proposta {proposta.Id} aprovada, gerando cartoes");
            if (proposta.Score >= 101 && proposta.Score <= 500)
            {
                quantidade = 1;
                limite = 1000m;
            }
            else
            {
                quantidade = 2;
                limite = 5000m;
            }

            var existentes = repoCartao.GetByCliente(proposta.ClienteId).ToList();
            if (existentes.Count() >= 2)
                throw new ArgumentException($"[CartaoService] Cliente {proposta.ClienteId} já tem a quantidade máximo de cartões emitidos");

            if (quantidade == 2 && existentes.Count() == 1)
            {
                Console.WriteLine($"[CartaoService] Reajustando quantidade de cartoes a emitir");
                quantidade = 1;
                limite = 1000m;
            }

            if (quantidade <= 0 || limite <= 0m)
                throw new ArgumentException($"[CartaoService] Proposta {proposta.Id} tem score invalido para determinar produtos selecionados");

            for (int i = 0; i < quantidade; i++)
            {
                var plastico = CartaoMock.GerarCartao();

                var cartao = new Cartao
                {
                    ClienteId = proposta.ClienteId,
                    PropostaId = proposta.Id,
                    Numero = plastico.Numero,
                    Bandeira = plastico.Bandeira,
                    Validade = plastico.Validade,
                    Limite = limite,
                };
                repoCartao.Insert(cartao);
                Console.WriteLine($"[CartaoService] Cartao {cartao.Numero} gerado para proposta {proposta.Id}");
            }
        }
    }
}
