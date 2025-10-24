using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Mock
{
    public class ClienteMock
    {
        /// <summary>
        /// Gera um nome completo aleatório
        /// </summary>
        /// <returns></returns>
        public static string GerarNome()
        {
            string[] nomes = { "João", "Maria", "Pedro", "Ana", "Carlos", "Sofia", "Lucas", "Laura", "Gustavo", "Camila",
                   "Fernando", "Isabela", "Bruno", "Larissa", "Diego", "Mariana", "Rafael", "Juliana", "Felipe", "Carolina",
                   "Rodrigo", "Beatriz", "Alexandre", "Vanessa", "Gabriel", "Tatiana", "Vinícius", "Aline", "Marcelo", "Patrícia",
                   "Daniel", "Renata", "Leonardo", "Manuela", "Guilherme", "Luana", "Ricardo", "Monica", "Feliciano", "Carla",
                   "Eduardo", "Letícia", "Samuel", "Paula", "Thiago", "Amanda", "Marcos", "Renata", "Jorge", "Valentina",
                   "Fábio", "Fernanda", "Ronaldo", "Adriana", "Marcos", "Roberta", "André", "Cristina", "Milton", "Larissa" };

            string[] sobrenomes = { "Silva", "Santos", "Oliveira", "Pereira", "Ferreira", "Rocha", "Carvalho", "Souza", "Lima", "Costa",
                        "Rodrigues", "Machado", "Gomes", "Almeida", "Nascimento", "Cavalcanti", "Cardoso", "Lopes", "Ribeiro", "Martins",
                        "Fernandes", "Barbosa", "Correia", "Campos", "Dias", "Castro", "Araújo", "Moraes", "Bezerra",
                        "Gonçalves", "Ramos", "Rocha", "Barros", "Azevedo", "Lins", "Garcia", "Coutinho", "Tavares",
                        "Santana", "Oliveira", "Mendonça", "Melo", "Cruz", "Leite", "Macedo", "Freitas", "Vasconcelos", "Cavalcante",
                        "Dantas", "Sales", "Duarte", "Gouveia", "Lobo", "Moura", "Figueiredo", "Farias" };

            Random random = new Random();
            return nomes[random.Next(nomes.Length)] + " " + sobrenomes[random.Next(sobrenomes.Length)] + " " + sobrenomes[random.Next(sobrenomes.Length)];
        }

        /// <summary>
        /// Gera um valor de renda aleatório
        /// </summary>
        /// <param name="minimo"></param>
        /// <param name="maximo"></param>
        /// <returns></returns>
        public static decimal GerarRenda(int minimo = 0, int maximo = 2500)
        {
            if (minimo < 1 || minimo > maximo) minimo = 1;
            if (maximo > 100000) maximo = 100000;
            Random random = new Random();
            return random.Next(minimo, maximo) + (random.Next(0, 100) / 100m);
        }

        /// <summary>
        /// Gera um CPF válido aleatório
        /// </summary>
        /// <returns></returns>
        public static string GerarCPF()
        {
            int soma = 0, resto = 0;
            int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

            Random rnd = new Random();
            string semente = rnd.Next(100000000, 999999999).ToString();

            for (int i = 0; i < 9; i++)
                soma += int.Parse(semente[i].ToString()) * multiplicador1[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            semente = semente + resto;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(semente[i].ToString()) * multiplicador2[i];

            resto = soma % 11;

            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            semente = semente + resto;
            return semente;
        }

        public static DateTime GerarNascimento(int minimo = 18, int maximo = 85)
        {
            if (minimo < 18) 
                minimo = 18;

            if (maximo > 85)
                maximo = 85;

            Random random = new Random();
            int idade = random.Next(minimo, maximo + 1);
            DateTime dataNascimento = DateTime.Now.AddYears(-idade);
            int diasAdicionais = random.Next(0, 365);
            dataNascimento = dataNascimento.AddDays(-diasAdicionais);
            return dataNascimento.Date;
        }

        public static T EnumRandom<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(new Random().Next(values.Length))!;
        }

    }
}
