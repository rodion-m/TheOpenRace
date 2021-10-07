using Ardalis.Specification;
using OpenRace.Entities;

namespace OpenRace.Data.Specifications
{
    public sealed class MemberByPaymentId : Specification<Member>, ISingleResultSpecification
    {
        public MemberByPaymentId(string paymentId)
        {
            Query.Where(x => x.Payment!.Id == paymentId);
            Query.Include(it => it.Payment);
        }
    }
    public sealed class MemberByPaymentHash : Specification<Member>, ISingleResultSpecification
    {
        public MemberByPaymentHash(string paymentHash)
        {
            Query.Where(x => x.Payment!.Hash == paymentHash);
            Query.Include(it => it.Payment);
        }
    }
    
    public sealed class MemberByEmailAndName : Specification<Member>, ISingleResultSpecification
    {
        public MemberByEmailAndName(string email, string fullName)
        {
            Query.Where(x => x.Email == email && x.FullName == fullName);
            Query.Include(it => it.Payment);
        }
    }
    
    public sealed class MemberByEmail : Specification<Member>, ISingleResultSpecification
    {
        public MemberByEmail(string email)
        {
            Query.Where(x => x.Email == email);
            Query.Include(it => it.Payment);
        }
    }
}