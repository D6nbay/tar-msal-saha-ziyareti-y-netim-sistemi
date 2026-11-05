namespace SahaZiyaret.Api.Exceptions;


/*Kurala uymayınca hata fırlatır. Mesajı kullanıcıya döndürür. */
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}