using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TechQuestions.Helper
{
    public static class TokenHelper
    {
        //public static string GenerateJwtToken(User input)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var tokenKey = Encoding.UTF8.GetBytes("LongSecrectStringForModulekodestartppopopopsdfjnshbvhueFGDKJSFBYJDSAGVYKDSGKFUYDGASYGFskc#$vhHJVCBYHVSKDGHASVBCL");
        //    var tokenDescriptior = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(new Claim[]
        //        {
        //                new Claim("Email",input.Email),
        //                new Claim("UserId",input.Id.ToString()),
        //                new Claim("Type",input.UserType.ToString()),
        //                new Claim("ClinicId",input.Clinic != null ? input.Clinic.Id.ToString():""),
        //                new Claim("Name",input.FirstName + " " + input.LastName)
        //        }),
        //        Expires = DateTime.Now.AddHours(9),
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey)
        //        , SecurityAlgorithms.HmacSha256Signature)
        //    };
        //    var token = tokenHandler.CreateToken(tokenDescriptior);
        //    return tokenHandler.WriteToken(token);
        //}

        //public static bool IsValidToken(string tokenString) //Decode
        //{
        //    String toke = "Bearer " + tokenString;
        //    var jwtEncodedString = toke.Substring(7);
        //    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
        //    if (token.ValidTo > DateTime.UtcNow)
        //    {
        //        //Read Cliamis 
        //        int userId = int.Parse((token.Claims.First(c => c.Type == "UserId").Value.ToString()));
        //        //valid
        //        return true;
        //    }
        //    return false;
        //}
    }
}
