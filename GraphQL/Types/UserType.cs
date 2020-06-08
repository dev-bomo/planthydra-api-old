using api.Models;
using GraphQL.Types;

namespace api.GraphQL.Types
{
    class UserType : ObjectGraphType<User>
    {
        public UserType()
        {
            Field(u => u.Id);
            Field(u => u.UserName);
            Field(u => u.Email);
        }
    }
}