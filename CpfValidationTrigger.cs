using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace Vinn.Function
{
    public static class CpfValidationTrigger
    {
        [FunctionName("CpfValidationTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Trigger iniciada");

            string cpf = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(cpf))
            {
                return new BadRequestObjectResult("É necessário informar o cpf para validação");
            }

            string responseMessage = IsCpfValid(cpf) ? "Válido" : "Inválido";

            return new OkObjectResult(responseMessage);
        }

        private static bool IsCpfValid(string cpf)
        {
            cpf = CleanCpf(cpf);

            if (cpf.Length != 11 || !cpf.All(char.IsDigit))
                return false;

            var total = cpf.Take(9).Select((c, index) => (c - '0') * (index + 1)).Sum();

            var firstDigitCheck = (total % 11).ToString().Last();

            total = cpf.Take(9).Append(firstDigitCheck).Select((c, index) => (c - '0') * index).Sum();

            var secondDigitCheck = (total % 11).ToString().Last();

            return firstDigitCheck == cpf.ElementAt(9) && secondDigitCheck == cpf.ElementAt(10);
        }

        private static string CleanCpf(string cpf)
        {
            return cpf.Replace("-", "").Replace(".", "");
        }
    }
}
