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

            Console.WriteLine($"[CartaoEmissor] Proposta {proposta.Id} recebida com status {proposta.Status} e score {proposta.Score}, emitindo plastico");
            if (proposta.Status != PropostaStatus.Aprovada || proposta.Score <= 100)
                throw new ArgumentException($"[CartaoEmissor] Proposta {proposta.Id} nao esta aprovada ou tem score insuficiente");
            
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
                throw new ArgumentException($"[CartaoEmissor] Cliente {proposta.ClienteId} já tem a quantidade máximo de cartões emitidos");

            if (quantidade == 2 && existentes.Count() == 1)
            {
                Console.WriteLine($"[CartaoEmissor] Reajustando quantidade de cartoes a emitir");
                quantidade = 1;
                limite = 1000m;
            }

            if (quantidade <= 0 || limite <= 0m)
                throw new ArgumentException($"[CartaoEmissor] Proposta {proposta.Id} tem score invalido para determinar produtos selecionados");

            Console.WriteLine($"[CartaoEmissor] Criando {quantidade} cartao(oes) com {limite} de limite alocado ");
            for (int i = 1; i <= quantidade; i++)
            {
                try
                {
                    Console.WriteLine($"[CartaoEmissor] Gerando plastico {i} de {quantidade} com limite de {limite}");
                    var plastico = CartaoMock.GerarCartao();

                    Console.WriteLine($"[CartaoEmissor] Plastico gerado : {plastico.Numero}");
                    var cartao = new Cartao
                    {
                        ClienteId = proposta.ClienteId,
                        PropostaId = proposta.Id,
                        Limite = limite,
                        Numero = plastico.Numero,
                        Bandeira = plastico.Bandeira,
                        Validade = plastico.Validade,
                    };
                    Console.WriteLine(cartao);
                    repoCartao.Insert(cartao);
                    Console.WriteLine($"[CartaoEmissor] Cartao {cartao.Numero} gerado para proposta {proposta.Id}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " " + e.StackTrace);
                }
            }
        }
    }
}
