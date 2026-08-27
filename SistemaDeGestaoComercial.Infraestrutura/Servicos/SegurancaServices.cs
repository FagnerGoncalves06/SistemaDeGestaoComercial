using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SistemaDeGestaoComercial.Aplicacao.Contratos;
using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Infraestrutura.Servicos;

internal sealed class SenhaService : ISenhaService
{
    private const int Iteracoes = 210_000;

    public string Hash(string senha)
    {
        Validar(senha);
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA512, 32);
        return $"{Iteracoes}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string senha, string armazenado)
    {
        var partesHash = armazenado.Split('.');
        if (partesHash.Length != 3 || !int.TryParse(partesHash[0], out var iteracoesArmazenadas))
            return false;
        try
        {
            var salt = Convert.FromBase64String(partesHash[1]);
            var hashEsperado = Convert.FromBase64String(partesHash[2]);
            return CryptographicOperations.FixedTimeEquals(
                hashEsperado,
                Rfc2898DeriveBytes.Pbkdf2(
                    senha,
                    salt,
                    iteracoesArmazenadas,
                    HashAlgorithmName.SHA512,
                    hashEsperado.Length
                )
            );
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static void Validar(string senha)
    {
        if (senha.Length < 8 || !senha.Any(char.IsUpper) || !senha.Any(char.IsLower) || !senha.Any(char.IsDigit))
            throw new ExcecaoDominio("Senha deve ter ao menos 8 caracteres, maiúscula, minúscula e número.");
    }
}

internal sealed class TokenService(IConfiguration configuracao) : ITokenService
{
    public LoginDto Criar(Usuario usuario)
    {
        var chave = configuracao["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
        if (Encoding.UTF8.GetByteCount(chave) < 32)
            throw new InvalidOperationException("Jwt:Key deve possuir pelo menos 32 bytes.");
        var expira = DateTime.UtcNow.AddHours(1);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim("token_version", usuario.VersaoToken.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        var token = new JwtSecurityToken(
            configuracao["Jwt:Issuer"],
            configuracao["Jwt:Audience"],
            claims,
            expires: expira,
            signingCredentials: new(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
                SecurityAlgorithms.HmacSha256
            )
        );
        return new(new JwtSecurityTokenHandler().WriteToken(token), expira, usuario.Nome, usuario.Perfil);
    }
}
