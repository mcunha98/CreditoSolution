using Shared.Domain;

namespace PropostasService.Domain
{
    public static class PropostaScoreService
    {
        /// <summary>
        /// Realiza o calculo do score baseado na idade e renda do cliente
        /// </summary>
        /// <param name="cliente"></param>
        /// <returns></returns>
        public static int Calculate(Cliente cliente)
        {
            if (cliente.Nascimento == null || cliente.Renda <= 0m)
                return 0;

            int idade = Idade((DateTime)cliente.Nascimento);
            decimal renda = cliente.Renda;

            int score = 0;
            if (idade < 21) 
                score += 5;
            else if (idade <= 30) 
                score += 10;
            else if (idade <= 50) 
                score += 20;
            else 
                score += 10;

            if (renda < 1000) 
                score += 10;
            else if (renda < 2500) 
                score += 300;
            else if (renda < 5000) 
                score += 750;
            else 
                score += 70;

            if (renda < 800 && idade < 25) 
                score -= 10;

            if (score < 0) 
                score = 0;
            
            if (score > 1000) 
                score = 1000;

            return score;
        }

        private static int Idade(DateTime nascimento)
        {
            var hoje = DateTime.Today;
            int idade = hoje.Year - nascimento.Year;
            if (nascimento.Date > hoje.AddYears(-idade)) idade--;
            return idade;
        }
    }
}
