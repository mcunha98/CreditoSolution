using Shared.Domain;

namespace ClientesService.Domain
{
    public static class ClienteValidator
    {
        public static void Validate(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nome))
                throw new ArgumentException("O nome do cliente é obrigatório");

            if (string.IsNullOrWhiteSpace(cliente.Cpf))
                throw new ArgumentException("O CPF do cliente é obrigatório");

            if (cliente.Cpf.Length != 11 || !cliente.Cpf.All(char.IsDigit))
                throw new FormatException("Formato de CPF inválido, deve conter 11 dígitos numéricos");

            if (!ValidadeCPF(cliente.Cpf))
                throw new ArgumentException("O CPF informado é inválido");

            if (cliente.Nascimento == null)
                throw new ArgumentNullException("Data de nascimento é obrigatória");

            if (cliente.Nascimento.Value > DateTime.Today.AddYears(-18))
                throw new ArgumentException("O cliente deve ter pelo menos 18 anos");
        }

        private static bool ValidadeCPF(string cpf)
        {
            // Na formula matematica eles são validos, mas na real sao cpfs invalidos.
            var cpfsInvalidos = new string[]
            {
                "11111111111",
                "22222222222",
                "33333333333",
                "44444444444",
                "55555555555",
                "66666666666",
                "77777777777",
                "88888888888",
                "99999999999"
            };

            if (string.IsNullOrEmpty(cpf))
                return false;

            if (cpfsInvalidos.Contains(cpf))
                return false;

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            if (cpf.Length != 11)
                return false;
            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf += digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito += resto.ToString();
            return cpf.EndsWith(digito);
        }
    }
}
