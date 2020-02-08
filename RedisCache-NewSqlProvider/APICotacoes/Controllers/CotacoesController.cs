using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace APICotacoes.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CotacoesController : ControllerBase
    {
        [HttpGet]
        public ContentResult Get(
            [FromServices]IConfiguration config,
            [FromServices]IDistributedCache cache)
        {
            string valorJSON = cache.GetString("Cotacoes");
            if (valorJSON == null)
            {
                using var conexao = new SqlConnection(
                    config.GetConnectionString("BaseCotacoes"));

                using var cmd = conexao.CreateCommand();
                cmd.CommandText =
                    "SELECT Sigla " +
                          ",NomeMoeda " +
                          ",UltimaCotacao " +
                          ",ValorComercial AS 'Cotacoes.Comercial' " +
                          ",ValorTurismo AS 'Cotacoes.Turismo' " +
                    "FROM dbo.Cotacoes " +
                    "ORDER BY NomeMoeda " +
                    "FOR JSON PATH, ROOT('Moedas')";

                conexao.Open();
                valorJSON = (string)cmd.ExecuteScalar();
                conexao.Close();

                DistributedCacheEntryOptions opcoesCache =
                          new DistributedCacheEntryOptions();
                opcoesCache.SetAbsoluteExpiration(
                    TimeSpan.FromMinutes(1));

                cache.SetString("Cotacoes", valorJSON, opcoesCache);
            }

            return Content(valorJSON, "application/json");
        }
    }
}